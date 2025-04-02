using UnityEngine.UIElements;
using UnityEngine;
using Models.Nodes;
using Controllers;

namespace Views.RightPanel
{
    public class RandomView : TweenityNodeView
    {
        private ListView _pathsList;

        public RandomView(RandomNodeModel model, GraphController controller) : base(model, controller)
        {
            var title = new Label("Random Node Details")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, whiteSpace = WhiteSpace.Normal }
            };
            Add(title);

            var typedModel = (RandomNodeModel)_model;

            var label = new Label("Possible Paths");
            label.style.whiteSpace = WhiteSpace.Normal;
            Add(label);

            var addPathButton = new Button(() =>
            {
                _controller.AddRandomPath(typedModel);
                _pathsList.Rebuild();
            })
            {
                text = "+ Add Path"
            };
            Add(addPathButton);

            _pathsList = new ListView(typedModel.PossiblePaths, 30, () =>
            {
                var container = new VisualElement
                {
                    style = { flexDirection = FlexDirection.Row, marginBottom = 4 }
                };

                var pathField = new TextField { style = { flexGrow = 1, marginRight = 4 } };
                var connectButton = new Button
                {
                    text = "Connect",
                    style = { flexShrink = 0 }
                };

                container.Add(pathField);
                container.Add(connectButton);
                return container;
            },
            (e, i) =>
            {
                var container = e as VisualElement;
                var pathField = container.ElementAt(0) as TextField;
                var connectButton = container.ElementAt(1) as Button;

                pathField.value = typedModel.PossiblePaths[i];
                pathField.RegisterValueChangedCallback(evt =>
                {
                    typedModel.UpdatePath(i, evt.newValue);
                });

                connectButton.clickable = new Clickable(() =>
                {
                    Debug.Log($"[RandomView] Connect clicked for path {i} in NodeID: {typedModel.NodeID}");
                    _controller.StartConnectionFrom(typedModel.NodeID, (targetNodeId) =>
                    {
                        _controller.ConnectNodes(typedModel.NodeID, targetNodeId);
                    });
                });
            });

            Add(_pathsList);

            Add(new Label("Outgoing Connections") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10 } });

            foreach (var nodeId in typedModel.ConnectedNodes)
            {
                var nodeLabel = new Label($"Connected to: {nodeId}");
                nodeLabel.style.whiteSpace = WhiteSpace.Normal;
                Add(nodeLabel);
            }
        }
    }
}
