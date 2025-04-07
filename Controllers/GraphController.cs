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

            Debug.Log($"[Selection] Looking for NodeID: {node.NodeID}");

            // 🔍 Dump all currently rendered nodes
            var allVisualNodes = GraphView.graphElements.OfType<TweenityNode>().ToList();
            Debug.Log($"[GraphView] Currently rendered node count: {allVisualNodes.Count}");
            foreach (var n in allVisualNodes)
            {
                Debug.Log($"[GraphView] Node in view: {n.title} | NodeID: {n.NodeID}");
            }

            var ghostNode = GraphView.graphElements
                .OfType<TweenityNode>()
                .FirstOrDefault(n => string.Equals(n.NodeID, node.NodeID, StringComparison.Ordinal));

            if (ghostNode != null)
            {
                Debug.Log($"[Selection - View Match] View NodeID: {ghostNode.NodeID} - ModelID: {node.NodeID}");
            }
            else
            {
                Debug.Log($"[Selection - View Match] No visual node found for model ID: {node.NodeID}");
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

        public (bool, string) ChangeNodeType(TweenityNodeModel oldModel, NodeType newType)
        {
            if (oldModel.Type == newType)
                return (true, null);

            Debug.Log($"Changing node type: Old Type = {oldModel.Type}, New Type = {newType}");

            // Prevent multiple Start nodes
            if (newType == NodeType.Start && Graph.Nodes.Any(n => n.Type == NodeType.Start && n.NodeID != oldModel.NodeID))
            {
                Debug.LogWarning("Cannot change to Start node: another Start node already exists.");
                return (false, "Only one Start node is allowed in the graph.");
            }

            // Capture visual node position before deletion
            var oldVisualNode = GraphView.graphElements
                .OfType<TweenityNode>()
                .FirstOrDefault(n => n.NodeID == oldModel.NodeID);

            Rect oldPosition = oldVisualNode != null ? oldVisualNode.GetPosition() : new Rect(200, 200, 150, 200);

            // Create new model with preserved data
            TweenityNodeModel newModel = newType switch
            {
                NodeType.Dialogue => new DialogueNodeModel(oldModel.Title),
                NodeType.Reminder => new ReminderNodeModel(oldModel.Title),
                NodeType.MultipleChoice => new MultipleChoiceNodeModel(oldModel.Title),
                NodeType.Random => new RandomNodeModel(oldModel.Title),
                NodeType.Start => new StartNodeModel(oldModel.Title),
                NodeType.End => new EndNodeModel(oldModel.Title),
                NodeType.Timeout => new TimeoutNodeModel(oldModel.Title),
                _ => new NoTypeNodeModel(oldModel.Title),
            };

            newModel.Description = oldModel.Description;
            newModel.ConnectedNodes = new List<string>();

            // Remove old node
            Debug.Log($"[GraphController] Removing old node: {oldModel.NodeID}");
            CleanupConnectionsTo(oldModel.NodeID);  
            GraphView.RemoveNodeFromView(oldModel.NodeID);
            Graph.RemoveNode(oldModel.NodeID);

            // Add new node with preserved position
            Graph.AddNode(newModel);
            GraphView.RenderNode(newModel, oldPosition);
            Debug.Log($"[GraphController] Added new node: {newModel.NodeID}");

            GraphView.RefreshNodeVisual(newModel.NodeID);
            OnNodeSelected(newModel);
            return (true, null);
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

            // Starting position
            Rect baseRect = new Rect(200, 200, 150, 200);
            Rect position = baseRect;

            // Scan for overlap
            var existingRects = GraphView.graphElements
                .OfType<TweenityNode>()
                .Select(n => n.GetPosition())
                .ToList();

            int attempts = 0;
            float offset = 30f;

            // Try shifting right/down until we find an empty space
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

        public void DebugGraph()
        {
            Debug.Log("=== Graph Debug Info ===");
            foreach (var node in Graph.Nodes)
            {
                Debug.Log($"Node: {node.Title} ({node.Type}) - ID: {node.NodeID}");
            }
        }

        public void PrintCurrentSelection()
        {
            var selected = GraphView.selection;
        }

        public void SearchNodes(string query)
        {
            Debug.Log($"Searching nodes for: {query}");
        }

        public void UpdateDialogueText(DialogueNodeModel model, string newText) => model.DialogueText = newText;

        public void UpdateDialogueResponse(DialogueNodeModel model, int index, string newValue)
        {
            if (index >= 0 && index < model.Responses.Count)
            {
                model.Responses[index] = newValue;
            }
            else
            {
                Debug.LogWarning($"[GraphController] Invalid response index: {index}");
            }
        }

        public void AddDialogueResponse(DialogueNodeModel model) => model.AddResponse("Response " + (model.Responses.Count + 1));
        public void AddChoiceToMultipleChoiceNode(MultipleChoiceNodeModel model) => model.AddChoice("Choice " + (model.Choices.Count + 1));
        public void UpdateNoTypeTitle(NoTypeNodeModel model, string newTitle) => model.Title = newTitle;
        public void UpdateNoTypeDescription(NoTypeNodeModel model, string newDesc) => model.Description = newDesc;
        public void AddRandomPath(RandomNodeModel model) => model.AddPath("Path " + (model.PossiblePaths.Count + 1));
        public void UpdateReminderText(ReminderNodeModel model, string newText) => model.ReminderText = newText;
        public void UpdateReminderTimer(ReminderNodeModel model, float newTimer) => model.ReminderTimer = newTimer;
        public void UpdateTimeoutCondition(TimeoutNodeModel model, string newCondition) => model.Condition = newCondition;
        public void UpdateTimeoutTimer(TimeoutNodeModel model, float newDuration) => model.TimeoutDuration = newDuration;

        public void ConnectNodes(string fromNodeId, string toNodeId)
        {
            var fromNode = Graph.GetNode(fromNodeId);
            var toNode = Graph.GetNode(toNodeId);

            if (fromNode == null || toNode == null)
            {
                Debug.LogWarning($"[GraphController] Cannot connect nodes. One or both nodes not found. From: {fromNodeId}");
                return;
            }

            if (!fromNode.ConnectedNodes.Contains(toNodeId))
            {
                fromNode.ConnectedNodes.Add(toNodeId);
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

            if (fromNode.ConnectedNodes.Contains(toNodeId))
            {
                fromNode.ConnectedNodes.Remove(toNodeId);
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
                if (node.ConnectedNodes.Contains(deletedNodeId))
                {
                    node.ConnectedNodes.Remove(deletedNodeId);
                    Debug.Log($"[GraphController] Removed connection to deleted node: {deletedNodeId} from node: {node.NodeID}");
                }
            }
        }

    }
}
