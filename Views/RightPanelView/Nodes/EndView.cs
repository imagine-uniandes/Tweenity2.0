using UnityEngine.UIElements;
using UnityEngine;
using Models.Nodes;
using Controllers;

namespace Views.RightPanel
{
    public class EndView : TweenityNodeView
    {
        public EndView(EndNodeModel model, GraphController controller) : base(model, controller)
        {
            var title = new Label("End Node Details");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.whiteSpace = WhiteSpace.Normal;
            Add(title);

            var note = new Label("This is an End Node. No editable properties.");
            note.style.unityFontStyleAndWeight = FontStyle.Italic;
            note.style.marginTop = 10;
            note.style.whiteSpace = WhiteSpace.Normal;
            note.style.flexShrink = 0;
            Add(note);

            // Incoming connections (for display only)
            Add(new Label("Incoming Connections")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 }
            });

            foreach (var nodeId in model.ConnectedNodes)
            {
                var label = new Label($"Connected from: {nodeId}")
                {
                    style = { whiteSpace = WhiteSpace.Normal }
                };
                Add(label);
            }
        }
    }
}
