using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using Models.Nodes;
using Controllers;

namespace Views.RightPanel
{
    public class TimeoutView : TweenityNodeView
    {
        private string selectedEventType = "";
        private List<string> availableEvents = new();

        public TimeoutView(TimeoutNodeModel model, GraphController controller) : base(model, controller)
        {
            Add(new Label("Timeout Node Details")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, whiteSpace = WhiteSpace.Normal }
            });

            var typedModel = (TimeoutNodeModel)_model;

            Add(new Label("Timeout Condition") { style = { whiteSpace = WhiteSpace.Normal } });

            var conditionField = new TextField { value = typedModel.Condition };
            conditionField.RegisterValueChangedCallback(evt =>
            {
                typedModel.Condition = evt.newValue;
                controller.GraphView.RefreshNodeVisual(typedModel.NodeID);
            });
            Add(conditionField);

            Add(new Label("Timeout Timer (seconds)") { style = { whiteSpace = WhiteSpace.Normal } });

            var timeoutTimerField = new FloatField { value = typedModel.TimeoutDuration };
            timeoutTimerField.RegisterValueChangedCallback(evt =>
            {
                typedModel.TimeoutDuration = evt.newValue;
                controller.GraphView.RefreshNodeVisual(typedModel.NodeID);
            });
            Add(timeoutTimerField);

            Add(new Label("Outgoing Connections")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 }
            });

            // Timeout path UI (Path 0)
            if (typedModel.OutgoingPaths.Count >= 1 && string.IsNullOrEmpty(typedModel.OutgoingPaths[0].TargetNodeID))
            {
                var timeoutBtn = new Button(() =>
                {
                    controller.StartConnectionFrom(typedModel.NodeID, targetId =>
                    {
                        typedModel.SetTimeoutPath("Timeout", targetId);
                        controller.SetTriggerForTimeoutPath(typedModel, true, $"{typedModel.NodeID}:timeout");
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
                var connectedLabel = new Button
                {
                    text = $"→ {connectedModel?.Title ?? "(Unknown)"}"
                };
                connectedLabel.SetEnabled(false);
                connectedLabel.style.marginTop = 4;
                Add(connectedLabel);
            }

            // Success path UI (Path 1)
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
                var connectedLabel = new Button
                {
                    text = $"→ {connectedModel?.Title ?? "(Unknown)"}"
                };
                connectedLabel.SetEnabled(false);
                connectedLabel.style.marginTop = 4;
                Add(connectedLabel);
            }

            // Trigger assignment for success path
            if (typedModel.OutgoingPaths.Count >= 2 && !string.IsNullOrEmpty(typedModel.OutgoingPaths[1].TargetNodeID))
            {
                Add(new Label("Assign Trigger to Success Path")
                {
                    style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 }
                });

                string currentTrigger = typedModel.OutgoingPaths[1].Trigger ?? "";

                string preselectedObjectName = "";
                string preselectedEvent = "";

                if (!string.IsNullOrEmpty(currentTrigger) && currentTrigger.Contains(":"))
                {
                    var parts = currentTrigger.Split(':');
                    if (parts.Length == 2)
                    {
                        preselectedObjectName = parts[0];
                        preselectedEvent = parts[1];
                    }
                }

                var selectionLabel = new Label("Current selection: (none)")
                {
                    style = {
                        marginTop = 4,
                        whiteSpace = WhiteSpace.Normal,
                        flexWrap = Wrap.Wrap,
                        unityTextAlign = TextAnchor.UpperLeft
                    }
                };
                Add(selectionLabel);

                var eventDropdown = new PopupField<string>("Event", new List<string>(), 0);
                eventDropdown.style.display = DisplayStyle.None;
                Add(eventDropdown);

                var saveBtn = new Button()
                {
                    text = "Save Trigger",
                    style = { marginTop = 4 }
                };
                saveBtn.SetEnabled(false);
                Add(saveBtn);

                void UpdateSaveButtonState()
                {
                    if (TriggerAssignmentController.SelectedObject != null && !string.IsNullOrEmpty(selectedEventType))
                    {
                        string newTrigger = $"{TriggerAssignmentController.SelectedObject.name}:{selectedEventType}";
                        saveBtn.SetEnabled(newTrigger != currentTrigger);
                    }
                    else
                    {
                        saveBtn.SetEnabled(false);
                    }
                }

                var startListeningBtn = new Button(() =>
                {
                    TriggerAssignmentController.Start(
                        trigger =>
                        {
                            controller.SetTriggerForTimeoutPath(typedModel, false, trigger);
                            controller.GraphView.RefreshNodeVisual(typedModel.NodeID);
                        },
                        onObjectSelectedImmediate: () =>
                        {
                            var go = TriggerAssignmentController.SelectedObject;
                            if (go != null)
                            {
                                selectionLabel.text = $"Current selection: {go.name}";

                                availableEvents = TriggerAssignmentController.GetAvailableEvents(go);
                                if (availableEvents.Count > 0)
                                {
                                    eventDropdown.choices = availableEvents;
                                    selectedEventType = availableEvents[0];
                                    eventDropdown.SetValueWithoutNotify(selectedEventType);
                                    eventDropdown.style.display = DisplayStyle.Flex;
                                }
                                else
                                {
                                    eventDropdown.style.display = DisplayStyle.None;
                                }

                                UpdateSaveButtonState();
                            }
                        }
                    );
                })
                {
                    text = "Select Object for Trigger",
                    style = { marginTop = 4 }
                };
                Add(startListeningBtn);

                var refreshBtn = new Button(() =>
                {
                    TriggerAssignmentController.ConfirmObjectSelection();

                    if (TriggerAssignmentController.SelectedObject != null)
                    {
                        var go = TriggerAssignmentController.SelectedObject;
                        selectionLabel.text = $"Current selection: {go.name}";

                        availableEvents = TriggerAssignmentController.GetAvailableEvents(go);
                        if (availableEvents.Count > 0)
                        {
                            eventDropdown.choices = availableEvents;
                            selectedEventType = availableEvents.Contains(preselectedEvent) ? preselectedEvent : availableEvents[0];
                            eventDropdown.SetValueWithoutNotify(selectedEventType);
                            eventDropdown.style.display = DisplayStyle.Flex;
                        }
                        else
                        {
                            eventDropdown.style.display = DisplayStyle.None;
                        }

                        UpdateSaveButtonState();
                    }
                })
                {
                    text = "Load Events From Selection"
                };
                Add(refreshBtn);

                eventDropdown.RegisterValueChangedCallback(evt =>
                {
                    selectedEventType = evt.newValue;
                    UpdateSaveButtonState();
                });

                saveBtn.clicked += () =>
                {
                    if (TriggerAssignmentController.SelectedObject != null && !string.IsNullOrEmpty(selectedEventType))
                    {
                        string finalTrigger = $"{TriggerAssignmentController.SelectedObject.name}:{selectedEventType}";
                        controller.SetTriggerForTimeoutPath(typedModel, false, finalTrigger);
                        controller.GraphView.RefreshNodeVisual(typedModel.NodeID);
                        currentTrigger = finalTrigger;
                        UpdateSaveButtonState();
                    }
                };

                // Auto-load if saved trigger exists
                if (!string.IsNullOrEmpty(preselectedObjectName))
                {
                    var preselectedGO = GameObject.Find(preselectedObjectName);
                    if (preselectedGO != null)
                    {
                        TriggerAssignmentController.SetSelectedObjectManually(preselectedGO);
                        selectionLabel.text = $"Current selection: {preselectedGO.name}";

                        availableEvents = TriggerAssignmentController.GetAvailableEvents(preselectedGO);
                        if (availableEvents.Count > 0)
                        {
                            eventDropdown.choices = availableEvents;
                            selectedEventType = availableEvents.Contains(preselectedEvent) ? preselectedEvent : availableEvents[0];
                            eventDropdown.SetValueWithoutNotify(selectedEventType);
                            eventDropdown.style.display = DisplayStyle.Flex;
                        }
                    }
                }
            }
        }
    }
}
