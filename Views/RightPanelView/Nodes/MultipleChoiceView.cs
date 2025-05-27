using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using System.Collections.Generic;
using Models.Nodes;
using Controllers;
using Models;
using System.Linq;

namespace Views.RightPanel
{
    public class MultipleChoiceView : TweenityNodeView
    {
        public MultipleChoiceView(MultipleChoiceNodeModel model, GraphController controller) : base(model, controller)
        {
            var typedModel = (MultipleChoiceNodeModel)_model;

            if (typedModel.OutgoingPaths == null || typedModel.OutgoingPaths.Count == 0)
            {
                Debug.LogWarning($"⚠️ [MultipleChoiceView] OutgoingPaths vacío al construir: {typedModel.Title}. Reintentando más tarde.");
                EditorApplication.delayCall += () =>
                {
                    if (_controller.GetNode(typedModel.NodeID) is MultipleChoiceNodeModel retryModel)
                        _controller.OnNodeSelected(retryModel);
                };
                return;
            }

            Add(new Label("Details")
            {
                style = {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    whiteSpace = WhiteSpace.Normal,
                    marginBottom = 10,
                    marginTop = 10
                }
            });

            Add(new Label("Question") { style = { whiteSpace = WhiteSpace.Normal } });

            var questionField = new TextField
            {
                value = typedModel.Question,
                multiline = true
            };
            questionField.RegisterValueChangedCallback(evt =>
            {
                typedModel.Question = evt.newValue;
                _controller.MarkDirty();
            });

            questionField.style.whiteSpace = WhiteSpace.Normal;
            questionField.style.flexGrow = 0;
            questionField.style.height = StyleKeyword.Auto;
            questionField.style.unityTextAlign = TextAnchor.UpperLeft;
            questionField.style.overflow = Overflow.Visible;
            questionField.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                questionField.style.height = StyleKeyword.Auto;
            });

            Add(questionField);

            Add(new Label("Choices")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 }
            });

            foreach (var path in typedModel.OutgoingPaths.ToList())
            {
                var capturedPath = path;

                var row = new VisualElement
                {
                    style = { marginTop = 6, flexDirection = FlexDirection.Column }
                };

                var choiceField = new TextField
                {
                    value = capturedPath.Label,
                    multiline = true
                };
                choiceField.RegisterValueChangedCallback(evt =>
                {
                    capturedPath.Label = evt.newValue;
                    _controller.MarkDirty();
                });

                choiceField.style.whiteSpace = WhiteSpace.Normal;
                choiceField.style.flexGrow = 0;
                choiceField.style.height = StyleKeyword.Auto;
                choiceField.style.unityTextAlign = TextAnchor.UpperLeft;
                choiceField.style.overflow = Overflow.Visible;
                choiceField.RegisterCallback<GeometryChangedEvent>(_ =>
                {
                    choiceField.style.height = StyleKeyword.Auto;
                });

                row.Add(choiceField);

                if (string.IsNullOrEmpty(capturedPath.TargetNodeID))
                {
                    var connectButton = new Button(() =>
                    {
                        _controller.StartConnectionFrom(typedModel.NodeID, targetId =>
                        {
                            capturedPath.TargetNodeID = targetId;
                            _controller.GraphView.RenderConnections();
                            _controller.CancelConnection();
                            _controller.OnNodeSelected(typedModel);
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
                    var connectedLabel = new Label($"→ {_controller.GetNode(capturedPath.TargetNodeID)?.Title ?? "(Unknown)"}");
                    connectedLabel.style.marginTop = 4;
                    row.Add(connectedLabel);
                }

                row.Add(new Label("Target Object") { style = { marginTop = 6 } });

                var objectField = new ObjectField
                {
                    objectType = typeof(GameObject),
                    style = { marginTop = 2 }
                };
                row.Add(objectField);

                var methodDropdown = new PopupField<string>("Trigger Method", new List<string>(), 0)
                {
                    style = { marginTop = 2, display = DisplayStyle.None }
                };
                row.Add(methodDropdown);

                if (!string.IsNullOrEmpty(capturedPath.Trigger) && capturedPath.Trigger.Contains(":"))
                {
                    var parts = capturedPath.Trigger.Split(':');
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
                                availableMethods.Contains(methodFull) ? methodFull : availableMethods.FirstOrDefault()
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

                            SetTriggerFromCombinedString(capturedPath, selectedObj.name, availableMethods[0]);
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
                        SetTriggerFromCombinedString(capturedPath, selectedObj.name, evt.newValue);
                    }
                });

                Add(row);
            }

            var addChoiceBtn = new Button(() =>
            {
                typedModel.AddChoice("New Choice");
                _controller.OnNodeSelected(typedModel);
            })
            {
                text = "Add Choice",
                style = { marginTop = 10 }
            };
            Add(addChoiceBtn);
        }

        private void SetTriggerFromCombinedString(PathData path, string objectName, string combined)
        {
            var parts = combined.Split('.');
            if (parts.Length != 2) return;

            var scriptName = parts[0];
            var methodName = parts[1];
            var triggerString = $"{objectName}:{scriptName}.{methodName}";
            path.Trigger = triggerString;

            _controller.MarkDirty();
        }
    }
}
