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
                _controller.MarkDirty();
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
                    _controller.StartConnectionFrom(typedModel.NodeID, targetId =>
                    {
                        typedModel.SetTimeoutPath("Timeout", targetId);
                        _controller.GraphView.RenderConnections();
                        _controller.CancelConnection();
                        _controller.OnNodeSelected(typedModel);
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
                var connectedModel = _controller.GetNode(typedModel.OutgoingPaths[0].TargetNodeID);
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
                    _controller.StartConnectionFrom(typedModel.NodeID, targetId =>
                    {
                        typedModel.SetSuccessPath("Success", targetId);
                        _controller.GraphView.RenderConnections();
                        _controller.CancelConnection();
                        _controller.OnNodeSelected(typedModel);
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
                var connectedModel = _controller.GetNode(typedModel.OutgoingPaths[1].TargetNodeID);
                var connectedLabel = new Label($"Success → {connectedModel?.Title ?? "(Unknown)"}")
                {
                    style = { marginTop = 4 }
                };
                Add(connectedLabel);
            }

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
                        var methodFull = parts[1];

                        var obj = GameObject.Find(objName);
                        if (obj != null)
                        {
                            objectField.SetValueWithoutNotify(obj);

                            var availableMethods = TriggerAssignmentController.GetAvailableEvents(obj);
                            methodDropdown.choices = availableMethods;

                            methodDropdown.SetValueWithoutNotify(
                                availableMethods.Contains(methodFull) ? methodFull : availableMethods[0]
                            );
                            methodDropdown.style.display = DisplayStyle.Flex;
                        }
                        else
                        {
                            objectField.label = "Objeto no disponible en escena";
                            objectField.value = null;

                            methodDropdown.choices = new List<string> { methodFull };
                            methodDropdown.SetValueWithoutNotify(methodFull);
                            methodDropdown.style.display = DisplayStyle.Flex;
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

                            SetTriggerFromCombinedString(typedModel, selectedObj.name, availableMethods[0]);
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
                        SetTriggerFromCombinedString(typedModel, selectedObj.name, evt.newValue);
                    }
                });
            }

            UpdateWaitInstruction(typedModel);
        }

        private void SetTriggerFromCombinedString(TimeoutNodeModel model, string objectName, string combined)
        {
            var parts = combined.Split('.');
            if (parts.Length != 2) return;

            var scriptName = parts[0];
            var methodName = parts[1];
            var triggerString = $"{objectName}:{scriptName}.{methodName}";
            model.OutgoingPaths[1].Trigger = triggerString;

            _controller.MarkDirty();
        }

        private void UpdateWaitInstruction(TimeoutNodeModel model)
        {
        }
    }
}