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

            var textLabel = new Label("Reminder Text");
            textLabel.style.whiteSpace = WhiteSpace.Normal;
            Add(textLabel);

            var reminderTextField = new TextField { value = typedModel.ReminderText, multiline = true };
            reminderTextField.RegisterValueChangedCallback(evt =>
            {
                controller.UpdateReminderText(typedModel, evt.newValue);
            });
            Add(reminderTextField);

            var timerLabel = new Label("Reminder Timer (seconds)");
            timerLabel.style.whiteSpace = WhiteSpace.Normal;
            Add(timerLabel);

            var timerField = new FloatField { value = typedModel.ReminderTimer };
            timerField.RegisterValueChangedCallback(evt =>
            {
                controller.UpdateReminderTimer(typedModel, evt.newValue);
            });
            Add(timerField);

            // Outgoing connection section
            if (typedModel.OutgoingPaths.Count == 0)
            {
                var connectButton = new Button(() =>
                {
                    controller.StartConnectionFrom(typedModel.NodeID, targetNodeId =>
                    {
                        typedModel.ConnectTo(targetNodeId, "Next");
                        controller.GraphView.RenderConnections();
                    });
                })
                {
                    text = "Connect"
                };
                connectButton.style.marginTop = 15;
                Add(connectButton);
            }
            else
            {
                Add(new Label("Outgoing Connection")
                {
                    style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 }
                });

                var connection = typedModel.OutgoingPaths[0];
                var connectedModel = controller.GetNode(connection.TargetNodeID);
                var label = new Label($"→ {connectedModel?.Title ?? "(Unknown)"}");
                label.style.whiteSpace = WhiteSpace.Normal;
                Add(label);

                // Success Action Section (for normal user interaction)
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

                // Reminder Behavior Section (for what happens if timer expires)
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

            string methodName = parts[1];

            var typedModel = (ReminderNodeModel)_model;

            if (typedModel.OutgoingPaths.Count == 0)
                typedModel.OutgoingPaths.Add(new PathData("Next"));

            typedModel.OutgoingPaths[0].Trigger = $"{selectedObject.name}:{methodName}";
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

            string methodName = parts[1];

            var typedModel = (ReminderNodeModel)_model;

            typedModel.ReminderBehavior = $"{selectedObject.name}:{methodName}";
        }
    }
}
