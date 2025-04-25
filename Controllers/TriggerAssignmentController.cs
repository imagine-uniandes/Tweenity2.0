using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System;
using System.Collections.Generic;

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
        Debug.Log("Trigger assignment started. Select an object from the scene or Hierarchy.");
    }

    public static void ConfirmObjectSelection()
    {
        GameObject selected = Selection.activeGameObject;

        if (selected == null)
        {
            Debug.LogWarning("No object selected.");
            return;
        }

        // Ignore prefab assets (project assets not in the scene)
        var prefabType = PrefabUtility.GetPrefabAssetType(selected);
        if (prefabType != PrefabAssetType.NotAPrefab && !selected.scene.IsValid())
        {
            Debug.LogWarning("Selected object is a prefab asset. Please select an instance from the scene (Hierarchy).");
            return;
        }

        // 🔁 Allow re-selection of the same object (force UI refresh)
        _selectedObject = selected;
        Debug.Log($"🎯 Object selected from Hierarchy: {_selectedObject.name}", _selectedObject);

        _onObjectSelectedImmediate?.Invoke();
        _onObjectSelectedImmediate = null;
    }

    public static List<string> GetAvailableEvents(GameObject obj)
    {
        List<string> events = new();

        if (obj == null) return events;

        var objectController = obj.GetComponent<ObjectController>();
        if (objectController != null)
        {
            events.AddRange(objectController.GetAvailableTriggerMethods());
        }

        return events;
    }

    public static void SaveTrigger(string selectedEvent)
    {
        if (_selectedObject == null)
        {
            Debug.LogWarning("No valid object selected. Trigger not saved.");
            return;
        }

        if (_onTriggerConfirmed == null)
        {
            Debug.LogWarning("No trigger confirmation callback set.");
            return;
        }

        string trigger = $"{_selectedObject.name}:{selectedEvent}";
        Debug.Log($"✅ Trigger saved: {trigger}", _selectedObject);
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
