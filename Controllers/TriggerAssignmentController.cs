using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System;
using System.Collections.Generic;
using Controllers;

#if UNITY_EDITOR
[InitializeOnLoad]
public static class TriggerAssignmentController
{
    private static Action<string> _onTriggerConfirmed;
    private static Action _onObjectSelectedImmediate;
    private static GameObject _selectedObject;

    public static bool IsListening => _onTriggerConfirmed != null;
    public static GameObject SelectedObject => _selectedObject;

    static TriggerAssignmentController()
    {
        Selection.selectionChanged += OnUnitySelectionChanged;
    }

    private static void OnUnitySelectionChanged()
    {
        if (IsListening)
        {
            ConfirmObjectSelection();
        }
    }

    public static void Start(Action<string> onTriggerSelected, Action onObjectSelectedImmediate = null)
    {
        _onTriggerConfirmed = onTriggerSelected;
        _onObjectSelectedImmediate = onObjectSelectedImmediate;
        _selectedObject = null;
    }

    public static void ConfirmObjectSelection()
    {
        GameObject selected = Selection.activeGameObject;

        if (selected == null)
        {
            return;
        }

        // Ignore prefab assets (project assets not in the scene)
        var prefabType = PrefabUtility.GetPrefabAssetType(selected);
        if (prefabType != PrefabAssetType.NotAPrefab && !selected.scene.IsValid())
        {
            return;
        }

        // Allow re-selection of the same object (force UI refresh)
        _selectedObject = selected;
        _onObjectSelectedImmediate?.Invoke();
        _onObjectSelectedImmediate = null;
    }

    public static List<string> GetAvailableEvents(GameObject obj)
    {
        if (obj == null) return new List<string>();

        return Controllers.ObjectController.GetAvailableTriggerMethods(obj);
    }

    public static void SaveTrigger(string selectedEvent)
    {
        if (_selectedObject == null)
        {
            return;
        }

        if (_onTriggerConfirmed == null)
        {
            return;
        }

        string trigger = $"{_selectedObject.name}:{selectedEvent}";
        _onTriggerConfirmed?.Invoke(trigger);
        Reset();
    }

    public static void Cancel() => Reset();

    private static void Reset()
    {
        _onTriggerConfirmed = null;
        _onObjectSelectedImmediate = null;
        _selectedObject = null;
    }

    public static void SetSelectedObjectManually(GameObject go)
    {
        _selectedObject = go;
    }
}
#endif
