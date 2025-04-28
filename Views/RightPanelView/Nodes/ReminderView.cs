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
        private ObjectField objectField;
        private PopupField<string> triggerDropdown;
        private List<string> availableTriggers = new();
        private string selectedEventType = "";

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

            Add(new Label("Reminder Trigger Assignment")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 }
            });

            // Object Picker
            objectField = new ObjectField("Target Object");
            objectField.objectType = typeof(GameObject);
            objectField.value = null;
            objectField.RegisterValueChangedCallback(OnObjectSelected);
            Add(objectField);

            // Trigger Dropdown
            triggerDropdown = new PopupField<string>("Trigger Method", availableTriggers, 0);
            triggerDropdown.SetEnabled(false);
            triggerDropdown.RegisterValueChangedCallback(OnTriggerSelected);
            Add(triggerDropdown);

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

                // 🚀 Now show trigger assignment ONLY after connection exists
                Add(new Label("Reminder Trigger Assignment")
                {
                    style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 }
                });

                // Object Picker
                objectField = new ObjectField("Target Object");
                objectField.objectType = typeof(GameObject);
                objectField.value = null;
                objectField.RegisterValueChangedCallback(OnObjectSelected);
                Add(objectField);

                // Trigger Dropdown
                triggerDropdown = new PopupField<string>("Trigger Method", availableTriggers, 0);
                triggerDropdown.SetEnabled(false);
                triggerDropdown.RegisterValueChangedCallback(OnTriggerSelected);
                Add(triggerDropdown);
            }

        }

        private void OnObjectSelected(ChangeEvent<UnityEngine.Object> evt)
        {
            GameObject selectedObject = evt.newValue as GameObject;
            if (selectedObject == null)
            {
                triggerDropdown.SetEnabled(false);
                return;
            }

            availableTriggers = TriggerAssignmentController.GetAvailableEvents(selectedObject);

            if (availableTriggers.Count > 0)
            {
                triggerDropdown.choices = availableTriggers;
                triggerDropdown.index = 0;
                triggerDropdown.SetEnabled(true);

                SetReminderTriggerInModel(selectedObject, availableTriggers[0]);
            }
            else
            {
                triggerDropdown.choices = new List<string>();
                triggerDropdown.SetEnabled(false);
            }
        }

        private void OnTriggerSelected(ChangeEvent<string> evt)
        {
            if (objectField.value == null || string.IsNullOrEmpty(evt.newValue))
                return;

            SetReminderTriggerInModel(objectField.value as GameObject, evt.newValue);
        }

        private void SetReminderTriggerInModel(GameObject selectedObject, string selectedTriggerFullName)
        {
            string[] parts = selectedTriggerFullName.Split('.');
            if (parts.Length != 2) return;

            string methodName = parts[1];

            var typedModel = (ReminderNodeModel)_model;

            if (typedModel.OutgoingPaths.Count == 0)
                typedModel.OutgoingPaths.Add(new PathData("Next"));

            // Set trigger format as "ObjectName:MethodName" into OutgoingPaths[0].Trigger
            typedModel.OutgoingPaths[0].Trigger = $"{selectedObject.name}:{methodName}";
        }

    }
}
