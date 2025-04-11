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

            if (model.OutgoingPaths.Count == 0)
            {
                var connectButton = new Button(() =>
                {
                    Debug.Log($"[StartView] Connect clicked for NodeID: {model.NodeID}");
                    controller.StartConnectionFrom(model.NodeID, (targetNodeId) =>
                    {
                        model.ConnectTo(targetNodeId, "Start");
                        controller.GraphView.RenderConnections();
                    });
                })
                {
                    text = "Connect"
                };
                connectButton.style.marginTop = 15;
                Add(connectButton);
            }
            else
            {
                Add(new Label("Outgoing Connection")
                {
                    style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 }
                });

                var connection = model.OutgoingPaths[0];
                var connectedModel = controller.GetNode(connection.TargetNodeID);
                var label = new Label($"→ {connectedModel?.Title ?? "(Unknown)"}");
                label.style.whiteSpace = WhiteSpace.Normal;
                Add(label);
            }
        }
    }
}
