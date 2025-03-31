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
            var title = new Label("Reminder Node Details");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.whiteSpace = WhiteSpace.Normal;
            Add(title);

            var typedModel = (ReminderNodeModel)_model;

            var textLabel = new Label("Reminder Text");
            textLabel.style.whiteSpace = WhiteSpace.Normal;
            Add(textLabel);

            var reminderTextField = new TextField { value = typedModel.ReminderText, multiline = true };
            reminderTextField.RegisterValueChangedCallback(evt =>
            {
                _controller.UpdateReminderText(typedModel, evt.newValue);
            });
            Add(reminderTextField);

            var timerLabel = new Label("Reminder Timer (seconds)");
            timerLabel.style.whiteSpace = WhiteSpace.Normal;
            Add(timerLabel);

            var timerField = new FloatField { value = typedModel.ReminderTimer };
            timerField.RegisterValueChangedCallback(evt =>
            {
                _controller.UpdateReminderTimer(typedModel, evt.newValue);
            });
            Add(timerField);

            var connectButton = new Button(() =>
            {
                Debug.Log($"[ReminderView] Connect button clicked for NodeID: {model.NodeID}");
                // Placeholder for connection logic
            })
            {
                text = "Connect"
            };
            connectButton.style.marginTop = 15;
            Add(connectButton);
        }
    }
}
