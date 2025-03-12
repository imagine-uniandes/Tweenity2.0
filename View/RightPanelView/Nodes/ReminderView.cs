using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;


namespace Nodes
{
    public class ReminderView : VisualElement
    {
        public ReminderView()
        {
            this.style.paddingLeft = 5;
            this.style.paddingRight = 5;
            this.style.paddingTop = 5;
            this.style.paddingBottom = 5;
            this.style.borderTopLeftRadius = 5;
            this.style.borderTopRightRadius = 5;
            this.style.borderBottomLeftRadius = 5;
            this.style.borderBottomRightRadius = 5;
            
            Label reminderLabel = new Label("Reminder Node");
            reminderLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            this.Add(reminderLabel);

            this.Add(new Label("Reminder Text"));
            TextField reminderTextField = new TextField();
            this.Add(reminderTextField);

            this.Add(new Label("Reminder Timer (seconds)"));
            TextField reminderTimerField = new TextField();
            this.Add(reminderTimerField);
        }
    }
}
