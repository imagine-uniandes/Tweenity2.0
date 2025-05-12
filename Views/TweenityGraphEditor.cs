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

        // Create controller
        GraphController graphController = new GraphController();
        TweenityGraphView graphView = new TweenityGraphView();
        graphController.SetGraphView(graphView);
        graphView.SetController(graphController);
        graphView.OnNodeSelected = graphController.OnNodeSelected;

        // Setup UI
        root.Add(TweenityToolbar.CreateToolbar(graphController));

        VisualElement mainLayout = new VisualElement { style = { flexDirection = FlexDirection.Row, flexGrow = 1 } };
        mainLayout.Add(graphView);

        VisualElement rightPanelRoot = TweenityRightPanel.CreateRightPanel();
        graphController.SetRightPanelRoot(rightPanelRoot);
        mainLayout.Add(rightPanelRoot);

        root.Add(mainLayout);
        root.Add(TweenityBottomToolbar.CreateBottomToolbar(graphController));

        // Defer graph restore after Editor is ready
        EditorApplication.delayCall += () =>
        {
            string lastPath = EditorPrefs.GetString("Tweenity_LastGraphPath", "");
            if (!string.IsNullOrEmpty(lastPath) && File.Exists(lastPath))
            {
                try
                {
                    string twee = File.ReadAllText(lastPath);
                    var importedNodes = GraphParser.ImportFromTwee(twee);

                    graphController.ClearGraph();
                    foreach (var node in importedNodes)
                        graphController.AddNode(node);
                }
                catch (Exception e)
                {
                    Debug.LogError($"❌ Failed to load graph after PlayMode reload: {e.Message}");
                }
            }
        };

#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif

        // Runtime startup check
        if (EditorApplication.isPlaying)
        {
            EditorApplication.delayCall += () =>
            {
                if (GraphController.ActiveEditorGraphController != null)
                {
                    GraphController.ActiveEditorGraphController.StartRuntime();
                }
                else
                {
                    Debug.LogWarning("⚠️ Tried to start runtime but ActiveEditorGraphController is still null.");
                }
            };
        }
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {

        if (state == PlayModeStateChange.EnteredPlayMode)
        {

            var controller = GraphController.ActiveEditorGraphController;
            if (controller == null)
            {
                return;
            }

            controller.StartRuntime();
        }
        else if (state == PlayModeStateChange.ExitingPlayMode)
        {

            var controller = GraphController.ActiveEditorGraphController;
            if (controller != null)
            {
                // 1. Marcar simulación como terminada
                var simField = typeof(GraphController).GetField("isSimulationRunning", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                simField?.SetValue(controller, false);

                // 2. Reactivar edición
                controller.GraphView?.SetEditingEnabled(true);

                // 3. Recargar grafo desde el último archivo guardado
                string lastPath = EditorPrefs.GetString("Tweenity_LastGraphPath", "");
                if (!string.IsNullOrEmpty(lastPath) && File.Exists(lastPath))
                {
                    controller.LoadGraphFrom(lastPath);
                }
            }
        }
    }
}
