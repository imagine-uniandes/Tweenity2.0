using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Controllers; // So we can access GraphController
using Models.Nodes; // For TweenityNodeModel
using Models; // For NodeType
using Views; // Assuming this is your shared namespace

namespace Views
{
    public static class TweenityToolbar
    {
        private static GraphController _graphController; // stored reference

        public static Toolbar CreateToolbar(GraphController graphController)
        {
            _graphController = graphController;

            Toolbar toolbar = new Toolbar();

            // File Dropdown Menu
            ToolbarMenu fileMenu = new ToolbarMenu() { text = "File" };
            fileMenu.menu.AppendAction("Save", action => Debug.Log("Save Clicked"));
            fileMenu.menu.AppendAction("Export", action => Debug.Log("Export Clicked"));
            fileMenu.menu.AppendAction("Import", action => Debug.Log("Import Clicked"));
            toolbar.Add(fileMenu);

            // View Dropdown Menu
            ToolbarMenu viewMenu = new ToolbarMenu() { text = "View" };
            viewMenu.menu.AppendAction("Toggle Grid", action => Debug.Log("Toggle Grid Clicked"));
            viewMenu.menu.AppendAction("Zoom In", action => Debug.Log("Zoom In Clicked"));
            viewMenu.menu.AppendAction("Zoom Out", action => Debug.Log("Zoom Out Clicked"));
            viewMenu.menu.AppendAction("Reset View", action => Debug.Log("Reset View Clicked"));
            toolbar.Add(viewMenu);

            // Help Button 
            ToolbarButton helpButton = new ToolbarButton(() => Debug.Log("Help Clicked")) { text = "Help" };
            toolbar.Add(helpButton);

            // Add Node Button (wired up to controller)
            ToolbarButton addNodeButton = new ToolbarButton(() =>
            {
                var newNode = new TweenityNodeModel("New Node", NodeType.NoType);
                bool added = _graphController.AddNode(newNode);
                if (!added)
                {
                    Debug.LogWarning("Node not added. Maybe a Start node already exists?");
                }
            })
            {
                text = "[ + ]"
            };
            toolbar.Add(addNodeButton);

            return toolbar;
        }
    }
}
