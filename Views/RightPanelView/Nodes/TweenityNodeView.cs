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

            // Title
            Label titleLabel = new Label("Title");
            TextField titleField = new TextField { value = _model.Title, name = "titleField" };
            titleField.RegisterValueChangedCallback(evt =>
            {
                _controller.UpdateNodeTitle(_model, evt.newValue);
            });

            // Description
            Label descriptionLabel = new Label("Description");
            TextField descriptionField = new TextField { value = _model.Description, multiline = true, name = "descriptionField" };
            descriptionField.RegisterValueChangedCallback(evt =>
            {
                _controller.UpdateNodeDescription(_model, evt.newValue);
            });

            // Node Type Dropdown
            Label typeLabel = new Label("Node Type");
            var nodeTypes = new System.Collections.Generic.List<string>(Enum.GetNames(typeof(NodeType)));
            var selectedType = _model.Type.ToString(); // Aquí obtenemos el valor en string del tipo de nodo

            var typeDropdown = new DropdownField(nodeTypes, selectedType, null); // Tercer parámetro a null
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

            Add(new VisualElement { style = { height = 10 } }); // Spacer
        }
    }
}
