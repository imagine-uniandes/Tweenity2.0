using UnityEditor;
using UnityEngine;
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
        Debug.Log("Trigger assignment started. Select an object from the scene.");
    }

    public static void ConfirmObjectSelection()
    {
        GameObject selected = Selection.activeGameObject;

        // 🧱 Ignore prefabs or project assets
        if (selected == null || !selected.scene.IsValid())
        {
            Debug.LogWarning("Selected object is not part of the active scene. Please select a placed GameObject.");
            return;
        }

        // 🔁 Allow re-selection of the same object (force UI refresh)
        _selectedObject = selected;
        Debug.Log($"Selected object for trigger: {_selectedObject.name}");

        _onObjectSelectedImmediate?.Invoke();
        _onObjectSelectedImmediate = null;
    }

    public static List<string> GetAvailableEvents(GameObject obj)
    {
        List<string> events = new();

        if (obj == null) return events;

        if (obj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>() != null)
            events.Add("used");

        var collider = obj.GetComponent<Collider>();
        if (collider != null && collider.isTrigger)
            events.Add("entered");

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
