#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Models;
using Models.Nodes;
using Controllers;
using Views;
using System.IO;
using System;


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
        graphView.SetController(graphController);
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

        // ✅ Restaurar automáticamente desde último path guardado
        string lastPath = EditorPrefs.GetString("Tweenity_LastGraphPath", "");
        if (!string.IsNullOrEmpty(lastPath) && File.Exists(lastPath))
        {
            try
            {
                string twee = File.ReadAllText(lastPath);
                var importedNodes = GraphParser.ImportFromTwee(twee);

                // Limpiar vista y modelo antes de restaurar
                graphController.ClearGraph();

                EditorApplication.delayCall += () =>
                {
                    foreach (var node in importedNodes)
                        graphController.AddNode(node);

                    Debug.Log($"✅ Graph restored from last saved file: {lastPath} ({importedNodes.Count} nodes)");
                };
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Failed to load graph from last saved path: {e.Message}");
            }
        }

        #if UNITY_EDITOR
    EditorApplication.playModeStateChanged += (PlayModeStateChange state) =>
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            Debug.Log("▶ Play Mode entered. Checking graph...");

            var controller = GraphController.ActiveEditorGraphController;
            if (controller == null)
            {
                Debug.LogWarning("GraphController not found. Cannot start simulation.");
                return;
            }

            if (EditorUtility.DisplayDialog("Save Graph?", "Do you want to save the current graph before starting simulation?", "Save and Start", "Start without Saving"))
            {
                controller.SaveCurrentGraph();
            }

            controller.StartRuntime();
        }
    };
    #endif

    }

}
