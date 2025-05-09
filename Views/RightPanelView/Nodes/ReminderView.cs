using UnityEngine;
using UnityEngine.UIElements;
using Models.Nodes;
using Controllers;
using System.Collections.Generic;
using UnityEditor.UIElements;

namespace Views.RightPanel
{
    public class ReminderView : TweenityNodeView
    {
        private ObjectField successObjectField;
        private PopupField<string> successTriggerDropdown;
        private List<string> availableSuccessTriggers = new();
        private string selectedSuccessEvent = "";

        private ObjectField reminderObjectField;
        private PopupField<string> reminderBehaviorDropdown;
        private List<string> availableReminderTriggers = new();
        private string selectedReminderBehavior = "";

        public ReminderView(ReminderNodeModel model, GraphController controller) : base(model, controller)
        {
            var title = new Label("Reminder Node Details");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.whiteSpace = WhiteSpace.Normal;
            Add(title);

            var typedModel = (ReminderNodeModel)_model;

            Add(new Label("Success Trigger Assignment")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 }
            });

            successObjectField = new ObjectField("Success Target Object");
            successObjectField.objectType = typeof(GameObject);
            successObjectField.value = null;
            successObjectField.RegisterValueChangedCallback(OnSuccessObjectSelected);
            Add(successObjectField);

            successTriggerDropdown = new PopupField<string>("Success Trigger Method", availableSuccessTriggers, 0);
            successTriggerDropdown.SetEnabled(false);
            successTriggerDropdown.RegisterValueChangedCallback(OnSuccessTriggerSelected);
            Add(successTriggerDropdown);

            Add(new Label("Reminder Behavior Assignment")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 }
            });

            reminderObjectField = new ObjectField("Reminder Target Object");
            reminderObjectField.objectType = typeof(GameObject);
            reminderObjectField.value = null;
            reminderObjectField.RegisterValueChangedCallback(OnReminderObjectSelected);
            Add(reminderObjectField);

            reminderBehaviorDropdown = new PopupField<string>("Reminder Behavior Method", availableReminderTriggers, 0);
            reminderBehaviorDropdown.SetEnabled(false);
            reminderBehaviorDropdown.RegisterValueChangedCallback(OnReminderBehaviorSelected);
            Add(reminderBehaviorDropdown);

            // 🚀 After UI fields are created, try auto-loading saved triggers!

            // Try preload Success Trigger
            if (typedModel.OutgoingPaths.Count >= 1 && !string.IsNullOrEmpty(typedModel.OutgoingPaths[0].Trigger))
            {
                var trigger = typedModel.OutgoingPaths[0].Trigger;
                var parts = trigger.Split(':');
                if (parts.Length == 2)
                {
                    string objectName = parts[0];
                    string methodInfo = parts[1];

                    GameObject obj = GameObject.Find(objectName);
                    if (obj != null)
                    {
                        successObjectField.SetValueWithoutNotify(obj);

                        availableSuccessTriggers = TriggerAssignmentController.GetAvailableEvents(obj);
                        successTriggerDropdown.choices = availableSuccessTriggers;

                        if (availableSuccessTriggers.Contains(methodInfo))
                            successTriggerDropdown.SetValueWithoutNotify(methodInfo);
                        else if (availableSuccessTriggers.Count > 0)
                            successTriggerDropdown.SetValueWithoutNotify(availableSuccessTriggers[0]);

                        successTriggerDropdown.SetEnabled(true);
                    }
                }
            }

            // Try preload Reminder Behavior
            if (typedModel.OutgoingPaths.Count >= 2 && !string.IsNullOrEmpty(typedModel.OutgoingPaths[1].Trigger))
            {
                var reminderTrigger = typedModel.OutgoingPaths[1].Trigger;
                var parts = reminderTrigger.Split(':');
                if (parts.Length == 2)
                {
                    string objectName = parts[0];
                    string methodInfo = parts[1];

                    GameObject obj = GameObject.Find(objectName);
                    if (obj != null)
                    {
                        reminderObjectField.SetValueWithoutNotify(obj);

                        availableReminderTriggers = TriggerAssignmentController.GetAvailableEvents(obj);
                        reminderBehaviorDropdown.choices = availableReminderTriggers;

                        if (availableReminderTriggers.Contains(methodInfo))
                            reminderBehaviorDropdown.SetValueWithoutNotify(methodInfo);
                        else if (availableReminderTriggers.Count > 0)
                            reminderBehaviorDropdown.SetValueWithoutNotify(availableReminderTriggers[0]);

                        reminderBehaviorDropdown.SetEnabled(true);
                    }
                }
            }
        }


        // -- Success Trigger Logic --

        private void OnSuccessObjectSelected(ChangeEvent<UnityEngine.Object> evt)
        {
            GameObject selectedObject = evt.newValue as GameObject;
            if (selectedObject == null)
            {
                successTriggerDropdown.SetEnabled(false);
                return;
            }

            availableSuccessTriggers = TriggerAssignmentController.GetAvailableEvents(selectedObject);

            if (availableSuccessTriggers.Count > 0)
            {
                successTriggerDropdown.choices = availableSuccessTriggers;
                successTriggerDropdown.index = 0;
                successTriggerDropdown.SetEnabled(true);

                SetSuccessTriggerInModel(selectedObject, availableSuccessTriggers[0]);
            }
            else
            {
                successTriggerDropdown.choices = new List<string>();
                successTriggerDropdown.SetEnabled(false);
            }
        }

        private void OnSuccessTriggerSelected(ChangeEvent<string> evt)
        {
            if (successObjectField.value == null || string.IsNullOrEmpty(evt.newValue))
                return;

            SetSuccessTriggerInModel(successObjectField.value as GameObject, evt.newValue);
        }

        private void SetSuccessTriggerInModel(GameObject selectedObject, string selectedTriggerFullName)
        {
            string[] parts = selectedTriggerFullName.Split('.');
            if (parts.Length != 2) return;

            string scriptName = parts[0];
            string methodName = parts[1];

            var typedModel = (ReminderNodeModel)_model;

            // Always update OutgoingPaths[0] = Success Path
            typedModel.SetSuccessPath(selectedObject.name, scriptName, methodName, typedModel.OutgoingPaths[0].TargetNodeID);
        }

        // -- Reminder Behavior Logic --

        private void OnReminderObjectSelected(ChangeEvent<UnityEngine.Object> evt)
        {
            GameObject selectedObject = evt.newValue as GameObject;
            if (selectedObject == null)
            {
                reminderBehaviorDropdown.SetEnabled(false);
                return;
            }

            availableReminderTriggers = TriggerAssignmentController.GetAvailableEvents(selectedObject);

            if (availableReminderTriggers.Count > 0)
            {
                reminderBehaviorDropdown.choices = availableReminderTriggers;
                reminderBehaviorDropdown.index = 0;
                reminderBehaviorDropdown.SetEnabled(true);

                SetReminderBehaviorInModel(selectedObject, availableReminderTriggers[0]);
            }
            else
            {
                reminderBehaviorDropdown.choices = new List<string>();
                reminderBehaviorDropdown.SetEnabled(false);
            }
        }

        private void OnReminderBehaviorSelected(ChangeEvent<string> evt)
        {
            if (reminderObjectField.value == null || string.IsNullOrEmpty(evt.newValue))
                return;

            SetReminderBehaviorInModel(reminderObjectField.value as GameObject, evt.newValue);
        }

        private void SetReminderBehaviorInModel(GameObject selectedObject, string selectedTriggerFullName)
        {
            string[] parts = selectedTriggerFullName.Split('.');
            if (parts.Length != 2) return;

            string scriptName = parts[0];
            string methodName = parts[1];

            var typedModel = (ReminderNodeModel)_model;

            // Update the reminder path visually
            typedModel.SetReminderPath(selectedObject.name, scriptName, methodName);

            // Also update the structured instruction in the model via the controller
            _controller.SetReminderInstruction(
                typedModel.NodeID,
                selectedObject.name,
                $"{scriptName}.{methodName}"
            );
        }
    }
}
