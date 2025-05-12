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
            Add(new Label("Details")
            {
                style = {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    whiteSpace = WhiteSpace.Normal,
                    marginBottom = 10,
                    marginTop = 10
                }
            });

            if (model.OutgoingPaths.Count == 0)
            {
                var connectButton = new Button(() =>
                {
                    controller.StartConnectionFrom(model.NodeID, (targetNodeId) =>
                    {
                        model.OutgoingPaths.Add(new PathData("Next", "", targetNodeId));
                        controller.GraphView.RenderConnections();
                    });
                })
                {
                    text = "Connect"
                };
                connectButton.style.marginTop = 10;
                Add(connectButton);
            }
            else
            {
                Add(new Label("Outgoing Connection")
                {
                    style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 }
                });

                var path = model.OutgoingPaths[0];
                var connectedModel = controller.GetNode(path.TargetNodeID);
                var connectionLabel = new Label($"→ {connectedModel?.Title ?? "(Unknown)"}")
                {
                    style = { whiteSpace = WhiteSpace.Normal }
                };
                Add(connectionLabel);
            }
        }
    }
}
