using UnityEngine.UIElements;
using UnityEngine;
using Models.Nodes;
using Controllers;

namespace Views.RightPanel
{
    public class RandomView : TweenityNodeView
    {
        private ListView _pathsList;
        private const int MaxLabelLength = 39;

        public RandomView(RandomNodeModel model, GraphController controller) : base(model, controller)
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

            Add(new Label("Possible Paths"));

            var addPathButton = new Button(() =>
            {
                controller.AddRandomPath(model);
                _pathsList.Rebuild();
            })
            {
                text = "+ Add Path"
            };
            Add(addPathButton);

            _pathsList = new ListView(model.OutgoingPaths, 70, () =>
            {
                var container = new VisualElement
                {
                    style = {
                        flexDirection = FlexDirection.Column,
                        marginBottom = 6,
                        backgroundColor = new Color(0, 0, 0, 0)
                    }
                };

                var pathField = new TextField
                {
                    multiline = true,
                    maxLength = MaxLabelLength,
                    style = {
                        whiteSpace = WhiteSpace.Normal,
                        overflow = Overflow.Visible,
                        flexGrow = 0,
                        minHeight = 20,
                        marginBottom = 4,
                        backgroundColor = Color.clear,
                        borderBottomWidth = 0,
                        borderTopWidth = 0,
                        borderLeftWidth = 0,
                        borderRightWidth = 0
                    }
                };

                pathField.RegisterCallback<ChangeEvent<string>>(evt =>
                {
                    int lineCount = evt.newValue.Split('\n').Length;
                    pathField.style.height = Mathf.Max(20, lineCount * 18);
                });

                var buttonRow = new VisualElement
                {
                    style = {
                        flexDirection = FlexDirection.Row,
                        justifyContent = Justify.SpaceBetween
                    }
                };

                var connectButton = new Button { text = "Connect", style = { flexGrow = 1 } };

                buttonRow.Add(connectButton);
                container.Add(pathField);
                container.Add(buttonRow);

                return container;
            },
            (element, i) =>
            {
                var container = element as VisualElement;
                var pathField = container.ElementAt(0) as TextField;
                var buttonRow = container.ElementAt(1) as VisualElement;
                var connectButton = buttonRow.ElementAt(0) as Button;

                var path = model.OutgoingPaths[i];
                pathField.value = path.Label;

                pathField.RegisterValueChangedCallback(evt =>
                {
                    var clamped = evt.newValue.Length > MaxLabelLength ? evt.newValue.Substring(0, MaxLabelLength) : evt.newValue;
                    model.UpdatePathLabel(path.TargetNodeID, clamped);
                    pathField.SetValueWithoutNotify(clamped);
                    controller.GraphView.RefreshNodeVisual(model.NodeID);
                });

                if (!string.IsNullOrEmpty(path.TargetNodeID))
                {
                    var targetName = controller.GetNode(path.TargetNodeID)?.Title ?? "(Unknown)";
                    connectButton.text = $"→ {targetName}";
                    connectButton.SetEnabled(false);
                }
                else
                {
                    connectButton.text = "Connect";
                    connectButton.SetEnabled(true);
                    connectButton.clickable = new Clickable(() =>
                    {
                        controller.StartConnectionFrom(model.NodeID, (targetNodeId) =>
                        {
                            model.UpdatePathTarget(path.TargetNodeID, targetNodeId);
                            controller.GraphView.RenderConnections();
                            _pathsList.Rebuild();
                        });
                    });
                }
            });

            _pathsList.selectionType = SelectionType.None;
            _pathsList.style.borderBottomWidth = 0;
            _pathsList.style.borderTopWidth = 0;
            _pathsList.style.borderLeftWidth = 0;
            _pathsList.style.borderRightWidth = 0;
            _pathsList.style.backgroundColor = Color.clear;
            _pathsList.style.flexGrow = 0;
            _pathsList.style.flexShrink = 1;
            _pathsList.style.height = StyleKeyword.Auto;

            Add(_pathsList);
        }
    }
}
