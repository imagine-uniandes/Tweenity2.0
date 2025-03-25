using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;

namespace Views.RightPanel{

    public class TimeoutView : VisualElement
    {
        public TimeoutView()
        {
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
            Button conditionButton = new Button(() => Debug.Log("Set Condition")) { text = "Condition" };
            this.Add(conditionButton);

            this.Add(new Label("Timeout Timer (seconds)"));
            TextField timeoutTimerField = new TextField();
            this.Add(timeoutTimerField);
        }
    }
}
