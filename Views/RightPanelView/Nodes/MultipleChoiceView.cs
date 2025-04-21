using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.Collections.Generic;
using Models.Nodes;
using Controllers;

namespace Views.RightPanel
{
    public class MultipleChoiceView : TweenityNodeView
    {
        public MultipleChoiceView(MultipleChoiceNodeModel model, GraphController controller) : base(model, controller)
        {
            var typedModel = (MultipleChoiceNodeModel)_model;

            Add(new Label("Multiple Choice Node Details")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, whiteSpace = WhiteSpace.Normal }
            });

            Add(new Label("Question") { style = { whiteSpace = WhiteSpace.Normal } });

            var questionField = new TextField { value = typedModel.Question };
            questionField.RegisterValueChangedCallback(evt =>
            {
                typedModel.Question = evt.newValue;
                controller.GraphView.RefreshNodeVisual(typedModel.NodeID);
            });
            Add(questionField);

            Add(new Label("Choices")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 }
            });

            for (int i = 0; i < typedModel.OutgoingPaths.Count; i++)
            {
                int index = i;
                var path = typedModel.OutgoingPaths[index];

                var row = new VisualElement();
                row.style.marginTop = 6;
                row.style.flexDirection = FlexDirection.Column;

                var choiceField = new TextField { value = path.Label };
                choiceField.RegisterValueChangedCallback(evt =>
                {
                    typedModel.UpdateChoice(index, evt.newValue);
                    controller.GraphView.RefreshNodeVisual(typedModel.NodeID);
                });
                row.Add(choiceField);

                var connectedModel = controller.GetNode(path.TargetNodeID);
                if (string.IsNullOrEmpty(path.TargetNodeID))
                {
                    var connectBtn = new Button(() =>
                    {
                        controller.StartConnectionFrom(typedModel.NodeID, targetId =>
                        {
                            typedModel.ConnectChoiceTo(index, targetId);
                            controller.GraphView.RenderConnections();
                            controller.CancelConnection();
                            controller.OnNodeSelected(typedModel);
                        });
                    })
                    {
                        text = "Connect",
                        style = { marginTop = 2 }
                    };
                    row.Add(connectBtn);
                }
                else
                {
                    var connectedBtn = new Button
                    {
                        text = $"→ {connectedModel?.Title ?? "(Unknown)"}"
                    };
                    connectedBtn.SetEnabled(false);
                    row.Add(connectedBtn);

                    // Trigger assignment section
                    string currentTrigger = path.Trigger ?? "";
                    string selectedEventType = "";
                    List<string> availableEvents = new();

                    string preObj = "", preEvent = "";
                    if (!string.IsNullOrEmpty(currentTrigger) && currentTrigger.Contains(":"))
                    {
                        var parts = currentTrigger.Split(':');
                        if (parts.Length == 2)
                        {
                            preObj = parts[0];
                            preEvent = parts[1];
                        }
                    }

                    var selectionLabel = new Label("Current selection: (none)")
                    {
                        style = {
                            marginTop = 2,
                            whiteSpace = WhiteSpace.Normal,
                            flexWrap = Wrap.Wrap,
                            unityTextAlign = TextAnchor.UpperLeft
                        }
                    };
                    row.Add(selectionLabel);

                    var eventDropdown = new PopupField<string>("Event", new List<string>(), 0);
                    eventDropdown.style.display = DisplayStyle.None;
                    row.Add(eventDropdown);

                    var saveBtn = new Button { text = "Save Trigger" };
                    saveBtn.SetEnabled(false);
                    row.Add(saveBtn);

                    void UpdateSaveButtonState()
                    {
                        if (TriggerAssignmentController.SelectedObject != null && !string.IsNullOrEmpty(selectedEventType))
                        {
                            string newTrigger = $"{TriggerAssignmentController.SelectedObject.name}:{selectedEventType}";
                            saveBtn.SetEnabled(newTrigger != currentTrigger);
                        }
                        else saveBtn.SetEnabled(false);
                    }

                    var startBtn = new Button(() =>
                    {
                        TriggerAssignmentController.Start(
                            trigger =>
                            {
                                controller.SetTriggerForMultipleChoicePath(typedModel, index, trigger);
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
                                    else eventDropdown.style.display = DisplayStyle.None;
                                    UpdateSaveButtonState();
                                }
                            }
                        );
                    })
                    {
                        text = "Select Object for Trigger",
                        style = { marginTop = 2 }
                    };
                    row.Add(startBtn);

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
                            controller.SetTriggerForMultipleChoicePath(typedModel, index, finalTrigger);
                            controller.GraphView.RefreshNodeVisual(typedModel.NodeID);
                            currentTrigger = finalTrigger;
                            UpdateSaveButtonState();

                            // NEW: Add AwaitTrigger instruction
                            ApplyInstructionsToMultipleChoiceNode(typedModel);
                        }
                    };

                    // Preload trigger
                    if (!string.IsNullOrEmpty(preObj))
                    {
                        var preGO = GameObject.Find(preObj);
                        if (preGO != null)
                        {
                            TriggerAssignmentController.SetSelectedObjectManually(preGO);
                            selectionLabel.text = $"Current selection: {preGO.name}";
                            availableEvents = TriggerAssignmentController.GetAvailableEvents(preGO);
                            if (availableEvents.Count > 0)
                            {
                                eventDropdown.choices = availableEvents;
                                selectedEventType = availableEvents.Contains(preEvent) ? preEvent : availableEvents[0];
                                eventDropdown.SetValueWithoutNotify(selectedEventType);
                                eventDropdown.style.display = DisplayStyle.Flex;
                            }
                        }
                    }
                }

                Add(row);
            }

            var addBtn = new Button(() =>
            {
                typedModel.AddChoice("New Choice");
                controller.OnNodeSelected(typedModel); // Refresh view
            })
            {
                text = "Add Choice",
                style = { marginTop = 6 }
            };
            Add(addBtn);
        }
        private void ApplyInstructionsToMultipleChoiceNode(MultipleChoiceNodeModel node)
        {
            if (node == null) return;

            node.Instructions ??= new List<string>();
            node.Instructions.Clear();

            InstructionHelpers.AddAwaitTriggerInstruction(node);
        }

    }
}
