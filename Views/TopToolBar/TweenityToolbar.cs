using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Controllers; 
using Models.Nodes; 
using Models; 
using Views; 

namespace Views
{
    public static class TweenityToolbar
    {
        private static GraphController _graphController; 

        public static Toolbar CreateToolbar(GraphController graphController)
        {
            _graphController = graphController;

            Toolbar toolbar = new Toolbar();

            // File Dropdown Menu
            ToolbarMenu fileMenu = new ToolbarMenu() { text = "File" };

            fileMenu.menu.AppendAction("Save", action =>
            {
                _graphController.SaveCurrentGraph(); 
            });

            fileMenu.menu.AppendAction("Export", action =>
            {
                string path = EditorUtility.SaveFilePanel("Export Graph", "", "tweenity_story.twee", "twee");
                if (!string.IsNullOrEmpty(path))
                {
                    _graphController.ExportGraphTo(path);
                }
            });

            fileMenu.menu.AppendAction("Open", action =>
            {
                string path = EditorUtility.OpenFilePanel("Open Graph", "", "twee");
                if (!string.IsNullOrEmpty(path))
                {
                    _graphController.LoadGraphFrom(path);
                }
            });

            toolbar.Add(fileMenu);

            // View Dropdown Menu
            ToolbarMenu viewMenu = new ToolbarMenu() { text = "View" };

            viewMenu.menu.AppendAction("Toggle Grid", action =>
            {
                _graphController.ToggleGrid();
            });

            viewMenu.menu.AppendAction("Zoom In", action =>
            {
                _graphController.ZoomIn();
            });

            viewMenu.menu.AppendAction("Zoom Out", action =>
            {
                _graphController.ZoomOut();
            });

            viewMenu.menu.AppendAction("Reset View", action =>
            {
                _graphController.ResetView();
            });

            ToolbarButton helpButton = new ToolbarButton(() =>
            {
                _graphController.ShowHelp();
            })
            {
                text = "Help"
            };
            toolbar.Add(helpButton);

            // Clear All Button 
            ToolbarButton clearAllButton = new ToolbarButton(() =>
            {
                if (EditorUtility.DisplayDialog("Clear Graph", "Are you sure you want to clear the entire graph?", "Yes", "Cancel"))
                {
                    _graphController.ClearGraph();
                }
            })
            {
                text = "Clear"
            };
            toolbar.Add(clearAllButton);

            // Add Node Button
            ToolbarButton addNodeButton = new ToolbarButton(() =>
            {
                _graphController.CreateNewNode();
            })
            {
                text = "[ + ]"
            };
            toolbar.Add(addNodeButton);
            
            return toolbar;
        }
    }
}
