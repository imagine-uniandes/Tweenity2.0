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

            this.style.paddingLeft = 5;
            this.style.paddingRight = 5;
            this.style.paddingTop = 5;
            this.style.paddingBottom = 5;
            this.style.borderTopLeftRadius = 5;
            this.style.borderTopRightRadius = 5;
            this.style.borderBottomLeftRadius = 5;
            this.style.borderBottomRightRadius = 5;
            this.style.flexDirection = FlexDirection.Column;

            // Title
            var titleLabel = new Label("Title");
            titleLabel.style.whiteSpace = WhiteSpace.Normal;

            var titleField = new TextField { value = _model.Title, name = "titleField" };
            titleField.RegisterValueChangedCallback(evt =>
            {
                _controller.UpdateNodeTitle(_model, evt.newValue);
            });
            titleField.style.flexShrink = 0;
            titleField.style.flexGrow = 1;

            // Description
            var descriptionLabel = new Label("Description");
            descriptionLabel.style.whiteSpace = WhiteSpace.Normal;

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
            descriptionField.style.whiteSpace = WhiteSpace.Normal;
            descriptionField.style.flexShrink = 0;
            descriptionField.style.flexGrow = 1;

            // Node Type Dropdown
            var typeLabel = new Label("Node Type");
            typeLabel.style.whiteSpace = WhiteSpace.Normal;

            var nodeTypes = new System.Collections.Generic.List<string>(Enum.GetNames(typeof(NodeType)));
            var selectedType = _model.Type.ToString();

            var typeDropdown = new DropdownField(nodeTypes, selectedType, null);
            typeDropdown.RegisterValueChangedCallback(evt =>
            {
                if (Enum.TryParse<NodeType>(evt.newValue, out var newType))
                {
                    _controller.ChangeNodeType(_model, newType);
                }
            });

            Add(new Label("Base Node Info") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
            Add(titleLabel);
            Add(titleField);
            Add(descriptionLabel);
            Add(descriptionField);
            Add(typeLabel);
            Add(typeDropdown);

            Add(new VisualElement { style = { height = 10 } });
        }
    }
}
