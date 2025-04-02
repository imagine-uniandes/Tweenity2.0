using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Models.Nodes;
using System.Linq;
using System;
using Views.MiddlePanel;
using Models;
using UnityEditor;

namespace Views
{
    public class TweenityGraphView : GraphView
    {
        public Action<TweenityNodeModel> OnNodeSelected;
        private GridBackground gridBackground;

        public TweenityGraphView()
        {
            this.style.flexGrow = 1;
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            gridBackground = new GridBackground();
            this.Insert(0, gridBackground);
        }

        public void RenderNode(TweenityNodeModel nodeModel, Rect? positionOverride = null)
        {
            EditorApplication.delayCall += () =>
            {
                TweenityNode visualNode = new TweenityNode(nodeModel.NodeID);

                visualNode.NodeModel = nodeModel;
                visualNode.UpdateFromModel();
                visualNode.userData = nodeModel;

                visualNode.RegisterCallback<MouseDownEvent>(evt =>
                {
                    if (evt.button == 0)
                    {
                        OnNodeSelected?.Invoke(nodeModel);
                    }
                });

                Rect position = positionOverride ?? new Rect(200, 200, 150, 200);
                visualNode.SetPosition(position);
                AddElement(visualNode);

                // Wait one frame to ensure all nodes are ready before drawing edges
                EditorApplication.delayCall += RenderConnections;

                int count = this.graphElements.OfType<TweenityNode>().Count();
                Debug.Log($"[RenderNode] Total visual nodes after AddElement: {count}");
            };
        }

        public void RemoveNodeFromView(string nodeId)
        {
            var target = this.graphElements
                .OfType<TweenityNode>()
                .FirstOrDefault(n => string.Equals(n.NodeID, nodeId, StringComparison.Ordinal));

            if (target != null)
            {
                Debug.Log($"[GraphView] Removing node from view: {nodeId}");
                RemoveElement(target); 
            }
            else
            {
                Debug.LogWarning($"[GraphView] Node not found in view: {nodeId}");
            }
        }

        public void UpdateNodeTitle(string nodeId, string newTitle)
        {
            var target = this.graphElements
                .OfType<TweenityNode>()
                .FirstOrDefault(n => n.NodeID == nodeId);

            if (target != null)
            {
                target.title = newTitle;
            }
        }

        public void RefreshNodeVisual(string nodeId)
        {
            var target = this.graphElements
                .OfType<TweenityNode>()
                .FirstOrDefault(n => n.NodeID == nodeId);

            if (target != null && target.NodeModel != null)
            {
                target.UpdateFromModel();
                target.RefreshExpandedState();
                target.RefreshPorts();
            }
        }

        public void RenderConnections()
        {
            var allNodes = this.graphElements.OfType<TweenityNode>().ToList();
            Debug.Log($"[RenderConnections] Total nodes in GraphView: {allNodes.Count}");

            foreach (var sourceNode in allNodes)
            {
                if (sourceNode.NodeModel == null || sourceNode.OutputPort == null)
                {
                    Debug.LogWarning($"[RenderConnections] Skipping node {sourceNode?.NodeID} due to null model or output port.");
                    continue;
                }

                foreach (var targetId in sourceNode.NodeModel.ConnectedNodes)
                {
                    Debug.Log($"[RenderConnections] {sourceNode.NodeID} trying to connect to {targetId}");

                    var targetNode = allNodes.FirstOrDefault(n => n.NodeID == targetId);
                    if (targetNode == null || targetNode.InputPort == null)
                    {
                        Debug.LogWarning($"[RenderConnections] Target node {targetId} not found or has no input port.");
                        continue;
                    }

                    bool alreadyConnected = this.graphElements.OfType<Edge>().Any(e =>
                        e.output?.node == sourceNode && e.input?.node == targetNode);

                    if (alreadyConnected)
                    {
                        Debug.Log($"[RenderConnections] Edge already exists from {sourceNode.NodeID} to {targetId}");
                        continue;
                    }

                    var edge = new Edge
                    {
                        output = sourceNode.OutputPort,
                        input = targetNode.InputPort
                    };

                    edge.output.Connect(edge);
                    edge.input.Connect(edge);
                    AddElement(edge);

                    Debug.Log($"[RenderConnections] Edge created from {sourceNode.NodeID} to {targetId}");
                }
            }
        }


        public void ToggleGridVisibility()
        {
            gridBackground.visible = !gridBackground.visible;
        }

        public void ZoomIn()
        {
            this.transform.scale *= 1.1f;
        }

        public void ZoomOut()
        {
            this.transform.scale *= 0.9f;
        }

        public void ResetView()
        {
            this.transform.position = Vector3.zero;
            this.transform.scale = Vector3.one;
        }
    }
}
