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
            Add(new Label("Timeout Node Details")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, whiteSpace = WhiteSpace.Normal }
            });

            var typedModel = (TimeoutNodeModel)_model;

            Add(new Label("Timeout Condition") { style = { whiteSpace = WhiteSpace.Normal } });

            var conditionField = new TextField { value = typedModel.Condition };
            conditionField.RegisterValueChangedCallback(evt =>
            {
                typedModel.Condition = evt.newValue;
                controller.GraphView.RefreshNodeVisual(typedModel.NodeID);
            });
            Add(conditionField);

            Add(new Label("Timeout Timer (seconds)") { style = { whiteSpace = WhiteSpace.Normal } });

            var timeoutTimerField = new FloatField { value = typedModel.TimeoutDuration };
            timeoutTimerField.RegisterValueChangedCallback(evt =>
            {
                typedModel.TimeoutDuration = evt.newValue;
                controller.GraphView.RefreshNodeVisual(typedModel.NodeID);
            });
            Add(timeoutTimerField);

            Add(new Label("Outgoing Connections")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 }
            });

            bool hasTimeout = false;
            bool hasSuccess = false;

            foreach (var path in typedModel.OutgoingPaths)
            {
                var connectedModel = controller.GetNode(path.TargetNodeID);
                var labelText = $"→ {path.Label} ({path.Trigger}) → {connectedModel?.Title ?? "(Unknown)"}";

                Add(new Label(labelText) { style = { whiteSpace = WhiteSpace.Normal, marginBottom = 2 } });

                if (path.Trigger == "onTimeout") hasTimeout = true;
                if (path.Trigger == "onSuccess") hasSuccess = true;
            }

            if (!hasTimeout)
            {
                var timeoutBtn = new Button(() =>
                {
                    controller.StartConnectionFrom(typedModel.NodeID, (targetId) =>
                    {
                        typedModel.ConnectTo(targetId, "Timeout", "onTimeout");
                        controller.GraphView.RenderConnections();
                        controller.OnNodeSelected(typedModel); // refresh panel
                    });
                })
                {
                    text = "Connect (On Timeout)",
                    style = { marginTop = 4 }
                };

                Add(timeoutBtn);
            }

            if (!hasSuccess)
            {
                var successBtn = new Button(() =>
                {
                    controller.StartConnectionFrom(typedModel.NodeID, (targetId) =>
                    {
                        typedModel.ConnectTo(targetId, "Success", "onSuccess");
                        controller.GraphView.RenderConnections();
                        controller.OnNodeSelected(typedModel);
                    });
                })
                {
                    text = "Connect (On Success)",
                    style = { marginTop = 4 }
                };

                Add(successBtn);
            }
        }
    }
}
