using Models;
using Models.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Views;
using Views.MiddlePanel;
using UnityEditor.Experimental.GraphView;
using Simulation.Runtime;
using Simulation;
using Tweenity;

namespace Controllers
{
    public class GraphController
    {
#if UNITY_EDITOR
        public static GraphController ActiveEditorGraphController;
#endif
        public SimulationController SimulationController { get; private set; }
        public GraphModel Graph { get; private set; } = new GraphModel();
        public TweenityGraphView GraphView { get; private set; }
        private VisualElement rightPanelRoot;
        private string lastSavedPath;
        public bool IsEditingEnabled { get; set; } = true;
        private SimulationController simulationController;
        private bool isSimulationRunning = false;
        public bool IsDirty { get; private set; } = false;
        private string pendingSourceNodeId;
        private Action<string> onTargetNodeSelected;

        public GraphController() {}

        // ==========================
        // Core Graph Lifecycle
        // ==========================
        public void MarkDirty() => IsDirty = true;

        public void SetGraphView(TweenityGraphView graphView)
        {
            GraphView = graphView;
#if UNITY_EDITOR
            ActiveEditorGraphController = this;
#endif
        }

        public void ClearGraph()
        {
            Graph.Nodes.Clear();
            GraphView?.ClearGraphView();
        }

        // ==========================
        // Node Operations
        // ==========================

        public void CreateNewNode()
        {
            if (!IsEditingEnabled || GraphView == null) return;

            var newNode = new NoTypeNodeModel("New Node");

            // Default position
            Vector2 spawnPosition = new Vector2(200, 200);

            // Try to spawn next to selected node
            var selectedNode = GraphView.selection
                .OfType<TweenityNode>()
                .FirstOrDefault();

            if (selectedNode != null)
            {
                spawnPosition = selectedNode.GetPosition().position + new Vector2(250, 0); // 250 pixels to the right
            }

            // Avoid overlaps if possible
            Rect positionRect = new Rect(spawnPosition, new Vector2(150, 200));
            var existingRects = GraphView.graphElements
                .OfType<TweenityNode>()
                .Select(n => n.GetPosition())
                .ToList();

            int attempts = 0;
            while (existingRects.Any(r => r.Overlaps(positionRect)))
            {
                positionRect.position += new Vector2(30, 30);
                if (++attempts > 100) break;
            }

            newNode.Position = positionRect.position;

            if (AddNode(newNode))
            {
                GraphView.CenterOnNode(newNode.NodeID);
            }
        }

        public bool AddNode(TweenityNodeModel node)
        {
            if (!IsEditingEnabled || GraphView == null)
                return false;

            if (Graph.AddNode(node))
            {
                GraphView.RenderNode(node);
                MarkDirty();
                return true;
            }
            return false;
        }

        public void RemoveNode(string nodeId)
        {
            if (!IsEditingEnabled || GraphView == null) return;

            CleanupConnectionsTo(nodeId);

            var node = Graph.GetNode(nodeId);
            if (node != null)
            {
                foreach (var path in node.OutgoingPaths.ToList())
                    node.DisconnectFrom(path.TargetNodeID);
            }

            GraphView.RemoveNodeFromView(nodeId);
            Graph.RemoveNode(nodeId);
            GraphView.RenderConnections();

            foreach (var n in Graph.Nodes)
                GraphView.RefreshNodeVisual(n.NodeID);

            MarkDirty();
        }

        private void CleanupConnectionsTo(string deletedNodeId)
        {
            foreach (var node in Graph.Nodes)
            {
                if (node.IsConnectedTo(deletedNodeId))
                {
                    node.DisconnectFrom(deletedNodeId);
                    GraphView?.RefreshNodeVisual(node.NodeID);
                    MarkDirty();
                }
            }
        }

        public void UpdateNodeTitle(TweenityNodeModel model, string newTitle)
        {
            if (!IsEditingEnabled || GraphView == null) return;

            model.Title = newTitle;
            GraphView.UpdateNodeTitle(model.NodeID, newTitle);
            GraphView.RefreshNodeVisual(model.NodeID);
            MarkDirty();
        }

        public void UpdateNodeDescription(TweenityNodeModel model, string newDescription)
        {
            if (!IsEditingEnabled || GraphView == null) return;

            model.Description = newDescription;
            GraphView.RefreshNodeVisual(model.NodeID);
            MarkDirty();
        }

        public (bool, string) ChangeNodeType(TweenityNodeModel oldModel, NodeType newType)
        {
            if (!IsEditingEnabled || GraphView == null) return (false, "Editing disabled or GraphView unavailable.");
            if (oldModel.Type == newType) return (true, null);

            if (newType == NodeType.Start && Graph.Nodes.Any(n => n.Type == NodeType.Start && n.NodeID != oldModel.NodeID))
                return (false, "Only one Start node allowed.");

            Vector2 pos = GraphView.graphElements
                .OfType<TweenityNode>()
                .FirstOrDefault(n => n.NodeID == oldModel.NodeID)
                ?.GetPosition().position ?? new Vector2(200, 200);

            RemoveNode(oldModel.NodeID);
            GraphView.ForceRemoveNodeById(oldModel.NodeID);

            TweenityNodeModel newModel = newType switch
            {
                NodeType.Dialogue       => new DialogueNodeModel(oldModel.Title),
                NodeType.Reminder       => new ReminderNodeModel(oldModel.Title),
                NodeType.MultipleChoice => new MultipleChoiceNodeModel(oldModel.Title),
                NodeType.Random         => new RandomNodeModel(oldModel.Title),
                NodeType.Start          => new StartNodeModel(oldModel.Title),
                NodeType.End            => new EndNodeModel(oldModel.Title),
                NodeType.Timeout        => new TimeoutNodeModel(oldModel.Title),
                _                       => new NoTypeNodeModel(oldModel.Title)
            };

            newModel.Description = oldModel.Description;
            newModel.Position = pos;

            AddNode(newModel); // This already internally calls RenderNode()

            OnNodeSelected(newModel);
            MarkDirty();

            return (true, null);
        }

        // ==========================
        // Node-Specific Updates 
        // ==========================
        public void SetReminderSuccessTrigger(ReminderNodeModel model, string objectName, string scriptName, string methodName, string targetNodeID)
        {
            if (!IsEditingEnabled || GraphView == null) return;

            model.SetSuccessPath(objectName, scriptName, methodName, targetNodeID);
            GraphView.RefreshNodeVisual(model.NodeID);
            MarkDirty();
        }

        public void SetReminderTimeoutTrigger(ReminderNodeModel model, string objectName, string scriptName, string methodName)
        {
            if (!IsEditingEnabled || GraphView == null) return;

            model.SetReminderPath(objectName, scriptName, methodName);
            GraphView.RefreshNodeVisual(model.NodeID);
            MarkDirty();
        }

        public void AddRandomPath(RandomNodeModel model)
        {
            if (!IsEditingEnabled || GraphView == null) return;

            model.OutgoingPaths.Add(new PathData($"Path {model.OutgoingPaths.Count + 1}"));
            GraphView.RefreshNodeVisual(model.NodeID);
            MarkDirty();
        }

        public void SetReminderInstruction(string nodeId, string objectName, string methodName, string parameters = "")
        {
            var node = Graph.GetNode(nodeId);
            if (node is ReminderNodeModel reminder)
            {
                reminder.SetReminderInstruction(objectName, methodName, parameters);
            }
            else
            {
                Debug.LogWarning($"[GraphController] Tried to set reminder instruction on a non-Reminder node: {nodeId}");
            }
        }

        // ==========================
        // Connection Operations
        // ==========================
        public void ConnectNodes(string fromNodeId, string toNodeId)
        {
            if (!IsEditingEnabled || GraphView == null) return;

            var fromNode = Graph.GetNode(fromNodeId);
            var toNode = Graph.GetNode(toNodeId);

            if (fromNode == null || toNode == null) return;

            if (fromNode.Type == NodeType.End) return;

            if ((fromNode.Type == NodeType.Start || fromNode.Type == NodeType.NoType || fromNode.Type == NodeType.Reminder)
                && fromNode.OutgoingPaths.Count >= 1)
                return;

            if (!fromNode.IsConnectedTo(toNodeId))
            {
                fromNode.ConnectTo(toNodeId);
                GraphView.RenderConnections();
                MarkDirty();
            }
        }

        public void DisconnectNodes(string fromNodeId, string toNodeId)
        {
            if (!IsEditingEnabled || GraphView == null) return;

            var fromNode = Graph.GetNode(fromNodeId);
            if (fromNode != null && fromNode.IsConnectedTo(toNodeId))
            {
                fromNode.DisconnectFrom(toNodeId);
                GraphView.RenderConnections();
                MarkDirty();
            }
        }

        public void StartConnectionFrom(string sourceNodeId, Action<string> onTargetSelected)
        {
            if (!IsEditingEnabled) return;
            pendingSourceNodeId = sourceNodeId;
            onTargetNodeSelected = onTargetSelected;
        }

        public void TryConnectTo(string targetId)
        {
            if (string.IsNullOrEmpty(pendingSourceNodeId) || pendingSourceNodeId == targetId)
            {
                CancelConnection();
                return;
            }

            try
            {
                ConnectNodes(pendingSourceNodeId, targetId);
            }
            finally
            {
                CancelConnection();
            }
        }

        public void CancelConnection()
        {
            pendingSourceNodeId = null;
            onTargetNodeSelected = null;
        }

        // ==========================
        // Path / Trigger Operations
        // ==========================
        public void SetTriggerForDialoguePath(DialogueNodeModel model, int index, string trigger)
        {
            if (index >= 0 && index < model.OutgoingPaths.Count)
            {
                model.OutgoingPaths[index].Trigger = trigger;
                MarkDirty();
            }
        }

        public void SetTriggerForMultipleChoicePath(MultipleChoiceNodeModel model, int index, string trigger)
        {
            if (index >= 0 && index < model.OutgoingPaths.Count)
            {
                model.OutgoingPaths[index].Trigger = trigger;
                MarkDirty();
            }
        }

        public void SetTriggerForTimeoutPath(TimeoutNodeModel model, bool isTimeoutPath, string trigger)
        {
            if (!IsEditingEnabled) return;

            if (isTimeoutPath)
                model.SetTriggerForTimeout(trigger);
            else
                model.SetTriggerForSuccess(trigger);

            GraphView?.RefreshNodeVisual(model.NodeID);
            MarkDirty();
        }

        // ==========================
        // Right Panel Operations
        // ==========================
        public void SetRightPanelRoot(VisualElement rightPanel)
        {
            rightPanelRoot = rightPanel;
        }

        public void OnNodeSelected(TweenityNodeModel node)
        {
            if (!string.IsNullOrEmpty(pendingSourceNodeId) && onTargetNodeSelected != null)
            {
                onTargetNodeSelected.Invoke(node.NodeID);
                CancelConnection();
                return;
            }

            if (rightPanelRoot == null) return;

            rightPanelRoot.Clear();

            switch (node)
            {
                case DialogueNodeModel m:         rightPanelRoot.Add(new Views.RightPanel.DialogueView(m, this)); break;
                case ReminderNodeModel m:         rightPanelRoot.Add(new Views.RightPanel.ReminderView(m, this)); break;
                case MultipleChoiceNodeModel m:   rightPanelRoot.Add(new Views.RightPanel.MultipleChoiceView(m, this)); break;
                case NoTypeNodeModel m:           rightPanelRoot.Add(new Views.RightPanel.NoTypeView(m, this)); break;
                case RandomNodeModel m:           rightPanelRoot.Add(new Views.RightPanel.RandomView(m, this)); break;
                case StartNodeModel m:            rightPanelRoot.Add(new Views.RightPanel.StartView(m, this)); break;
                case EndNodeModel m:              rightPanelRoot.Add(new Views.RightPanel.EndView(m, this)); break;
                case TimeoutNodeModel m:          rightPanelRoot.Add(new Views.RightPanel.TimeoutView(m, this)); break;
            }
        }

        // ==========================
        // File Operations
        // ==========================
        public void SaveCurrentGraph()
        {
            if (string.IsNullOrEmpty(lastSavedPath))
            {
                lastSavedPath = EditorUtility.SaveFilePanel("Save Graph", "", "tweenity_story.twee", "twee");
            }

            if (!string.IsNullOrEmpty(lastSavedPath))
            {
                ExportGraphTo(lastSavedPath);
                EditorPrefs.SetString("Tweenity_LastGraphPath", lastSavedPath);
                IsDirty = false;
            }
        }

        public void ExportGraphTo(string path)
        {
            string twee = GraphParser.ExportToTwee(Graph.Nodes);
            File.WriteAllText(path, twee);
        }

        public void LoadGraphFrom(string path)
        {
            string twee = File.ReadAllText(path);
            var importedNodes = GraphParser.ImportFromTwee(twee);

            ClearGraph();

            foreach (var node in importedNodes)
                AddNode(node);

            lastSavedPath = path;
            IsDirty = false;
        }

        // ==========================
        // Runtime Execution
        // ==========================
        public void StartRuntime()
        {
            if (isSimulationRunning || GraphView == null)
            {
                return;
            }

            isSimulationRunning = true;
            GraphView.SetEditingEnabled(false);

            var startNode = Graph.Nodes.FirstOrDefault(n => n.Type == NodeType.Start);
            if (startNode == null)
            {
                return;
            }

            if (simulationController == null)
                simulationController = new SimulationController();

            simulationController.SetGraphView(GraphView);
            TweenityEvents.RegisterSimulationController(simulationController);

            // Cargar el modelo directamente en el runtime
            simulationController.SetSimulationFromGraph(Graph);
        }

        // ==========================
        // Debugging Tools
        // ==========================
        public void DebugGraph()
        {
            foreach (var node in Graph.Nodes)
            {
                Debug.Log($"Node: {node.Title} ({node.Type})");
                foreach (var path in node.OutgoingPaths)
                    Debug.Log($" → {path.Label} → {path.TargetNodeID}");
            }
        }

        public void PrintCurrentSelection()
        {
            var selected = GraphView?.selection;
            if (selected == null || selected.Count == 0) return;

            foreach (var item in selected)
            {
                if (item is TweenityNode n && n.NodeModel != null)
                    Debug.Log($"→ {n.NodeID} | {n.NodeModel.Title}");
            }
        }

        public void SearchNodes(string query)
        {
            var results = Graph.Nodes
                .Where(n => n.Title.ToLower().Contains(query.ToLower()) ||
                            n.Description.ToLower().Contains(query.ToLower()))
                .ToList();

            foreach (var node in results)
                Debug.Log($"[Search] → {node.Title} ({node.Type})");
        }
    
        // ==========================
        // GraphView Utilities (UI Tools)
        // ==========================
        public TweenityNodeModel GetNode(string nodeId)
        {
            return Graph.GetNode(nodeId);
        }

        public void ToggleGrid()
        {
            GraphView?.ToggleGridVisibility();
        }

        public void ZoomIn()
        {
            GraphView?.ZoomIn();
        }

        public void ZoomOut()
        {
            GraphView?.ZoomOut();
        }

        public void ResetView()
        {
            GraphView?.ResetView();
        }

        public void ShowHelp()
        {
            Application.OpenURL("https://github.com/imagine-uniandes/Tweenity2.0/wiki");
        }
    }
}
