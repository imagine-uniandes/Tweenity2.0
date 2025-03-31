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
            if (Graph.AddNode(node))
            {
                GraphView.RenderNode(node);
                return true;
            }
            return false;
        }

        public void RemoveNode(string nodeId)
        {
            Graph.RemoveNode(nodeId);
            GraphView.RemoveNodeFromView(nodeId);
        }

        public TweenityNodeModel GetNode(string nodeId)
        {
            return Graph.GetNode(nodeId);
        }

        public List<TweenityNodeModel> GetAllNodes()
        {
            return Graph.Nodes;
        }

        public void SetRightPanelRoot(VisualElement rightPanel)
        {
            rightPanelRoot = rightPanel;
        }

        public void OnNodeSelected(TweenityNodeModel node)
        {
            if (rightPanelRoot == null)
            {
                Debug.LogWarning("Right panel root not set.");
                return;
            }

            Debug.Log($"[Selection] Title: {node.Title}, Type: {node.Type}, Actual Type: {node.GetType().Name}");

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

        public void ChangeNodeType(TweenityNodeModel oldModel, NodeType newType)
        {
            // Don't do anything if the type hasn't changed
            if (oldModel.Type == newType)
                return;

            // Log the old and new types for debugging
            Debug.Log($"Changing node type: Old Type = {oldModel.Type}, New Type = {newType}");

            // Create new model of the selected type
            TweenityNodeModel newModel;
            string title = oldModel.Title;
            string description = oldModel.Description;
            List<string> connections = new List<string>(oldModel.ConnectedNodes);

            switch (newType)
            {
                case NodeType.Dialogue:
                    newModel = new DialogueNodeModel(title);
                    break;
                case NodeType.Reminder:
                    newModel = new ReminderNodeModel(title);
                    break;
                case NodeType.MultipleChoice:
                    newModel = new MultipleChoiceNodeModel(title);
                    break;
                case NodeType.Random:
                    newModel = new RandomNodeModel(title);
                    break;
                case NodeType.Start:
                    newModel = new StartNodeModel(title);
                    break;
                case NodeType.End:
                    newModel = new EndNodeModel(title);
                    break;
                case NodeType.Timeout:
                    newModel = new TimeoutNodeModel(title);
                    break;
                default:
                    newModel = new NoTypeNodeModel(title);
                    break;
            }

            newModel.Description = description;
            newModel.ConnectedNodes = connections;

            // Remove old node from Graph and GraphView
            Graph.RemoveNode(oldModel.NodeID);
            GraphView.RemoveNodeFromView(oldModel.NodeID);
            Debug.Log($"Removed old node: {oldModel.NodeID}");

            // Add new node to Graph and GraphView
            Graph.AddNode(newModel);
            GraphView.RenderNode(newModel);
            Debug.Log($"Added new node: {newModel.NodeID}");

            // Re-add visual elements explicitly (if needed)
            var visualNode = GraphView.Children()
                .OfType<TweenityNode>()
                .FirstOrDefault(n => n.NodeID == newModel.NodeID);
            
            if (visualNode != null)
            {
                // Refresh or update the visual node (force re-rendering)
               GraphView.RefreshNodeVisual(newModel.NodeID);
            }

            // Refresh right panel with new node
            GraphView.RefreshNodeVisual(newModel.NodeID); 
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

            // Clear current graph
            foreach (var node in Graph.Nodes)
            {
                GraphView.RemoveNodeFromView(node.NodeID);
            }

            Graph = new GraphModel(); // Reset
            foreach (var node in nodes)
            {
                AddNode(node);
            }

            lastSavedPath = path;
            Debug.Log("Graph loaded from: " + path);
        }
        // View control stubs
        public void ToggleGrid()
        {
            GraphView.ToggleGridVisibility();
        }

        public void ZoomIn()
        {
            GraphView.ZoomIn();
        }

        public void ZoomOut()
        {
            GraphView.ZoomOut();
        }

        public void ResetView()
        {
            GraphView.ResetView();
        }

        public void ShowHelp()
        {
            Debug.Log("Help clicked (controller)");
            // Future: open documentation window or help dialog
        }

        public void CreateNewNode()
        {
            var newNode = new NoTypeNodeModel("New Node");

            bool added = AddNode(newNode);
            if (!added)
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
            // Future: Filter nodes in GraphView or highlight matches
        }

        public void UpdateDialogueText(DialogueNodeModel model, string newText)
        {
            model.DialogueText = newText;
        }

        public void AddDialogueResponse(DialogueNodeModel model)
        {
            model.AddResponse("Response " + (model.Responses.Count + 1));
        }

        public void AddChoiceToMultipleChoiceNode(MultipleChoiceNodeModel model)
        {
            model.AddChoice("Choice " + (model.Choices.Count + 1));
        }
        public void UpdateNoTypeTitle(NoTypeNodeModel model, string newTitle)
        {
            model.Title = newTitle;
        }

        public void UpdateNoTypeDescription(NoTypeNodeModel model, string newDesc)
        {
            model.Description = newDesc;
        }
        public void AddRandomPath(RandomNodeModel model)
        {
            model.AddPath("Path " + (model.PossiblePaths.Count + 1));
        }

        public void UpdateReminderText(ReminderNodeModel model, string newText)
        {
            model.ReminderText = newText;
        }

        public void UpdateReminderTimer(ReminderNodeModel model, float newTimer)
        {
            model.ReminderTimer = newTimer;
        }

        public void UpdateTimeoutCondition(TimeoutNodeModel model, string newCondition)
        {
            model.Condition = newCondition;
        }

        public void UpdateTimeoutTimer(TimeoutNodeModel model, float newDuration)
        {
            model.TimeoutDuration = newDuration;
        }

    }
}
