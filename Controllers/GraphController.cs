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

        public GraphController(TweenityGraphView graphView)
        {
            Graph = new GraphModel();
            GraphView = graphView;
        }

        public bool AddNode(TweenityNodeModel node)
        {
            if (Graph.AddNode(node))
            {
                GraphView.AddNodeToView(node);
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
            Debug.Log("Toggle Grid clicked (controller)");
            // Future: toggle visibility of GraphView background grid
        }

        public void ZoomIn()
        {
            Debug.Log("Zoom In clicked (controller)");
            // Future: increase zoom level in GraphView
        }

        public void ZoomOut()
        {
            Debug.Log("Zoom Out clicked (controller)");
            // Future: decrease zoom level in GraphView
        }

        public void ResetView()
        {
            Debug.Log("Reset View clicked (controller)");
            // Future: reset zoom and center position
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
    }
}
