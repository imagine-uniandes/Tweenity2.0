using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace Views.MiddlePanel
{
    public class DialogueNode : TweenityNode
    {
        public DialogueNode(string nodeID) : base(nodeID)
        {
            this.title = "Dialogue Node";

            // Minimal visual cue that this is a Dialogue node
            Label typeLabel = new Label("Type: Dialogue");
            typeLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
            typeLabel.style.marginTop = 4;
            typeLabel.style.marginBottom = 4;
            typeLabel.style.marginLeft = 5;

            this.extensionContainer.Add(typeLabel);

            this.RefreshExpandedState();
            this.RefreshPorts();
        }
    }
}
