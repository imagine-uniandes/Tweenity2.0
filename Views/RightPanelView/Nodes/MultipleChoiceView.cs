using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
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
                controller.MarkDirty();
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

                // --- Choice Text ---
                var choiceField = new TextField { value = path.Label };
                choiceField.RegisterValueChangedCallback(evt =>
                {
                    typedModel.UpdateChoice(index, evt.newValue);
                    controller.MarkDirty();
                });
                row.Add(choiceField);

                // --- Connection Section ---
                if (string.IsNullOrEmpty(path.TargetNodeID))
                {
                    var connectButton = new Button(() =>
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
                        text = "Connect Choice",
                        style = { marginTop = 4 }
                    };
                    row.Add(connectButton);
                }
                else
                {
                    var connectedLabel = new Label($"→ {controller.GetNode(path.TargetNodeID)?.Title ?? "(Unknown)"}");
                    connectedLabel.style.marginTop = 4;
                    row.Add(connectedLabel);
                }

                // --- Target Object + Trigger Method ---
                row.Add(new Label("Target Object") { style = { marginTop = 6 } });

                var objectField = new ObjectField();
                objectField.objectType = typeof(GameObject);
                objectField.style.marginTop = 2;
                row.Add(objectField);

                var methodDropdown = new PopupField<string>("Trigger Method", new List<string>(), 0);
                methodDropdown.style.marginTop = 2;
                methodDropdown.style.display = DisplayStyle.None;
                row.Add(methodDropdown);

                // Load from saved Trigger if exists
                if (!string.IsNullOrEmpty(path.Trigger) && path.Trigger.Contains(":"))
                {
                    var parts = path.Trigger.Split(':');
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

                            UpdateTrigger(typedModel, index, selectedObj.name, methodDropdown.value);
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
                        UpdateTrigger(typedModel, index, selectedObj.name, evt.newValue);
                    }
                });

                Add(row);
            }

            // --- Add New Choice Button ---
            var addChoiceBtn = new Button(() =>
            {
                typedModel.AddChoice("New Choice");
                controller.OnNodeSelected(typedModel);
            })
            {
                text = "Add Choice",
                style = { marginTop = 10 }
            };
            Add(addChoiceBtn);
        }

        private void UpdateTrigger(MultipleChoiceNodeModel model, int choiceIndex, string objectName, string methodName)
        {
            var triggerString = $"{objectName}:{methodName}";
            model.OutgoingPaths[choiceIndex].Trigger = triggerString;
            model.Instructions ??= new List<string>();
            model.Instructions.Clear();

            InstructionHelpers.AddAwaitTriggerInstruction(model);
        }
    }
}
