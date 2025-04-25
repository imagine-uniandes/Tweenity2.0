using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.Collections.Generic;
using Models.Nodes;
using Controllers;

namespace Views.RightPanel
{
    public class DialogueView : TweenityNodeView
    {
        private string selectedEventType = "";
        private List<string> availableEvents = new();

        public DialogueView(DialogueNodeModel model, GraphController controller) : base(model, controller)
        {
            var typedModel = (DialogueNodeModel)_model;

            Add(new Label("Dialogue Node Details")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, whiteSpace = WhiteSpace.Normal }
            });

            Add(new Label("Dialogue Text") { style = { whiteSpace = WhiteSpace.Normal } });

            var textField = new TextField { value = typedModel.DialogueText };
            textField.RegisterValueChangedCallback(evt =>
            {
                typedModel.DialogueText = evt.newValue;
                controller.GraphView.RefreshNodeVisual(typedModel.NodeID);
            });
            Add(textField);

            Add(new Label("Responses")
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

                var responseField = new TextField { value = path.Label };
                responseField.RegisterValueChangedCallback(evt =>
                {
                    typedModel.UpdateResponse(index, evt.newValue);
                    controller.GraphView.RefreshNodeVisual(typedModel.NodeID);
                });
                row.Add(responseField);

                var connectedModel = controller.GetNode(path.TargetNodeID);
                if (string.IsNullOrEmpty(path.TargetNodeID))
                {
                    var connectBtn = new Button(() =>
                    {
                        controller.StartConnectionFrom(typedModel.NodeID, targetId =>
                        {
                            typedModel.ConnectResponseTo(index, targetId);
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

                    string currentTrigger = path.Trigger ?? "";
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
                            marginTop = 4,
                            whiteSpace = WhiteSpace.Normal,
                            flexWrap = Wrap.Wrap,
                            unityTextAlign = TextAnchor.UpperLeft
                        }
                    };
                    row.Add(selectionLabel);

                    var eventDropdown = new PopupField<string>("Event", new List<string>(), 0);
                    eventDropdown.style.display = DisplayStyle.None;
                    row.Add(eventDropdown);

                    var saveBtn = new Button { text = "Save Trigger", style = { marginTop = 2 } };
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

                    var selectBtn = new Button(() =>
                    {
                        TriggerAssignmentController.Start(
                            trigger =>
                            {
                                controller.SetTriggerForDialoguePath(typedModel, index, trigger);
                                controller.GraphView.RefreshNodeVisual(typedModel.NodeID);
                                ApplyInstructionsToDialogueNode(typedModel);
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
                        style = { marginTop = 4 }
                    };
                    row.Add(selectBtn);

                    var refreshBtn = new Button(() =>
                    {
                        TriggerAssignmentController.ConfirmObjectSelection();

                        var go = TriggerAssignmentController.SelectedObject;
                        if (go != null)
                        {
                            selectionLabel.text = $"Current selection: {go.name}";
                            availableEvents = TriggerAssignmentController.GetAvailableEvents(go);
                            if (availableEvents.Count > 0)
                            {
                                eventDropdown.choices = availableEvents;
                                selectedEventType = availableEvents.Contains(preEvent) ? preEvent : availableEvents[0];
                                eventDropdown.SetValueWithoutNotify(selectedEventType);
                                eventDropdown.style.display = DisplayStyle.Flex;
                            }
                            else eventDropdown.style.display = DisplayStyle.None;

                            UpdateSaveButtonState();
                        }
                    })
                    {
                        text = "Load Events From Selection"
                    };
                    row.Add(refreshBtn);

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
                            controller.SetTriggerForDialoguePath(typedModel, index, finalTrigger);
                            controller.GraphView.RefreshNodeVisual(typedModel.NodeID);
                            currentTrigger = finalTrigger;
                            UpdateSaveButtonState();

                            ApplyInstructionsToDialogueNode(typedModel);
                        }
                    };

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
                typedModel.AddResponse("New Response");
                controller.OnNodeSelected(typedModel); // Refresh
            })
            {
                text = "Add Response",
                style = { marginTop = 6 }
            };
            Add(addBtn);
        }

        private void ApplyInstructionsToDialogueNode(DialogueNodeModel node)
        {
            if (node == null) return;

            node.Instructions ??= new List<string>();
            node.Instructions.Clear();

            InstructionHelpers.AddAwaitTriggerInstruction(node);
        }
    }
}
