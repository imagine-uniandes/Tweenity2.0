using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Models;
using Models.Nodes;
using Controllers;
using Views;

public class TweenityGraphEditor : EditorWindow
{
    [MenuItem("Window/Tweenity Graph Editor")]
    public static void OpenWindow()
    {
        TweenityGraphEditor window = GetWindow<TweenityGraphEditor>("Tweenity Graph Editor", true);
        window.titleContent = new GUIContent("Tweenity Graph Editor");
    }

    private void CreateGUI()
    {
        VisualElement root = rootVisualElement;
        root.style.flexDirection = FlexDirection.Column;
        root.style.flexGrow = 1;

        // Create GraphController
        GraphController graphController = new GraphController();

        // Create GraphView 
        TweenityGraphView graphView = new TweenityGraphView();
        graphView.style.backgroundColor = EditorGUIUtility.isProSkin
            ? new Color(0.18f, 0.18f, 0.18f)
            : new Color(0.82f, 0.82f, 0.82f);

        // Bind controller <-> view
        graphController.SetGraphView(graphView);
        graphView.SetController(graphController); // <- 💡 Nuevo binding
        graphView.OnNodeSelected = graphController.OnNodeSelected;

        // Add toolbar
        root.Add(TweenityToolbar.CreateToolbar(graphController));

        // Layout: Left - Center - Right
        VisualElement mainLayout = new VisualElement
        {
            style = { flexDirection = FlexDirection.Row, flexGrow = 1 }
        };

        mainLayout.Add(TweenityLeftPanel.CreateLeftPanel(graphController));
        mainLayout.Add(graphView);

        VisualElement rightPanelRoot = TweenityRightPanel.CreateRightPanel();
        graphController.SetRightPanelRoot(rightPanelRoot);
        mainLayout.Add(rightPanelRoot);

        root.Add(mainLayout);

        // Bottom bar
        root.Add(TweenityBottomToolbar.CreateBottomToolbar(graphController));
    }
}
