using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Models.Nodes;
using System.Linq;
using System;
using Views.MiddlePanel;
using Models;
using UnityEditor;
using Controllers;

namespace Views
{
    public class TweenityGraphView : GraphView
    {
        public Action<TweenityNodeModel> OnNodeSelected;
        private GridBackground gridBackground;

        private GraphController _controller;

        public bool _editingEnabled = true;
        private ContentDragger _contentDragger;
        private SelectionDragger _selectionDragger;
        private RectangleSelector _rectangleSelector;

        private Label _runtimeLabel;


        public TweenityGraphView()
        {
            this.style.flexGrow = 1;
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            this.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f); 
            this.Insert(0, gridBackground);

            this.graphViewChanged = (GraphViewChange change) =>
            {
                if (change.elementsToRemove != null)
                {
                    foreach (var element in change.elementsToRemove)
                    {
                        switch (element)
                        {
                            case Edge edge:
                                if (edge.output?.node is TweenityNode outputNode &&
                                    edge.input?.node is TweenityNode inputNode)
                                {
                                    string fromId = outputNode.NodeID;
                                    string toId = inputNode.NodeID;

                                    outputNode.NodeModel?.DisconnectFrom(toId);
                                    Debug.Log($"[GraphViewChanged] Removed model connection: {fromId} -> {toId}");
                                }
                                break;

                            case TweenityNode node:
                                string nodeId = node.NodeID;
                                Debug.Log($"[GraphViewChanged] Removing node: {nodeId}"); 
                                _controller?.RemoveNode(nodeId); 
                                break;
                        }
                    }
                }

                return change;
            };
            RegisterCallback<KeyDownEvent>(evt =>
            {
                if (!_editingEnabled) return; // ← PROTECCIÓN

                if (evt.keyCode is KeyCode.Delete or KeyCode.Backspace)
                {
                    var selectedNodes = selection.OfType<TweenityNode>().ToList();

                    if (selectedNodes.Count == 0)
                    {
                        Debug.Log("[DeleteKey] No node selected.");
                        return;
                    }

                    foreach (var node in selectedNodes)
                    {
                        Debug.Log($"[DeleteKey] Removing node: {node.NodeID}");
                        _controller?.RemoveNode(node.NodeID);
                    }

                    evt.StopPropagation();
                }
            });

            _contentDragger = new ContentDragger();
            _selectionDragger = new SelectionDragger();
            _rectangleSelector = new RectangleSelector();

            this.AddManipulator(_contentDragger);
            this.AddManipulator(_selectionDragger);
            this.AddManipulator(_rectangleSelector);
        }

        public void SetController(GraphController controller)
        {
            _controller = controller;
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
                    if (_editingEnabled && evt.button == 0)
                    {
                        OnNodeSelected?.Invoke(nodeModel);
                    }
                });

                Rect position = positionOverride ?? new Rect(nodeModel.Position.x, nodeModel.Position.y, 150, 200);
                visualNode.SetPosition(position);

                visualNode.RegisterCallback<GeometryChangedEvent>(_ =>
                {
                    var currentPosition = visualNode.GetPosition().position;
                    nodeModel.Position = currentPosition;
                });

                AddElement(visualNode);

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
                Debug.Log($"[GraphView] Removing node and its edges from view: {nodeId}");

                var connectedEdges = this.graphElements
                    .OfType<Edge>()
                    .Where(e =>
                        e.input?.node == target || e.output?.node == target)
                    .ToList();

                foreach (var edge in connectedEdges)
                {
                    edge.input?.Disconnect(edge);
                    edge.output?.Disconnect(edge);
                    RemoveElement(edge);
                }

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

                foreach (var path in sourceNode.NodeModel.OutgoingPaths)
                {
                    string targetId = path.TargetNodeID;

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
        public void SetEditingEnabled(bool enabled)
        {
            _editingEnabled = enabled;

            // Toggle manipulators
            if (_contentDragger != null) this.RemoveManipulator(_contentDragger);
            if (_selectionDragger != null) this.RemoveManipulator(_selectionDragger);
            if (_rectangleSelector != null) this.RemoveManipulator(_rectangleSelector);

            if (enabled)
            {
                _contentDragger = new ContentDragger();
                _selectionDragger = new SelectionDragger();
                _rectangleSelector = new RectangleSelector();

                this.AddManipulator(_contentDragger);
                this.AddManipulator(_selectionDragger);
                this.AddManipulator(_rectangleSelector);
            }

            // Mostrar u ocultar mensaje de runtime
            if (_runtimeLabel == null)
            {
                _runtimeLabel = new Label("⚠ La edición está deshabilitada durante la ejecución de la simulación.");
                _runtimeLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                _runtimeLabel.style.color = Color.yellow;
                _runtimeLabel.style.marginTop = 4;
                _runtimeLabel.style.marginBottom = 4;
                _runtimeLabel.style.alignSelf = Align.Center;
                _runtimeLabel.style.display = DisplayStyle.None;
                _runtimeLabel.style.unityTextAlign = TextAnchor.MiddleCenter;

                // Lo insertamos después del fondo
                this.Insert(1, _runtimeLabel);
            }

            _runtimeLabel.style.display = enabled ? DisplayStyle.None : DisplayStyle.Flex;
        }
        public void ClearGraphView()
        {
            // Remove all nodes
            var nodes = this.graphElements.OfType<TweenityNode>().ToList();
            foreach (var node in nodes)
            {
                RemoveElement(node);
            }

            // Remove all edges
            var edges = this.graphElements.OfType<Edge>().ToList();
            foreach (var edge in edges)
            {
                edge.input?.Disconnect(edge);
                edge.output?.Disconnect(edge);
                RemoveElement(edge);
            }

            Debug.Log("[GraphView] Cleared all nodes and edges from view.");
        }


    }
}
