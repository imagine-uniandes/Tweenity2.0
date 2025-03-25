using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using Models.Nodes;
using Controllers;

namespace Views.RightPanel
{
    public class NoTypeView : VisualElement
    {
        private NoTypeNodeModel _model;
        private GraphController _controller;

        public NoTypeView(NoTypeNodeModel model, GraphController controller)
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

            Label header = new Label("Basic Node");
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            this.Add(header);

            this.Add(new Label("Title"));
            var titleField = new TextField { value = _model.Title };
            titleField.RegisterValueChangedCallback(evt =>
            {
                _controller.UpdateNoTypeTitle(_model, evt.newValue);
            });
            this.Add(titleField);

            this.Add(new Label("Description"));
            var descriptionField = new TextField { value = _model.Description };
            descriptionField.RegisterValueChangedCallback(evt =>
            {
                _controller.UpdateNoTypeDescription(_model, evt.newValue);
            });
            this.Add(descriptionField);
        }
    }
}
