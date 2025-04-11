using UnityEngine.UIElements;
using UnityEngine;
using Models.Nodes;
using Controllers;
using System.Linq;

namespace Views.RightPanel
{
    public class MultipleChoiceView : TweenityNodeView
    {
        private ListView _choicesList;
        private const int MaxChoiceLength = 39;

        public MultipleChoiceView(MultipleChoiceNodeModel model, GraphController controller) : base(model, controller)
        {
            Add(new Label("Multiple Choice Node Details")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, whiteSpace = WhiteSpace.Normal }
            });

            Add(new Label("Question"));

            var questionField = new TextField { value = model.Question, multiline = true };
            questionField.RegisterValueChangedCallback(evt =>
            {
                model.Question = evt.newValue;
            });
            Add(questionField);

            Add(new Label("Answers"));

            var addChoiceButton = new Button(() =>
            {
                model.OutgoingPaths.Add(new PathData($"Choice {model.OutgoingPaths.Count + 1}"));
                _choicesList.Rebuild();
                controller.GraphView.RefreshNodeVisual(model.NodeID);
            })
            {
                text = "+ Add Answer"
            };
            Add(addChoiceButton);

            _choicesList = new ListView(model.OutgoingPaths, 90, () =>
            {
                var container = new VisualElement
                {
                    style = {
                        flexDirection = FlexDirection.Column,
                        marginBottom = 10
                    }
                };

                var answerField = new TextField
                {
                    multiline = true,
                    maxLength = MaxChoiceLength,
                    style = {
                        whiteSpace = WhiteSpace.Normal,
                        flexGrow = 0,
                        overflow = Overflow.Visible,
                        marginBottom = 2,
                        minHeight = 20,
                        backgroundColor = Color.clear,
                        borderBottomWidth = 0,
                        borderTopWidth = 0,
                        borderLeftWidth = 0,
                        borderRightWidth = 0
                    }
                };

                answerField.RegisterCallback<ChangeEvent<string>>(evt =>
                {
                    int lineCount = evt.newValue.Count(c => c == '\n') + 1;
                    answerField.style.height = Mathf.Max(20, lineCount * 18);
                });

                var buttonRow = new VisualElement
                {
                    style = {
                        flexDirection = FlexDirection.Row,
                        justifyContent = Justify.SpaceBetween
                    }
                };

                var triggerButton = new Button(() => Debug.Log("[Trigger]"))
                {
                    text = "Trigger",
                    style = {
                        flexGrow = 1,
                        marginRight = 4
                    }
                };

                var connectButton = new Button
                {
                    text = "Connect",
                    style = { flexGrow = 1 }
                };

                buttonRow.Add(triggerButton);
                buttonRow.Add(connectButton);

                container.Add(answerField);
                container.Add(buttonRow);

                return container;
            },
            (element, i) =>
            {
                var container = element as VisualElement;
                var answerField = container.ElementAt(0) as TextField;
                var buttonRow = container.ElementAt(1) as VisualElement;
                var triggerButton = buttonRow.ElementAt(0) as Button;
                var connectButton = buttonRow.ElementAt(1) as Button;

                var path = model.OutgoingPaths[i];
                answerField.value = path.Label;

                answerField.RegisterValueChangedCallback(evt =>
                {
                    path.Label = evt.newValue.Length > MaxChoiceLength ? evt.newValue.Substring(0, MaxChoiceLength) : evt.newValue;
                    answerField.SetValueWithoutNotify(path.Label);
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
                        controller.StartConnectionFrom(model.NodeID, targetId =>
                        {
                            model.OutgoingPaths[i].TargetNodeID = targetId;
                            controller.GraphView.RenderConnections();
                            _choicesList.Rebuild();
                        });
                    });
                }
            });

            _choicesList.selectionType = SelectionType.None;
            _choicesList.style.flexGrow = 0;
            _choicesList.style.flexShrink = 1;
            _choicesList.style.height = StyleKeyword.Auto;
            _choicesList.style.borderBottomWidth = 0;
            _choicesList.style.borderTopWidth = 0;
            _choicesList.style.borderLeftWidth = 0;
            _choicesList.style.borderRightWidth = 0;
            _choicesList.style.backgroundColor = Color.clear;

            Add(_choicesList);
        }
    }
}
