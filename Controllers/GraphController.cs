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
        public GraphModel Graph { get; private set; } = new GraphModel();
        public TweenityGraphView GraphView { get; private set; }

        private VisualElement rightPanelRoot;
        private string lastSavedPath;

        public bool IsEditingEnabled { get; set; } = true;

        // private SimulationRuntimeEngine runtimeEngine;
        private bool isSimulationRunning = false;

        public GraphController() {}

        public void SetGraphView(TweenityGraphView graphView)
        {
            GraphView = graphView;
        }

        public bool AddNode(TweenityNodeModel node)
        {
            if (!IsEditingEnabled)
            {
                Debug.LogWarning("[GraphController] AddNode blocked — editing is disabled.");
                return false;
            }

            Debug.Log($"[GraphController]  AddNode called for: {node.Title} | ID: {node.NodeID}");

            if (Graph.AddNode(node))
            {
                Debug.Log($"[GraphController]  Node added to model. Now rendering...");
                GraphView?.RenderNode(node);
                Debug.Log($"[GraphController]  RenderNode completed for: {node.NodeID}");
                return true;
            }

            Debug.LogWarning($"[GraphController]  Node not added. It may already exist or conflict (e.g., Start node)");
            return false;
        }

        public void RemoveNode(string nodeId)
        {
            if (!IsEditingEnabled)
            {
                Debug.LogWarning("[GraphController] RemoveNode blocked — editing is disabled.");
                return;
            }

            if (GraphView == null) return;

            CleanupConnectionsTo(nodeId);

            var node = Graph.GetNode(nodeId);
            if (node != null)
            {
                foreach (var path in node.OutgoingPaths.ToList())
                {
                    node.DisconnectFrom(path.TargetNodeID);
                }
            }

            GraphView.RemoveNodeFromView(nodeId);
            Graph.RemoveNode(nodeId);

            GraphView.RenderConnections();
            foreach (var n in Graph.Nodes)
                GraphView.RefreshNodeVisual(n.NodeID);
        }

        private void CleanupConnectionsTo(string deletedNodeId)
        {
            if (!IsEditingEnabled)
            {
                Debug.LogWarning("[GraphController] CleanupConnectionsTo blocked — editing is disabled.");
                return;
            }

            foreach (var node in Graph.Nodes)
            {
                if (node.IsConnectedTo(deletedNodeId))
                {
                    node.DisconnectFrom(deletedNodeId);
                    GraphView?.RefreshNodeVisual(node.NodeID);
                }
            }
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
                case DialogueNodeModel m:         rightPanelRoot.Add(new Views.RightPanel.DialogueView(m, this)); break;
                case ReminderNodeModel m:         rightPanelRoot.Add(new Views.RightPanel.ReminderView(m, this)); break;
                case MultipleChoiceNodeModel m:   rightPanelRoot.Add(new Views.RightPanel.MultipleChoiceView(m, this)); break;
                case NoTypeNodeModel m:           rightPanelRoot.Add(new Views.RightPanel.NoTypeView(m, this)); break;
                case RandomNodeModel m:           rightPanelRoot.Add(new Views.RightPanel.RandomView(m, this)); break;
                case StartNodeModel m:            rightPanelRoot.Add(new Views.RightPanel.StartView(m, this)); break;
                case EndNodeModel m:              rightPanelRoot.Add(new Views.RightPanel.EndView(m, this)); break;
                case TimeoutNodeModel m:          rightPanelRoot.Add(new Views.RightPanel.TimeoutView(m, this)); break;
                default: Debug.LogWarning("Unknown node type selected."); break;
            }
        }

        public void UpdateNodeTitle(TweenityNodeModel model, string newTitle)
        {
            if (!IsEditingEnabled)
            {
                Debug.LogWarning("[GraphController] UpdateNodeTitle blocked — editing is disabled.");
                return;
            }

            if (GraphView == null) return;

            model.Title = newTitle;
            GraphView.UpdateNodeTitle(model.NodeID, newTitle);
            GraphView.RefreshNodeVisual(model.NodeID);
        }

        public void UpdateNodeDescription(TweenityNodeModel model, string newDescription)
        {
            if (!IsEditingEnabled)
            {
                Debug.LogWarning("[GraphController] UpdateNodeDescription blocked — editing is disabled.");
                return;
            }

            if (GraphView == null) return;

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
            var importedNodes = GraphParser.ImportFromTwee(twee);

            foreach (var node in Graph.Nodes)
                GraphView?.RemoveNodeFromView(node.NodeID);

            Graph = new GraphModel();
            var titleToId = new Dictionary<string, string>();

            foreach (var node in importedNodes)
            {
                AddNode(node);
                titleToId[node.Title] = node.NodeID;
            }

            foreach (var node in importedNodes)
            {
                foreach (var pathData in node.OutgoingPaths)
                {
                    if (titleToId.TryGetValue(pathData.TargetNodeID, out var newId))
                        pathData.TargetNodeID = newId;
                }
            }

            lastSavedPath = path;
            Debug.Log("Graph loaded from: " + path);
        }

        public void ToggleGrid() => GraphView?.ToggleGridVisibility();
        public void ZoomIn() => GraphView?.ZoomIn();
        public void ZoomOut() => GraphView?.ZoomOut();
        public void ResetView() => GraphView?.ResetView();
        public void ShowHelp() => Debug.Log("Help clicked (controller)");

        public void CreateNewNode()
        {
            if (!IsEditingEnabled)
            {
                Debug.LogWarning("[GraphController] CreateNewNode blocked — editing is disabled.");
                return;
            }

            if (GraphView == null) return;

            var newNode = new NoTypeNodeModel("New Node");
            Rect baseRect = new Rect(200, 200, 150, 200);
            Rect position = baseRect;

            var existingRects = GraphView.graphElements
                .OfType<TweenityNode>()
                .Select(n => n.GetPosition())
                .ToList();

            int attempts = 0;
            while (existingRects.Any(r => r.Overlaps(position)))
            {
                position.position += new Vector2(30, 30);
                if (++attempts > 100) break;
            }

            newNode.Position = position.position;

            if (Graph.AddNode(newNode))
                GraphView.RenderNode(newNode, position);
        }

        public void ConnectNodes(string fromNodeId, string toNodeId)
        {
            if (!IsEditingEnabled)
            {
                Debug.LogWarning("[GraphController] ConnectNodes blocked — editing is disabled.");
                return;
            }

            if (GraphView == null) return;

            var fromNode = Graph.GetNode(fromNodeId);
            var toNode = Graph.GetNode(toNodeId);

            if (fromNode == null || toNode == null)
                return;

            if (fromNode.Type == NodeType.End)
            {
        #if UNITY_EDITOR
                EditorUtility.DisplayDialog("Invalid Connection", "End nodes cannot have outgoing connections.", "OK");
        #endif
                return;
            }

            if ((fromNode.Type == NodeType.Start || fromNode.Type == NodeType.NoType || fromNode.Type == NodeType.Reminder) &&
                fromNode.OutgoingPaths.Count >= 1)
            {
        #if UNITY_EDITOR
                EditorUtility.DisplayDialog("Connection Limit Reached", $"{fromNode.Type} nodes can only have one outgoing connection.", "OK");
        #endif
                return;
            }

            if (!fromNode.IsConnectedTo(toNodeId))
            {
                fromNode.ConnectTo(toNodeId);
                GraphView.RenderConnections();
            }
        }

        public void DisconnectNodes(string fromNodeId, string toNodeId)
        {
            if (!IsEditingEnabled)
            {
                Debug.LogWarning("[GraphController] DisconnectNodes blocked — editing is disabled.");
                return;
            }

            if (GraphView == null) return;

            var fromNode = Graph.GetNode(fromNodeId);
            if (fromNode != null && fromNode.IsConnectedTo(toNodeId))
            {
                fromNode.DisconnectFrom(toNodeId);
            }
        }

        private string pendingSourceNodeId;
        private Action<string> onTargetNodeSelected;

        public void StartConnectionFrom(string sourceNodeId, Action<string> onTargetSelected)
        {
            if (!IsEditingEnabled)
            {
                Debug.LogWarning("[GraphController] StartConnectionFrom blocked — editing is disabled.");
                return;
            }

            pendingSourceNodeId = sourceNodeId;
            onTargetNodeSelected = onTargetSelected;
        }

        public void TryConnectTo(string targetId)
        {
            if (!IsEditingEnabled)
            {
                Debug.LogWarning("[GraphController] TryConnectTo blocked — editing is disabled.");
                pendingSourceNodeId = null; // ensure we clean up state
                return;
            }

            if (string.IsNullOrEmpty(pendingSourceNodeId) || pendingSourceNodeId == targetId)
                return;

            ConnectNodes(pendingSourceNodeId, targetId);
            pendingSourceNodeId = null;
        }
        public void CancelConnection()
        {
            pendingSourceNodeId = null;
            onTargetNodeSelected = null;
        }


        public (bool, string) ChangeNodeType(TweenityNodeModel oldModel, NodeType newType)
        {
            if (!IsEditingEnabled)
            {
                Debug.LogWarning("[GraphController] ChangeNodeType blocked — editing is disabled.");
                return (false, "Editing is disabled.");
            }

            if (GraphView == null) return (false, "GraphView not available.");
            if (oldModel.Type == newType) return (true, null);

            if (newType == NodeType.Start && Graph.Nodes.Any(n => n.Type == NodeType.Start && n.NodeID != oldModel.NodeID))
                return (false, "Only one Start node is allowed in the graph.");

            Vector2 pos = GraphView.graphElements
                .OfType<TweenityNode>()
                .FirstOrDefault(n => n.NodeID == oldModel.NodeID)
                ?.GetPosition().position ?? new Vector2(200, 200);

            RemoveNode(oldModel.NodeID);

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

            newModel.Description = oldModel.Description;
            newModel.Position = pos;

            Graph.AddNode(newModel);
            GraphView.RenderNode(newModel, new Rect(pos, new Vector2(150, 200)));
            OnNodeSelected(newModel);
            return (true, null);
        }

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
            if (selected == null || selected.Count == 0)
            {
                Debug.Log("[PrintSelection] No nodes selected.");
                return;
            }

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

        public void UpdateReminderText(ReminderNodeModel model, string newText)
        {
            if (!IsEditingEnabled)
            {
                Debug.LogWarning("[GraphController] UpdateReminderText blocked — editing is disabled.");
                return;
            }

            if (GraphView == null) return;

            model.ReminderText = newText;
            GraphView.RefreshNodeVisual(model.NodeID);
        }

        public void UpdateReminderTimer(ReminderNodeModel model, float newTimer)
        {
            if (!IsEditingEnabled)
            {
                Debug.LogWarning("[GraphController] UpdateReminderTimer blocked — editing is disabled.");
                return;
            }

            if (GraphView == null) return;

            model.ReminderTimer = newTimer;
            GraphView.RefreshNodeVisual(model.NodeID);
        }

        public void AddRandomPath(RandomNodeModel model)
        {
            if (!IsEditingEnabled)
            {
                Debug.LogWarning("[GraphController] AddRandomPath blocked — editing is disabled.");
                return;
            }

            if (GraphView == null) return;

            model.OutgoingPaths.Add(new PathData($"Path {model.OutgoingPaths.Count + 1}"));
            GraphView.RefreshNodeVisual(model.NodeID);
        }

        public void UpdateDialogueText(DialogueNodeModel model, string newText)
        {
            if (!IsEditingEnabled)
            {
                Debug.LogWarning("[GraphController] UpdateDialogueText blocked — editing is disabled.");
                return;
            }

            if (GraphView == null) return;

            model.DialogueText = newText;
            GraphView.RefreshNodeVisual(model.NodeID);
        }

        public void AddDialogueResponse(DialogueNodeModel model)
        {
            if (!IsEditingEnabled)
            {
                Debug.LogWarning("[GraphController] AddDialogueResponse blocked — editing is disabled.");
                return;
            }

            if (GraphView == null) return;

            model.AddResponse($"Response {model.OutgoingPaths.Count + 1}");
            GraphView.RefreshNodeVisual(model.NodeID);
        }

        public void SetTriggerForDialogueResponse(DialogueNodeModel model, int index, string trigger)
        {
            if (!IsEditingEnabled)
            {
                Debug.LogWarning("[GraphController] SetTriggerForDialogueResponse blocked — editing is disabled.");
                return;
            }

            model.SetTriggerForResponse(index, trigger);
            GraphView?.RefreshNodeVisual(model.NodeID);
        }

        public void SetTriggerForMultipleChoiceOption(MultipleChoiceNodeModel model, int index, string trigger)
        {
            if (!IsEditingEnabled)
            {
                Debug.LogWarning("[GraphController] SetTriggerForMultipleChoiceOption blocked — editing is disabled.");
                return;
            }

            model.SetTriggerForOption(index, trigger);
            GraphView?.RefreshNodeVisual(model.NodeID);
        }

        public void SetTriggerForTimeoutPath(TimeoutNodeModel model, bool isTimeoutPath, string trigger)
        {
            if (!IsEditingEnabled)
            {
                Debug.LogWarning("[GraphController] SetTriggerForTimeoutPath blocked — editing is disabled.");
                return;
            }

            if (isTimeoutPath)
                model.SetTriggerForTimeout(trigger);
            else
                model.SetTriggerForSuccess(trigger);

            GraphView?.RefreshNodeVisual(model.NodeID);
        }


        // public void StartRuntime()
        // {
        //     if (isSimulationRunning || GraphView == null)
        //     {
        //         Debug.LogWarning("Runtime already running or graphView not set.");
        //         return;
        //     }

        //     isSimulationRunning = true;

        //     // Bloquear edición y mostrar mensaje
        //     GraphView.SetEditingEnabled(false);

        //     // Buscar nodo de inicio
        //     var startNode = Graph.Nodes.FirstOrDefault(n => n.Type == NodeType.Start);
        //     if (startNode == null)
        //     {
        //         Debug.LogError("No Start node found in the graph.");
        //         return;
        //     }

        //     // Instanciar el motor de ejecución
        //     runtimeEngine = new SimulationRuntimeEngine(Graph, this, HighlightNode);
        //     runtimeEngine.RunFrom(startNode);
        // }
        // public void StopRuntime()
        // {
        //     if (!isSimulationRunning) return;
        //     isSimulationRunning = false;
        //     GraphView.SetEditingEnabled(true);
        //     runtimeEngine = null;
        // }

    }
}
