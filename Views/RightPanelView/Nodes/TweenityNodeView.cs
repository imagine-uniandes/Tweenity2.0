using UnityEngine.UIElements;
using UnityEngine;
using Models;
using Models.Nodes;
using Controllers;
using System;

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

            // Title
            Add(new Label("Base Node Info") { style = { unityFontStyleAndWeight = FontStyle.Bold } });

            var titleLabel = new Label("Title") { style = { whiteSpace = WhiteSpace.Normal } };
            var titleField = new TextField { value = _model.Title, name = "titleField" };
            titleField.RegisterValueChangedCallback(evt =>
            {
                _controller.UpdateNodeTitle(_model, evt.newValue);
            });

            // Description
            var descriptionLabel = new Label("Description") { style = { whiteSpace = WhiteSpace.Normal } };
            var descriptionField = new TextField
            {
                value = _model.Description,
                multiline = true,
                name = "descriptionField"
            };
            descriptionField.RegisterValueChangedCallback(evt =>
            {
                _controller.UpdateNodeDescription(_model, evt.newValue);
            });

            // Type Dropdown
            var typeLabel = new Label("Node Type") { style = { whiteSpace = WhiteSpace.Normal } };
            var nodeTypes = new System.Collections.Generic.List<string>(Enum.GetNames(typeof(NodeType)));
            var selectedType = _model.Type.ToString();
            var typeDropdown = new DropdownField(nodeTypes, selectedType, null);
            typeDropdown.RegisterValueChangedCallback(evt =>
            {
                if (Enum.TryParse<NodeType>(evt.newValue, out var newType))
                {
                    var (success, errorMessage) = _controller.ChangeNodeType(_model, newType);
                    if (!success)
                    {
                        typeDropdown.SetValueWithoutNotify(_model.Type.ToString());
#if UNITY_EDITOR
                        UnityEditor.EditorUtility.DisplayDialog("Invalid Node Type", errorMessage, "OK");
#endif
                    }
                }
            });

            // Position (read-only display)
            var positionLabel = new Label("Position");
            var positionValue = new Label($"{_model.Position.x:F0}, {_model.Position.y:F0}")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Italic,
                    color = new Color(0.4f, 0.4f, 0.4f),
                    marginBottom = 6
                }
            };

            Add(titleLabel);
            Add(titleField);
            Add(descriptionLabel);
            Add(descriptionField);
            Add(typeLabel);
            Add(typeDropdown);
            Add(positionLabel);
            Add(positionValue);

            Add(new VisualElement { style = { height = 10 } });
        }
    }
}
