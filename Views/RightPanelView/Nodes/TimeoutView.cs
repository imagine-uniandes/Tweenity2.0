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

            var conditionLabel = new Label("Timeout Condition");
            conditionLabel.style.whiteSpace = WhiteSpace.Normal;
            Add(conditionLabel);

            var conditionField = new TextField { value = typedModel.Condition };
            conditionField.RegisterValueChangedCallback(evt =>
            {
                _controller.UpdateTimeoutCondition(typedModel, evt.newValue);
            });
            Add(conditionField);

            var timerLabel = new Label("Timeout Timer (seconds)");
            timerLabel.style.whiteSpace = WhiteSpace.Normal;
            Add(timerLabel);

            var timeoutTimerField = new FloatField { value = typedModel.TimeoutDuration };
            timeoutTimerField.RegisterValueChangedCallback(evt =>
            {
                _controller.UpdateTimeoutTimer(typedModel, evt.newValue);
            });
            Add(timeoutTimerField);
        }
    }
}
