using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using Models.Nodes;
using Controllers;

namespace Views.RightPanel
{
    public class ReminderView : VisualElement
    {
        private ReminderNodeModel _model;
        private GraphController _controller;

        public ReminderView(ReminderNodeModel model, GraphController controller)
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

            Label header = new Label("Reminder Node");
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            this.Add(header);

            this.Add(new Label("Reminder Text"));
            TextField reminderTextField = new TextField { value = _model.ReminderText };
            reminderTextField.RegisterValueChangedCallback(evt =>
            {
                _controller.UpdateReminderText(_model, evt.newValue);
            });
            this.Add(reminderTextField);

            this.Add(new Label("Reminder Timer (seconds)"));
            FloatField timerField = new FloatField { value = _model.ReminderTimer };
            timerField.RegisterValueChangedCallback(evt =>
            {
                _controller.UpdateReminderTimer(_model, evt.newValue);
            });
            this.Add(timerField);
        }
    }
}
