using UnityEngine.UIElements;
using UnityEngine;
using Models.Nodes;
using Controllers;

namespace Views.RightPanel
{
    public class ReminderView : TweenityNodeView
    {
        public ReminderView(ReminderNodeModel model, GraphController controller) : base(model, controller)
        {
            Add(new Label("Reminder Node Details") { style = { unityFontStyleAndWeight = FontStyle.Bold } });

            var typedModel = (ReminderNodeModel)_model;

            Add(new Label("Reminder Text"));
            TextField reminderTextField = new TextField { value = typedModel.ReminderText, multiline = true };
            reminderTextField.RegisterValueChangedCallback(evt =>
            {
                _controller.UpdateReminderText(typedModel, evt.newValue);
            });
            Add(reminderTextField);

            Add(new Label("Reminder Timer (seconds)"));
            FloatField timerField = new FloatField { value = typedModel.ReminderTimer };
            timerField.RegisterValueChangedCallback(evt =>
            {
                _controller.UpdateReminderTimer(typedModel, evt.newValue);
            });
            Add(timerField);
        }
    }
}
