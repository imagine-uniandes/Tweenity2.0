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

        public ReminderView(ReminderNodeModel model, GraphController controller) : base(model, controller)
        {
            Add(new Label("Details")
            {
                style = {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    whiteSpace = WhiteSpace.Normal,
                    marginBottom = 10,
                    marginTop = 10
                }
            });

            var typedModel = (ReminderNodeModel)_model;

            // ----- Connection UI -----
            if (typedModel.OutgoingPaths.Count == 0 || string.IsNullOrEmpty(typedModel.OutgoingPaths[0].TargetNodeID))
            {
                var connectButton = new Button(() =>
                {
                    controller.StartConnectionFrom(typedModel.NodeID, (targetNodeId) =>
                    {
                        if (typedModel.OutgoingPaths.Count == 0)
                            typedModel.OutgoingPaths.Add(new PathData("Success", "", targetNodeId));
                        else
                            typedModel.OutgoingPaths[0].TargetNodeID = targetNodeId;

                        controller.GraphView.RenderConnections();
                    });
                })
                {
                    text = "Connect"
                };
                connectButton.style.marginTop = 10;
                Add(connectButton);
            }
            else
            {
                Add(new Label("Outgoing Connection")
                {
                    style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 }
                });

                var path = typedModel.OutgoingPaths[0];
                var connectedModel = controller.GetNode(path.TargetNodeID);
                var connectionLabel = new Label($"→ {connectedModel?.Title ?? "(Unknown)"}")
                {
                    style = { whiteSpace = WhiteSpace.Normal }
                };
                Add(connectionLabel);
            }

            // ----- Trigger Assignment Section -----
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

            // Preload Success Trigger
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
        }

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

            if (typedModel.OutgoingPaths.Count == 0)
                typedModel.OutgoingPaths.Add(new PathData("Success", "", ""));

            typedModel.SetSuccessPath(selectedObject.name, scriptName, methodName, typedModel.OutgoingPaths[0].TargetNodeID);
        }
    }
}
