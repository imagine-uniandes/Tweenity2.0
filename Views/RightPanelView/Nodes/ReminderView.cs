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
                controller.UpdateReminderText(typedModel, evt.newValue);
            });
            Add(reminderTextField);

            var timerLabel = new Label("Reminder Timer (seconds)");
            timerLabel.style.whiteSpace = WhiteSpace.Normal;
            Add(timerLabel);

            var timerField = new FloatField { value = typedModel.ReminderTimer };
            timerField.RegisterValueChangedCallback(evt =>
            {
                controller.UpdateReminderTimer(typedModel, evt.newValue);
            });
            Add(timerField);

            // Solo se permite una conexión
            if (typedModel.OutgoingPaths.Count == 0)
            {
                var connectButton = new Button(() =>
                {
                    Debug.Log($"[ReminderView] Connect clicked for NodeID: {typedModel.NodeID}");
                    controller.StartConnectionFrom(typedModel.NodeID, targetNodeId =>
                    {
                        typedModel.ConnectTo(targetNodeId, "Next");
                        controller.GraphView.RenderConnections();
                    });
                })
                {
                    text = "Connect"
                };
                connectButton.style.marginTop = 15;
                Add(connectButton);
            }
            else
            {
                Add(new Label("Outgoing Connection")
                {
                    style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 }
                });

                var connection = typedModel.OutgoingPaths[0];
                var connectedModel = controller.GetNode(connection.TargetNodeID);
                var label = new Label($"→ {connectedModel?.Title ?? "(Unknown)"}");
                label.style.whiteSpace = WhiteSpace.Normal;
                Add(label);
            }
        }
    }
}
