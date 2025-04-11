using UnityEngine.UIElements;
using UnityEngine;
using Models.Nodes;
using Controllers;

namespace Views.RightPanel
{
    public class TimeoutView : TweenityNodeView
    {
        public TimeoutView(TimeoutNodeModel model, GraphController controller) : base(model, controller)
        {
            var title = new Label("Timeout Node Details");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.whiteSpace = WhiteSpace.Normal;
            Add(title);

            var typedModel = (TimeoutNodeModel)_model;

            // Timeout Condition
            var conditionLabel = new Label("Timeout Condition");
            conditionLabel.style.whiteSpace = WhiteSpace.Normal;
            Add(conditionLabel);

            var conditionField = new TextField { value = typedModel.Condition };
            conditionField.RegisterValueChangedCallback(evt =>
            {
                typedModel.Condition = evt.newValue;
                controller.GraphView.RefreshNodeVisual(typedModel.NodeID);
            });
            Add(conditionField);

            // Timeout Duration
            var timerLabel = new Label("Timeout Timer (seconds)");
            timerLabel.style.whiteSpace = WhiteSpace.Normal;
            Add(timerLabel);

            var timeoutTimerField = new FloatField { value = typedModel.TimeoutDuration };
            timeoutTimerField.RegisterValueChangedCallback(evt =>
            {
                typedModel.TimeoutDuration = evt.newValue;
                controller.GraphView.RefreshNodeVisual(typedModel.NodeID);
            });
            Add(timeoutTimerField);

            // Connect buttons
            var connectTimeoutButton = new Button(() =>
            {
                Debug.Log($"[TimeoutView] Connect (On Timeout) clicked for NodeID: {typedModel.NodeID}");
                controller.StartConnectionFrom(typedModel.NodeID, (targetNodeId) =>
                {
                    typedModel.ConnectTo(targetNodeId, "Timeout", "onTimeout");
                    controller.GraphView.RenderConnections();
                });
            })
            {
                text = "Connect (On Timeout)"
            };
            connectTimeoutButton.style.marginTop = 10;
            Add(connectTimeoutButton);

            var connectSuccessButton = new Button(() =>
            {
                Debug.Log($"[TimeoutView] Connect (On Success) clicked for NodeID: {typedModel.NodeID}");
                controller.StartConnectionFrom(typedModel.NodeID, (targetNodeId) =>
                {
                    typedModel.ConnectTo(targetNodeId, "Success", "onSuccess");
                    controller.GraphView.RenderConnections();
                });
            })
            {
                text = "Connect (On Success)"
            };
            Add(connectSuccessButton);

            // Show connections
            Add(new Label("Outgoing Connections")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 }
            });

            foreach (var path in typedModel.OutgoingPaths)
            {
                var connectedModel = controller.GetNode(path.TargetNodeID);
                var label = new Label($"→ {path.Label} ({path.Trigger}) → {connectedModel?.Title ?? "(Unknown)"}")
                {
                    style = { whiteSpace = WhiteSpace.Normal }
                };
                Add(label);
            }
        }
    }
}
