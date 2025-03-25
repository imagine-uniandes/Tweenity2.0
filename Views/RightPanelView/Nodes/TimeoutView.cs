using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using Models.Nodes;
using Controllers;

namespace Views.RightPanel
{
    public class TimeoutView : VisualElement
    {
        private TimeoutNodeModel _model;
        private GraphController _controller;

        public TimeoutView(TimeoutNodeModel model, GraphController controller)
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

            Label timeoutLabel = new Label("Timeout Node");
            timeoutLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            this.Add(timeoutLabel);

            this.Add(new Label("Timeout Condition"));
            TextField conditionField = new TextField { value = _model.Condition };
            conditionField.RegisterValueChangedCallback(evt =>
            {
                _controller.UpdateTimeoutCondition(_model, evt.newValue);
            });
            this.Add(conditionField);

            this.Add(new Label("Timeout Timer (seconds)"));
            FloatField timeoutTimerField = new FloatField { value = _model.TimeoutDuration };
            timeoutTimerField.RegisterValueChangedCallback(evt =>
            {
                _controller.UpdateTimeoutTimer(_model, evt.newValue);
            });
            this.Add(timeoutTimerField);
        }
    }
}
