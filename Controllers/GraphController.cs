using Models;
using Models.Nodes;
using System;
using Views;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Controllers
{
    public class GraphController
    {
        public GraphModel Graph { get; private set; }
        public TweenityGraphView GraphView { get; private set; }

        private VisualElement rightPanelRoot;

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
            Debug.Log($"Selected node: {node.Title}");

            if (rightPanelRoot == null)
            {
                Debug.LogWarning("Right panel not set.");
                return;
            }

            // Clear previous content
            rightPanelRoot.Clear();

            // Rebuild right panel based on node type
            switch (node.Type)
            {
                case NodeType.Dialogue:
                    rightPanelRoot.Add(new Nodes.DialogueView(node));
                    break;
                case NodeType.Reminder:
                    rightPanelRoot.Add(new Nodes.ReminderView(node));
                    break;
                // Add other node types as needed
                default:
                    rightPanelRoot.Add(new Nodes.NoTypeView(node));
                    break;
            }
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
            var newNode = new TweenityNodeModel("New Node", NodeType.NoType);
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
            Debug.Log("Current selection: (not implemented yet)");
            // In future: you can read from GraphView.selection and show details
        }
       public void SearchNodes(string query)
        {
            Debug.Log($"Searching nodes for: {query}");
            // Future: Filter nodes in GraphView or highlight matches
        }
 
    }
}
