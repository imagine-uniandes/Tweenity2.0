using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace Nodes
{
    public class TimeoutNode : TweenityNode
    {
        public TimeoutNode(string nodeID) : base(nodeID)
        {
            this.title = "Timeout Node";
            
            // Timeout Condition Button
            Button conditionButton = new Button(() => Debug.Log("Set Condition")) { text = "Condition" };
            this.extensionContainer.Add(conditionButton);

            // Timeout Timer Field
            this.extensionContainer.Add(new Label("Timeout Timer (seconds)"));
            TextField timeoutTimerField = new TextField();
            this.extensionContainer.Add(timeoutTimerField);

            this.RefreshExpandedState();
            this.RefreshPorts();
        }
    }
}
