using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using Models.Nodes;
using Controllers;
using Models;

namespace Views.RightPanel
{
    public class TimeoutView : TweenityNodeView
    {
        public TimeoutView(TimeoutNodeModel model, GraphController controller) : base(model, controller)
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

            var typedModel = (TimeoutNodeModel)_model;

            Add(new Label("Timeout Timer (seconds)") { style = { whiteSpace = WhiteSpace.Normal } });

            var timeoutTimerField = new FloatField { value = typedModel.TimeoutDuration };
            timeoutTimerField.RegisterValueChangedCallback(evt =>
            {
                typedModel.TimeoutDuration = evt.newValue;
                UpdateWaitInstruction(typedModel);
                controller.MarkDirty();
            });
            Add(timeoutTimerField);

            Add(new Label("Connections")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 }
            });

            if (typedModel.OutgoingPaths.Count >= 1 && string.IsNullOrEmpty(typedModel.OutgoingPaths[0].TargetNodeID))
            {
                var timeoutBtn = new Button(() =>
                {
                    controller.StartConnectionFrom(typedModel.NodeID, targetId =>
                    {
                        typedModel.SetTimeoutPath("Timeout", targetId);
                        controller.GraphView.RenderConnections();
                        controller.CancelConnection();
                        controller.OnNodeSelected(typedModel);
                    });
                })
                {
                    text = "Connect (On Timeout)",
                    style = { marginTop = 4 }
                };
                Add(timeoutBtn);
            }
            else if (typedModel.OutgoingPaths.Count >= 1)
            {
                var connectedModel = controller.GetNode(typedModel.OutgoingPaths[0].TargetNodeID);
                var connectedLabel = new Label($"Timeout → {connectedModel?.Title ?? "(Unknown)"}")
                {
                    style = { marginTop = 4 }
                };
                Add(connectedLabel);
            }

            if (typedModel.OutgoingPaths.Count < 2 || string.IsNullOrEmpty(typedModel.OutgoingPaths[1].TargetNodeID))
            {
                var successBtn = new Button(() =>
                {
                    controller.StartConnectionFrom(typedModel.NodeID, targetId =>
                    {
                        typedModel.SetSuccessPath("Success", targetId);
                        controller.GraphView.RenderConnections();
                        controller.CancelConnection();
                        controller.OnNodeSelected(typedModel);
                    });
                })
                {
                    text = "Connect (On Success)",
                    style = { marginTop = 4 }
                };
                Add(successBtn);
            }
            else if (typedModel.OutgoingPaths.Count >= 2)
            {
                var connectedModel = controller.GetNode(typedModel.OutgoingPaths[1].TargetNodeID);
                var connectedLabel = new Label($"Success → {connectedModel?.Title ?? "(Unknown)"}")
                {
                    style = { marginTop = 4 }
                };
                Add(connectedLabel);
            }

            // --- Trigger Assignment for Success Path ---
            if (typedModel.OutgoingPaths.Count >= 2 && !string.IsNullOrEmpty(typedModel.OutgoingPaths[1].TargetNodeID))
            {
                Add(new Label("Success Trigger Assignment")
                {
                    style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 }
                });

                var objectField = new ObjectField();
                objectField.objectType = typeof(GameObject);
                Add(objectField);

                var methodDropdown = new PopupField<string>("Trigger Method", new List<string>(), 0);
                methodDropdown.style.marginTop = 2;
                methodDropdown.style.display = DisplayStyle.None;
                Add(methodDropdown);

                if (!string.IsNullOrEmpty(typedModel.OutgoingPaths[1].Trigger) && typedModel.OutgoingPaths[1].Trigger.Contains(":"))
                {
                    var parts = typedModel.OutgoingPaths[1].Trigger.Split(':');
                    if (parts.Length == 2)
                    {
                        var objName = parts[0];
                        var methodName = parts[1];

                        var obj = GameObject.Find(objName);
                        if (obj != null)
                        {
                            objectField.SetValueWithoutNotify(obj);

                            var availableMethods = TriggerAssignmentController.GetAvailableEvents(obj);
                            if (availableMethods.Count > 0)
                            {
                                methodDropdown.choices = availableMethods;
                                methodDropdown.SetValueWithoutNotify(availableMethods.Contains(methodName) ? methodName : availableMethods[0]);
                                methodDropdown.style.display = DisplayStyle.Flex;
                            }
                        }
                    }
                }

                objectField.RegisterValueChangedCallback(evt =>
                {
                    var selectedObj = evt.newValue as GameObject;
                    if (selectedObj != null)
                    {
                        var availableMethods = TriggerAssignmentController.GetAvailableEvents(selectedObj);
                        if (availableMethods.Count > 0)
                        {
                            methodDropdown.choices = availableMethods;
                            methodDropdown.index = 0;
                            methodDropdown.style.display = DisplayStyle.Flex;

                            UpdateTrigger(typedModel, selectedObj.name, methodDropdown.value, controller);
                        }
                        else
                        {
                            methodDropdown.style.display = DisplayStyle.None;
                        }
                    }
                    else
                    {
                        methodDropdown.style.display = DisplayStyle.None;
                    }
                });

                methodDropdown.RegisterValueChangedCallback(evt =>
                {
                    var selectedObj = objectField.value as GameObject;
                    if (selectedObj != null && !string.IsNullOrEmpty(evt.newValue))
                    {
                        UpdateTrigger(typedModel, selectedObj.name, evt.newValue, controller);
                    }
                });
            }

            UpdateWaitInstruction(typedModel);
        }

        private void UpdateTrigger(TimeoutNodeModel model, string objectName, string methodName, GraphController controller)
        {
            var triggerString = $"{objectName}:{methodName}";
            model.OutgoingPaths[1].Trigger = triggerString;

            controller.MarkDirty(); 
        }

        private void UpdateWaitInstruction(TimeoutNodeModel model)
        {
        }
    }
}
