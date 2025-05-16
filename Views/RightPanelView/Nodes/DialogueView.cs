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

            var dialogueTextField = new TextField
            {
                value = typedModel.DialogueText,
                multiline = true
            };
            dialogueTextField.RegisterValueChangedCallback(evt =>
            {
                typedModel.DialogueText = evt.newValue;
                _controller.MarkDirty();
            });

            dialogueTextField.style.whiteSpace = WhiteSpace.Normal;
            dialogueTextField.style.flexGrow = 0;
            dialogueTextField.style.height = StyleKeyword.Auto;
            dialogueTextField.style.unityTextAlign = TextAnchor.UpperLeft;
            dialogueTextField.style.overflow = Overflow.Visible;
            dialogueTextField.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                dialogueTextField.style.height = StyleKeyword.Auto;
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

                var responseField = new TextField
                {
                    value = path.Label,
                    multiline = true
                };
                responseField.RegisterValueChangedCallback(evt =>
                {
                    typedModel.UpdateResponse(index, evt.newValue);
                    _controller.MarkDirty();
                });

                responseField.style.whiteSpace = WhiteSpace.Normal;
                responseField.style.flexGrow = 0;
                responseField.style.height = StyleKeyword.Auto;
                responseField.style.unityTextAlign = TextAnchor.UpperLeft;
                responseField.style.overflow = Overflow.Visible;
                responseField.RegisterCallback<GeometryChangedEvent>(_ =>
                {
                    responseField.style.height = StyleKeyword.Auto;
                });

                row.Add(responseField);

                if (string.IsNullOrEmpty(path.TargetNodeID))
                {
                    var connectButton = new Button(() =>
                    {
                        _controller.StartConnectionFrom(typedModel.NodeID, targetId =>
                        {
                            typedModel.ConnectResponseTo(index, targetId);
                            _controller.GraphView.RenderConnections();
                            _controller.CancelConnection();
                            _controller.OnNodeSelected(typedModel);
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
                    var connectedLabel = new Label($"→ {_controller.GetNode(path.TargetNodeID)?.Title ?? "(Unknown)"}");
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

                            SetTriggerFromCombinedString(typedModel, index, selectedObj.name, availableMethods[0]);
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
                        SetTriggerFromCombinedString(typedModel, index, selectedObj.name, evt.newValue);
                    }
                });

                Add(row);
            }

            var addResponseBtn = new Button(() =>
            {
                typedModel.AddResponse("New Response");
                _controller.OnNodeSelected(typedModel);
            })
            {
                text = "Add Response",
                style = { marginTop = 10 }
            };
            Add(addResponseBtn);
        }

        private void SetTriggerFromCombinedString(DialogueNodeModel model, int index, string objectName, string combined)
        {
            var parts = combined.Split('.');
            if (parts.Length != 2) return;

            var scriptName = parts[0];
            var methodName = parts[1];
            var triggerString = $"{objectName}:{scriptName}.{methodName}";
            model.OutgoingPaths[index].Trigger = triggerString;

            _controller.MarkDirty();
        }
    }
}
