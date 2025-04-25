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
        private string selectedEventType = "";
        private List<string> availableEvents = new();

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

                var row = new VisualElement
                {
                    style = { marginTop = 6, flexDirection = FlexDirection.Column }
                };

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
                    AddTriggerAssignmentSection(row, controller, typedModel, index, path);
                }

                Add(row);
            }

            var addBtn = new Button(() =>
            {
                typedModel.AddChoice("New Choice");
                controller.OnNodeSelected(typedModel); // Refresh
            })
            {
                text = "Add Choice",
                style = { marginTop = 6 }
            };
            Add(addBtn);
        }

        private void AddTriggerAssignmentSection(VisualElement container, GraphController controller, MultipleChoiceNodeModel typedModel, int index, PathData path)
        {
            string currentTrigger = path.Trigger ?? "";
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
            container.Add(selectionLabel);

            var eventDropdown = new PopupField<string>("Event", new List<string>(), 0);
            eventDropdown.style.display = DisplayStyle.None;
            container.Add(eventDropdown);

            var saveBtn = new Button()
            {
                text = "Save Trigger",
                style = { marginTop = 4 }
            };
            saveBtn.SetEnabled(false);
            container.Add(saveBtn);

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

            var selectBtn = new Button(() =>
            {
                TriggerAssignmentController.Start(
                    trigger =>
                    {
                        controller.SetTriggerForMultipleChoicePath(typedModel, index, trigger);
                        controller.GraphView.RefreshNodeVisual(typedModel.NodeID);
                        ApplyInstructionsToMultipleChoiceNode(typedModel);
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
            container.Add(selectBtn);

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
            container.Add(refreshBtn);

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

                    ApplyInstructionsToMultipleChoiceNode(typedModel);
                }
            };

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

        private void ApplyInstructionsToMultipleChoiceNode(MultipleChoiceNodeModel node)
        {
            if (node == null) return;

            node.Instructions ??= new List<string>();
            node.Instructions.Clear();

            InstructionHelpers.AddAwaitTriggerInstruction(node);
        }
    }
}
