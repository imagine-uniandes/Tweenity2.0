using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using Models.Nodes;
using Controllers;
using Models;

namespace Views.RightPanel
{
    public class DialogueView : TweenityNodeView
    {
        public DialogueView(DialogueNodeModel model, GraphController controller) : base(model, controller)
        {
            var typedModel = (DialogueNodeModel)_model;

            Add(new Label("Details")
            {
                style = {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    whiteSpace = WhiteSpace.Normal,
                    marginBottom = 10,
                    marginTop = 10
                }
            });

            Add(new Label("Dialogue Text") { style = { whiteSpace = WhiteSpace.Normal } });

            var dialogueTextField = new TextField { value = typedModel.DialogueText };
            dialogueTextField.RegisterValueChangedCallback(evt =>
            {
                typedModel.DialogueText = evt.newValue;
                controller.MarkDirty();
            });
            Add(dialogueTextField);

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
                    controller.MarkDirty();
                });
                row.Add(responseField);

                if (string.IsNullOrEmpty(path.TargetNodeID))
                {
                    var connectButton = new Button(() =>
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
                        text = "Connect Response",
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

                row.Add(new Label("Target Object") { style = { marginTop = 6 } });

                var objectField = new ObjectField();
                objectField.objectType = typeof(GameObject);
                objectField.style.marginTop = 2;
                row.Add(objectField);

                var methodDropdown = new PopupField<string>("Trigger Method", new List<string>(), 0);
                methodDropdown.style.marginTop = 2;
                methodDropdown.style.display = DisplayStyle.None;
                row.Add(methodDropdown);

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

            var addResponseBtn = new Button(() =>
            {
                typedModel.AddResponse("New Response");
                controller.OnNodeSelected(typedModel);
            })
            {
                text = "Add Response",
                style = { marginTop = 10 }
            };
            Add(addResponseBtn);
        }

        private void UpdateTrigger(DialogueNodeModel model, int responseIndex, string objectName, string methodName)
        {
            var triggerString = $"{objectName}:{methodName}";
            model.OutgoingPaths[responseIndex].Trigger = triggerString;
        }
    }
}
