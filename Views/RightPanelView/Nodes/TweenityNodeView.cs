using UnityEngine.UIElements;
using UnityEngine;
using Models;
using Models.Nodes;
using Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;

namespace Views.RightPanel
{
    public class TweenityNodeView : VisualElement
    {
        protected TweenityNodeModel _model;
        protected GraphController _controller;

        public TweenityNodeView(TweenityNodeModel model, GraphController controller)
        {
            _model = model;
            _controller = controller;

            style.paddingLeft = 5;
            style.paddingRight = 5;
            style.paddingTop = 5;
            style.paddingBottom = 5;
            style.borderTopLeftRadius = 5;
            style.borderTopRightRadius = 5;
            style.borderBottomLeftRadius = 5;
            style.borderBottomRightRadius = 5;
            style.flexDirection = FlexDirection.Column;

            Add(new Label("Base Node Info") { style = { unityFontStyleAndWeight = FontStyle.Bold } });

            var titleField = new TextField("Title") { value = _model.Title };
            titleField.RegisterValueChangedCallback(evt => _controller.UpdateNodeTitle(_model, evt.newValue));

            var descriptionField = new TextField("Description")
            {
                value = _model.Description,
                multiline = true
            };
            descriptionField.RegisterValueChangedCallback(evt => _controller.UpdateNodeDescription(_model, evt.newValue));

            descriptionField.style.whiteSpace = WhiteSpace.Normal;
            descriptionField.style.flexGrow = 0;
            descriptionField.style.height = StyleKeyword.Auto;
            descriptionField.style.unityTextAlign = TextAnchor.UpperLeft;
            descriptionField.style.overflow = Overflow.Visible;
            descriptionField.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                descriptionField.style.height = StyleKeyword.Auto;
            });

            var nodeTypes = new List<string>(Enum.GetNames(typeof(NodeType)));
            var typeDropdown = new DropdownField("Node Type", nodeTypes, _model.Type.ToString());
            typeDropdown.RegisterValueChangedCallback(evt =>
            {
                if (Enum.TryParse<NodeType>(evt.newValue, out var newType))
                {
                    var (success, errorMessage) = _controller.ChangeNodeType(_model, newType);
                    if (!success)
                    {
                        typeDropdown.SetValueWithoutNotify(_model.Type.ToString());
    #if UNITY_EDITOR
                        EditorUtility.DisplayDialog("Invalid Node Type", errorMessage, "OK");
    #endif
                    }
                }
            });

            Add(titleField);
            Add(descriptionField);
            Add(typeDropdown);
            Add(new VisualElement { style = { height = 10 } });

            BuildInstructionSection();
        }

        private void BuildInstructionSection()
        {
            Add(new Label("Simulator Actions")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 }
            });

            bool allowRemindCreation = _model.Type == NodeType.Reminder;

            ListView instructionList = null;

            instructionList = new ListView
            {
                itemsSource = _model.Instructions,
                makeItem = () =>
                {
                    var row = new VisualElement
                    {
                        style = {
                            flexDirection = FlexDirection.Row,
                            alignItems = Align.Center,
                            marginBottom = 10,
                            paddingTop = 6,
                            paddingBottom = 6,
                            backgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.2f),
                            borderBottomLeftRadius = 4,
                            borderBottomRightRadius = 4,
                            borderTopLeftRadius = 4,
                            borderTopRightRadius = 4
                        }
                    };

                    var gripIcon = new Image
                    {
                        image = EditorGUIUtility.IconContent("d_UnityEditor.SceneHierarchyWindow").image,
                        scaleMode = ScaleMode.ScaleToFit,
                        style = {
                            width = 16,
                            height = 16,
                            marginLeft = 6,
                            marginRight = 8,
                            unityBackgroundImageTintColor = new Color(0.7f, 0.7f, 0.7f)
                        }
                    };

                    row.Add(gripIcon);

                    var container = new VisualElement
                    {
                        style = {
                            flexDirection = FlexDirection.Column,
                            flexGrow = 1,
                            paddingRight = 6
                        }
                    };

                    var options = allowRemindCreation
                        ? new List<string> { "Wait", "Action", "Remind" }
                        : new List<string> { "Wait", "Action" };

                    var typeDropdown = new PopupField<string>("Type", options, 0);
                    container.Add(typeDropdown);

                    var waitField = new FloatField("Duration (s)") { value = 1.0f };
                    waitField.style.display = DisplayStyle.None;
                    container.Add(waitField);

                    var objectField = new ObjectField("Target Object") { objectType = typeof(GameObject) };
                    var methodDropdown = new PopupField<string>("Trigger Method", new List<string>(), 0);
                    methodDropdown.style.display = DisplayStyle.None;

                    container.Add(objectField);
                    container.Add(methodDropdown);

                    var deleteButton = new Button { text = "Remove Instruction" };
                    deleteButton.style.marginTop = 4;
                    container.Add(deleteButton);

                    row.Add(container);

                    return row;
                },

                bindItem = (element, i) =>
                {
                    var row = (VisualElement)element;
                    var container = row.ElementAt(1);

                    var instruction = _model.Instructions[i];

                    var typeDropdown = container.ElementAt(0) as PopupField<string>;
                    var waitField = container.ElementAt(1) as FloatField;
                    var objectField = container.ElementAt(2) as ObjectField;
                    var methodDropdown = container.ElementAt(3) as PopupField<string>;
                    var deleteButton = container.ElementAt(4) as Button;

                    var typeStr = instruction.Type.ToString();
                    if (!typeDropdown.choices.Contains(typeStr))
                        typeDropdown.choices.Add(typeStr);

                    typeDropdown.SetValueWithoutNotify(typeStr);
                    waitField.SetValueWithoutNotify(float.TryParse(instruction.Params, out var p) ? p : 1f);

                    waitField.style.display = instruction.Type == ActionInstructionType.Wait ? DisplayStyle.Flex : DisplayStyle.None;
                    bool showFields = instruction.Type == ActionInstructionType.Action || instruction.Type == ActionInstructionType.Remind;
                    objectField.style.display = showFields ? DisplayStyle.Flex : DisplayStyle.None;
                    methodDropdown.style.display = showFields ? DisplayStyle.Flex : DisplayStyle.None;

                    if (showFields && !string.IsNullOrEmpty(instruction.ObjectName))
                    {
                        var obj = GameObject.Find(instruction.ObjectName);
                        if (obj != null)
                        {
                            objectField.SetValueWithoutNotify(obj);
                            var available = TriggerAssignmentController.GetAvailableEvents(obj);
                            methodDropdown.choices = available;
                            methodDropdown.SetValueWithoutNotify(available.Contains(instruction.MethodName) ? instruction.MethodName : available.FirstOrDefault());
                        }
                        else
                        {
                            objectField.label = "Objeto no disponible en escena";
                            objectField.value = null;

                            methodDropdown.choices = new List<string> { instruction.MethodName };
                            methodDropdown.SetValueWithoutNotify(instruction.MethodName);
                        }
                    }

                    typeDropdown.RegisterValueChangedCallback(evt =>
                    {
                        if (Enum.TryParse(evt.newValue, out ActionInstructionType newType))
                        {
                            instruction.Type = newType;
                            waitField.style.display = newType == ActionInstructionType.Wait ? DisplayStyle.Flex : DisplayStyle.None;
                            bool show = newType == ActionInstructionType.Action || newType == ActionInstructionType.Remind;
                            objectField.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                            methodDropdown.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                            _controller.MarkDirty();
                        }
                    });

                    waitField.RegisterValueChangedCallback(evt =>
                    {
                        instruction.Params = evt.newValue.ToString();
                        _controller.MarkDirty();
                    });

                    objectField.RegisterValueChangedCallback(evt =>
                    {
                        var obj = evt.newValue as GameObject;
                        if (obj != null)
                        {
                            instruction.ObjectName = obj.name;
                            var methods = TriggerAssignmentController.GetAvailableEvents(obj);
                            methodDropdown.choices = methods;
                            if (methods.Count > 0)
                            {
                                instruction.MethodName = methods[0];
                                methodDropdown.SetValueWithoutNotify(methods[0]);
                                methodDropdown.style.display = DisplayStyle.Flex;
                            }
                            else
                            {
                                methodDropdown.style.display = DisplayStyle.None;
                            }
                            _controller.MarkDirty();
                        }
                    });

                    methodDropdown.RegisterValueChangedCallback(evt =>
                    {
                        instruction.MethodName = evt.newValue;
                        _controller.MarkDirty();
                    });

                    deleteButton.clicked += () =>
                    {
                        _model.Instructions.RemoveAt(i);
                        instructionList.Rebuild();
                        _controller.MarkDirty();
                    };
                },

                selectionType = SelectionType.None,
                reorderable = true,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            };

            Add(instructionList);

            var addButton = new Button(() =>
            {
                _model.Instructions.Add(new ActionInstruction(ActionInstructionType.Wait, "", "", "1"));
                instructionList.Rebuild();
                _controller.MarkDirty();
            })
            {
                text = "Add Instruction",
                style = { marginTop = 6 }
            };

            Add(addButton);

            Add(new VisualElement
            {
                style = {
                    height = 12,
                    marginTop = 6,
                    marginBottom = 6
                }
            });

            Add(new VisualElement
            {
                style = {
                    height = 1,
                    backgroundColor = new Color(0.3f, 0.3f, 0.3f),
                    marginTop = 4,
                    marginBottom = 4
                }
            });
        }
    }
}
