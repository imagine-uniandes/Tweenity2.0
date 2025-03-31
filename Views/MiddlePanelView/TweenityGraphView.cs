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
