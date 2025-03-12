using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public static class TweenityToolbar
{
    public static Toolbar CreateToolbar()
    {
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

        // Add Node Button
        ToolbarButton addNodeButton = new ToolbarButton(() => Debug.Log("Add Node")) { text = "[ + ]" };
        toolbar.Add(addNodeButton);

        return toolbar;
    }
}
