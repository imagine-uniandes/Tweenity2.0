using UnityEngine.UIElements;
using UnityEngine;
using Models.Nodes;
using Controllers;

namespace Views.RightPanel
{
    public class MultipleChoiceView : TweenityNodeView
    {
        private ListView _choicesList;

        public MultipleChoiceView(MultipleChoiceNodeModel model, GraphController controller) : base(model, controller)
        {
            var title = new Label("Multiple Choice Node Details")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    whiteSpace = WhiteSpace.Normal
                }
            };
            Add(title);

            var questionLabel = new Label("Question");
            questionLabel.style.whiteSpace = WhiteSpace.Normal;
            Add(questionLabel);

            var questionField = new TextField { value = model.Question, multiline = true };
            questionField.RegisterValueChangedCallback(evt =>
            {
                model.Question = evt.newValue;
            });
            Add(questionField);

            var choicesLabel = new Label("Answers");
            choicesLabel.style.whiteSpace = WhiteSpace.Normal;
            Add(choicesLabel);

            var addChoiceButton = new Button(() =>
            {
                controller.AddChoiceToMultipleChoiceNode(model);
                _choicesList.Rebuild();
            })
            {
                text = "+ Add Answer"
            };
            Add(addChoiceButton);

            _choicesList = new ListView(model.Choices, itemHeight: 50, makeItem: () =>
            {
                var container = new VisualElement();
                container.style.flexDirection = FlexDirection.Row;
                container.style.justifyContent = Justify.SpaceBetween;
                container.style.alignItems = Align.Center;
                container.style.marginBottom = 5;

                var answerField = new TextField { style = { flexGrow = 1, marginRight = 5 } };
                answerField.multiline = false;

                var triggerButton = new Button(() => Debug.Log("[Trigger]")) { text = "Trigger" };
                var connectButton = new Button(() => Debug.Log("[Connect]")) { text = "Connect" };

                container.Add(answerField);
                container.Add(triggerButton);
                container.Add(connectButton);

                return container;
            },
            bindItem: (element, i) =>
            {
                var container = element as VisualElement;
                var choice = model.Choices[i];

                var answerField = container.ElementAt(0) as TextField;
                answerField.value = choice.AnswerText;
                answerField.RegisterValueChangedCallback(evt =>
                {
                    choice.AnswerText = evt.newValue;
                });
            });

            Add(_choicesList);
        }
    }
}
