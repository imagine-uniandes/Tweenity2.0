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
                Debug.Log($"[StartView] Connect clicked for NodeID: {model.NodeID}");
                controller.StartConnectionFrom(model.NodeID, (targetNodeId) =>
                {
                    controller.ConnectNodes(model.NodeID, targetNodeId);
                });
            })
            {
                text = "Connect"
            };
            connectButton.style.marginTop = 15;
            Add(connectButton);

            Add(new Label("Outgoing Connections")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 }
            });

            foreach (var nodeId in model.ConnectedNodes)
            {
                var label = new Label($"Connected to: {nodeId}");
                label.style.whiteSpace = WhiteSpace.Normal;
                Add(label);
            }
        }
    }
}
