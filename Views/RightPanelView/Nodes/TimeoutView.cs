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
            Add(new Label("Timeout Node Details") { style = { unityFontStyleAndWeight = FontStyle.Bold } });

            var typedModel = (TimeoutNodeModel)_model;

            Add(new Label("Timeout Condition"));
            TextField conditionField = new TextField { value = typedModel.Condition };
            conditionField.RegisterValueChangedCallback(evt =>
            {
                _controller.UpdateTimeoutCondition(typedModel, evt.newValue);
            });
            Add(conditionField);

            Add(new Label("Timeout Timer (seconds)"));
            FloatField timeoutTimerField = new FloatField { value = typedModel.TimeoutDuration };
            timeoutTimerField.RegisterValueChangedCallback(evt =>
            {
                _controller.UpdateTimeoutTimer(typedModel, evt.newValue);
            });
            Add(timeoutTimerField);
        }
    }
}
