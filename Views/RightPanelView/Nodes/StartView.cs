using UnityEngine.UIElements;
using UnityEngine;
using Models.Nodes;
using Controllers;

namespace Views.RightPanel
{
    public class StartView : TweenityNodeView
    {
        public StartView(StartNodeModel model, GraphController controller) : base(model, controller)
        {
            var title = new Label("Start Node Details");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.whiteSpace = WhiteSpace.Normal;
            Add(title);

            var note = new Label("This is a Start Node. It does not contain any editable properties.");
            note.style.unityFontStyleAndWeight = FontStyle.Italic;
            note.style.marginTop = 10;
            note.style.whiteSpace = WhiteSpace.Normal;
            note.style.flexShrink = 0;
            Add(note);

            var connectButton = new Button(() =>
            {
                Debug.Log($"[StartView] Connect button clicked for NodeID: {model.NodeID}");
                // Placeholder action
            })
            {
                text = "Connect"
            };
            connectButton.style.marginTop = 15;
            Add(connectButton);
        }
    }
}
