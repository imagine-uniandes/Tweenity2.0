using Models;
using Models.Nodes;
using System;
using Views;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System.IO;
using UnityEditor;
using Views.MiddlePanel;
using System.Linq;

namespace Controllers
{
    public class GraphController
    {
        public GraphModel Graph { get; private set; }
        public TweenityGraphView GraphView { get; private set; }

        private VisualElement rightPanelRoot;
        private string lastSavedPath;

        public GraphController(TweenityGraphView graphView)
        {
            Graph = new GraphModel();
            GraphView = graphView;
        }

        public bool AddNode(TweenityNodeModel node)
        {
            Debug.Log($"[GraphController]  AddNode called for: {node.Title} | ID: {node.NodeID}");

            if (Graph.AddNode(node))
            {
                Debug.Log($"[GraphController]  Node added to model. Now rendering...");
                GraphView.RenderNode(node);
                Debug.Log($"[GraphController]  RenderNode completed for: {node.NodeID}");
                return true;
            }

            Debug.LogWarning($"[GraphController]  Node not added. It may already exist or conflict (e.g., Start node)");
            return false;
        }

        public void RemoveNode(string nodeId)
        {
            CleanupConnectionsTo(nodeId);  
            GraphView.RemoveNodeFromView(nodeId);
            Graph.RemoveNode(nodeId);
        }

        public TweenityNodeModel GetNode(string nodeId) => Graph.GetNode(nodeId);

        public List<TweenityNodeModel> GetAllNodes() => Graph.Nodes;

        public void SetRightPanelRoot(VisualElement rightPanel)
        {
            rightPanelRoot = rightPanel;
        }

        public void OnNodeSelected(TweenityNodeModel node)
        {
            if (!string.IsNullOrEmpty(pendingSourceNodeId) && onTargetNodeSelected != null)
            {
                Debug.Log($"[GraphController] Connecting {pendingSourceNodeId} -> {node.NodeID}");
                onTargetNodeSelected.Invoke(node.NodeID);
                pendingSourceNodeId = null;
                onTargetNodeSelected = null;
                return; 
            }

            if (rightPanelRoot == null)
            {
                Debug.LogWarning("Right panel root not set.");
                return;
            }

            rightPanelRoot.Clear();

            switch (node)
            {
                case DialogueNodeModel dialogue:
                    rightPanelRoot.Add(new Views.RightPanel.DialogueView(dialogue, this));
                    break;
                case ReminderNodeModel reminder:
                    rightPanelRoot.Add(new Views.RightPanel.ReminderView(reminder, this));
                    break;
                case MultipleChoiceNodeModel multi:
                    rightPanelRoot.Add(new Views.RightPanel.MultipleChoiceView(multi, this));
                    break;
                case NoTypeNodeModel noType:
                    rightPanelRoot.Add(new Views.RightPanel.NoTypeView(noType, this));
                    break;
                case RandomNodeModel random:
                    rightPanelRoot.Add(new Views.RightPanel.RandomView(random, this));
                    break;
                case StartNodeModel start:
                    rightPanelRoot.Add(new Views.RightPanel.StartView(start, this));
                    break;
                case EndNodeModel end:
                    rightPanelRoot.Add(new Views.RightPanel.EndView(end, this));
                    break;
                case TimeoutNodeModel timeout:
                    rightPanelRoot.Add(new Views.RightPanel.TimeoutView(timeout, this));
                    break;
                default:
                    Debug.LogWarning("Unknown node type selected.");
                    break;
            }
        }

        public void UpdateNodeTitle(TweenityNodeModel model, string newTitle)
        {
            model.Title = newTitle;
            GraphView.UpdateNodeTitle(model.NodeID, newTitle);
            GraphView.RefreshNodeVisual(model.NodeID);
        }

        public void UpdateNodeDescription(TweenityNodeModel model, string newDescription)
        {
            model.Description = newDescription;
            GraphView.RefreshNodeVisual(model.NodeID);
        }

        public void SaveCurrentGraph()
        {
            if (string.IsNullOrEmpty(lastSavedPath))
            {
                lastSavedPath = EditorUtility.SaveFilePanel("Save Graph", "", "tweenity_story.twee", "twee");
            }

            if (!string.IsNullOrEmpty(lastSavedPath))
            {
                ExportGraphTo(lastSavedPath);
            }
        }

        public void ExportGraphTo(string path)
        {
            string twee = GraphParser.ExportToTwee(Graph.Nodes);
            File.WriteAllText(path, twee);
            Debug.Log("Graph exported to: " + path);
        }

        public void LoadGraphFrom(string path)
        {
            string twee = File.ReadAllText(path);
            var nodes = GraphParser.ImportFromTwee(twee);

            foreach (var node in Graph.Nodes)
            {
                GraphView.RemoveNodeFromView(node.NodeID);
            }

            Graph = new GraphModel();
            foreach (var node in nodes)
            {
                AddNode(node);
            }

            lastSavedPath = path;
            Debug.Log("Graph loaded from: " + path);
        }

        public void ToggleGrid() => GraphView.ToggleGridVisibility();
        public void ZoomIn() => GraphView.ZoomIn();
        public void ZoomOut() => GraphView.ZoomOut();
        public void ResetView() => GraphView.ResetView();

        public void ShowHelp() => Debug.Log("Help clicked (controller)");

        public void CreateNewNode()
        {
            var newNode = new NoTypeNodeModel("New Node");

            Rect baseRect = new Rect(200, 200, 150, 200);
            Rect position = baseRect;

            var existingRects = GraphView.graphElements
                .OfType<TweenityNode>()
                .Select(n => n.GetPosition())
                .ToList();

            int attempts = 0;
            float offset = 30f;

            while (existingRects.Any(r => r.Overlaps(position)))
            {
                attempts++;
                position.position += new Vector2(offset, offset);
                if (attempts > 100)
                {
                    Debug.LogWarning("Too many overlapping nodes; giving up on auto-placement.");
                    break;
                }
            }

            newNode.Position = position.position;

            if (Graph.AddNode(newNode))
            {
                GraphView.RenderNode(newNode, position);
                Debug.Log($"[GraphController] Created new node at: {position.position}");
            }
            else
            {
                Debug.LogWarning("Node not added. Maybe a Start node already exists?");
            }
        }

        public void ConnectNodes(string fromNodeId, string toNodeId)
        {
            var fromNode = Graph.GetNode(fromNodeId);
            var toNode = Graph.GetNode(toNodeId);

            if (fromNode == null || toNode == null)
            {
                Debug.LogWarning($"[GraphController] Cannot connect nodes. One or both nodes not found. From: {fromNodeId}");
                return;
            }

            if (fromNode.Type is NodeType.End)
            {
                string msg = "End nodes cannot have outgoing connections.";
                Debug.LogWarning($"[GraphController] {msg}");
    #if UNITY_EDITOR
                EditorUtility.DisplayDialog("Invalid Connection", msg, "OK");
    #endif
                return;
            }

            if (fromNode.Type is NodeType.Start or NodeType.NoType or NodeType.Reminder)
            {
                if (fromNode.OutgoingPaths.Count >= 1)
                {
                    string msg = $"{fromNode.Type} nodes can only have one outgoing connection.";
                    Debug.LogWarning($"[GraphController] {msg}");
    #if UNITY_EDITOR
                    EditorUtility.DisplayDialog("Connection Limit Reached", msg, "OK");
    #endif
                    return;
                }
            }

            if (!fromNode.IsConnectedTo(toNodeId))
            {
                fromNode.ConnectTo(toNodeId);
                Debug.Log($"[GraphController] Connected node {fromNodeId} -> {toNodeId}");
                GraphView.RenderConnections();
            }
            else
            {
                Debug.LogWarning($"[GraphController] Nodes already connected: {fromNodeId} -> {toNodeId}");
            }
        }

        public void DisconnectNodes(string fromNodeId, string toNodeId)
        {
            var fromNode = Graph.GetNode(fromNodeId);
            if (fromNode == null)
            {
                Debug.LogWarning($"[GraphController] Cannot disconnect. Node not found: {fromNodeId}");
                return;
            }

            if (fromNode.IsConnectedTo(toNodeId))
            {
                fromNode.DisconnectFrom(toNodeId);
                Debug.Log($"[GraphController] Disconnected node {fromNodeId} -> {toNodeId}");
            }
            else
            {
                Debug.LogWarning($"[GraphController] Connection not found: {fromNodeId} -> {toNodeId}");
            }
        }

        private string pendingSourceNodeId;
        private Action<string> onTargetNodeSelected;

        public void StartConnectionFrom(string sourceNodeId, Action<string> onTargetSelected)
        {
            pendingSourceNodeId = sourceNodeId;
            onTargetNodeSelected = onTargetSelected;
            Debug.Log($"[GraphController] Ready to connect from node: {sourceNodeId}");
        }

        public void TryConnectTo(string targetId)
        {
            if (string.IsNullOrEmpty(pendingSourceNodeId))
            {
                Debug.LogWarning("[Connect] No source selected for connection.");
                return;
            }

            if (pendingSourceNodeId == targetId)
            {
                Debug.LogWarning("[Connect] Cannot connect node to itself.");
                pendingSourceNodeId = null;
                return;
            }

            ConnectNodes(pendingSourceNodeId, targetId);
            pendingSourceNodeId = null;
        }

        private void CleanupConnectionsTo(string deletedNodeId)
        {
            foreach (var node in Graph.Nodes)
            {
                if (node.IsConnectedTo(deletedNodeId))
                {
                    node.DisconnectFrom(deletedNodeId);
                    Debug.Log($"[GraphController] Removed connection to deleted node: {deletedNodeId} from node: {node.NodeID}");
                }
            }
        }
        public (bool, string) ChangeNodeType(TweenityNodeModel oldModel, NodeType newType)
        {
            if (oldModel.Type == newType)
                return (true, null);

            Debug.Log($"Changing node type: Old Type = {oldModel.Type}, New Type = {newType}");

            // Solo se permite un nodo de tipo Start
            if (newType == NodeType.Start && Graph.Nodes.Any(n => n.Type == NodeType.Start && n.NodeID != oldModel.NodeID))
            {
                Debug.LogWarning("Cannot change to Start node: another Start node already exists.");
                return (false, "Only one Start node is allowed in the graph.");
            }

            // Capturar la posición del nodo visual
            var visualNode = GraphView.graphElements
                .OfType<TweenityNode>()
                .FirstOrDefault(n => n.NodeID == oldModel.NodeID);

            Vector2 pos = visualNode?.GetPosition().position ?? new Vector2(200, 200);

            // Crear nuevo modelo del tipo deseado
            TweenityNodeModel newModel = newType switch
            {
                NodeType.Dialogue        => new DialogueNodeModel(oldModel.Title),
                NodeType.Reminder        => new ReminderNodeModel(oldModel.Title),
                NodeType.MultipleChoice  => new MultipleChoiceNodeModel(oldModel.Title),
                NodeType.Random          => new RandomNodeModel(oldModel.Title),
                NodeType.Start           => new StartNodeModel(oldModel.Title),
                NodeType.End             => new EndNodeModel(oldModel.Title),
                NodeType.Timeout         => new TimeoutNodeModel(oldModel.Title),
                _                        => new NoTypeNodeModel(oldModel.Title)
            };

            newModel.Description   = oldModel.Description;
            newModel.Position      = pos;
            newModel.OutgoingPaths = new List<PathData>(oldModel.OutgoingPaths);

            // Actualizar conexiones entrantes: redirigir nodos que apuntaban al viejo al nuevo
            foreach (var node in Graph.Nodes)
            {
                foreach (var path in node.OutgoingPaths)
                {
                    if (path.TargetNodeID == oldModel.NodeID)
                    {
                        path.TargetNodeID = newModel.NodeID;
                    }
                }
            }

            // Reemplazo en modelo y vista
            Graph.RemoveNode(oldModel.NodeID);
            GraphView.RemoveNodeFromView(oldModel.NodeID);

            Graph.AddNode(newModel);
            GraphView.RenderNode(newModel, new UnityEngine.Rect(pos, new Vector2(150, 200)));

            GraphView.RefreshNodeVisual(newModel.NodeID);
            OnNodeSelected(newModel);

            return (true, null);
        }
        public void DebugGraph()
        {
            Debug.Log("=== Graph Debug Info ===");

            foreach (var node in Graph.Nodes)
            {
                Debug.Log($"Node: {node.Title} ({node.Type}) - ID: {node.NodeID} - Pos: {node.Position}");

                foreach (var path in node.OutgoingPaths)
                {
                    Debug.Log($"  ↳ Path: Label='{path.Label}' | Trigger='{path.Trigger}' → TargetID: {path.TargetNodeID}");
                }
            }
        }
        public void PrintCurrentSelection()
        {
            var selected = GraphView.selection;

            if (selected == null || selected.Count == 0)
            {
                Debug.Log("[PrintSelection] No nodes selected.");
                return;
            }

            Debug.Log($"[PrintSelection] Selected {selected.Count} element(s):");

            foreach (var item in selected)
            {
                if (item is Views.MiddlePanel.TweenityNode node && node.NodeModel != null)
                {
                    Debug.Log($"→ NodeID: {node.NodeID}, Title: {node.NodeModel.Title}, Type: {node.NodeModel.Type}");
                }
                else
                {
                    Debug.Log($"→ Non-node element selected: {item}");
                }
            }
        }
        public void SearchNodes(string query)
        {
            Debug.Log($"[SearchNodes] Searching nodes for query: {query}");

            var lowerQuery = query.ToLowerInvariant();

            var results = Graph.Nodes
                .Where(n =>
                    (!string.IsNullOrEmpty(n.Title) && n.Title.ToLowerInvariant().Contains(lowerQuery)) ||
                    (!string.IsNullOrEmpty(n.Description) && n.Description.ToLowerInvariant().Contains(lowerQuery)))
                .ToList();

            Debug.Log($"[SearchNodes] Found {results.Count} matching node(s):");

            foreach (var node in results)
            {
                Debug.Log($"→ {node.Title} ({node.Type}) - ID: {node.NodeID}");
            }

        #if UNITY_EDITOR
            if (results.Count == 0)
            {
                UnityEditor.EditorUtility.DisplayDialog("Search", "No matching nodes found.", "OK");
            }
        #endif
        }
        public void UpdateReminderText(ReminderNodeModel model, string newText)
        {
            model.ReminderText = newText;
            GraphView.RefreshNodeVisual(model.NodeID);
        }

        public void UpdateReminderTimer(ReminderNodeModel model, float newTimer)
        {
            model.ReminderTimer = newTimer;
            GraphView.RefreshNodeVisual(model.NodeID);
        }
        public void AddRandomPath(RandomNodeModel model)
        {
            int count = model.OutgoingPaths.Count;
            model.OutgoingPaths.Add(new PathData($"Path {count + 1}"));
            GraphView.RefreshNodeVisual(model.NodeID);
        }
        public void UpdateDialogueText(DialogueNodeModel model, string newText)
        {
            model.DialogueText = newText;
            GraphView.RefreshNodeVisual(model.NodeID);
        }
        public void AddDialogueResponse(DialogueNodeModel model)
        {
            int count = model.OutgoingPaths.Count;
            model.AddResponse("Response " + (count + 1));
            GraphView.RefreshNodeVisual(model.NodeID);
        }


    }
}
