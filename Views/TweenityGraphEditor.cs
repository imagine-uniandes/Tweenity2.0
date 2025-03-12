using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Models;
using Models.Nodes;
using Controllers;
using Views.MiddlePanelView;

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

        // Graph View (Center Area)
        TweenityGraphView graphView = new TweenityGraphView();
        graphView.style.backgroundColor = EditorGUIUtility.isProSkin ? new Color(0.18f, 0.18f, 0.18f) : new Color(0.82f, 0.82f, 0.82f);

        // Initialize GraphController with the Graph View
        GraphController graphController = new GraphController(graphView);

        // Add Toolbar and pass the GraphController
        root.Add(TweenityToolbar.CreateToolbar(graphController));

        // Main Layout (Left Panel, GraphView, Right Panel)
        VisualElement mainLayout = new VisualElement();
        mainLayout.style.flexDirection = FlexDirection.Row;
        mainLayout.style.flexGrow = 1;

        // Add Left Panel
        mainLayout.Add(TweenityLeftPanel.CreateLeftPanel());

        // Add Graph View
        mainLayout.Add(graphView);

        // Add Right Panel
        mainLayout.Add(TweenityRightPanel.CreateRightPanel());

        root.Add(mainLayout);

        // Add Bottom Status Bar
        root.Add(TweenityBottomToolbar.CreateBottomToolbar());
    }

}
