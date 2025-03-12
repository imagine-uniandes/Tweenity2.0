using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace Nodes
{
    public class ReminderNode : TweenityNode
    {
        public ReminderNode(string nodeID) : base(nodeID)
        {
            this.title = "Reminder Node";
            
            // Reminder Text Field
            this.extensionContainer.Add(new Label("Reminder Text"));
            TextField reminderTextField = new TextField();
            this.extensionContainer.Add(reminderTextField);

            // Reminder Timer Field
            this.extensionContainer.Add(new Label("Reminder Timer (seconds)"));
            TextField reminderTimerField = new TextField();
            this.extensionContainer.Add(reminderTimerField);

            this.RefreshExpandedState();
            this.RefreshPorts();
        }
    }
}
