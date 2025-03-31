using UnityEngine.UIElements;
using UnityEngine;
using Models.Nodes;
using Controllers;

namespace Views.RightPanel
{
    public class NoTypeView : TweenityNodeView
    {
        public NoTypeView(NoTypeNodeModel model, GraphController controller) : base(model, controller)
        {
            var title = new Label("Generic Node Details");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.whiteSpace = WhiteSpace.Normal;
            Add(title);

            var label = new Label("This node has no specific properties.");
            label.style.unityFontStyleAndWeight = FontStyle.Italic;
            label.style.marginTop = 10;
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.flexShrink = 0;
            Add(label);

            var connectButton = new Button(() =>
            {
                Debug.Log($"[NoTypeView] Connect clicked for NodeID: {model.NodeID}");
                // Placeholder: invoke connection logic in controller
            })
            {
                text = "Connect"
            };
            connectButton.style.marginTop = 10;
            Add(connectButton);
        }
    }
}
