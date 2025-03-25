using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using Models.Nodes;
using Controllers;

namespace Views.RightPanel
{
    public class MultipleChoiceView : VisualElement
    {
        private ListView _choicesList;
        private MultipleChoiceNodeModel _model;
        private GraphController _controller;

        public MultipleChoiceView(MultipleChoiceNodeModel model, GraphController controller)
        {
            _model = model;
            _controller = controller;

            this.style.paddingLeft = 5;
            this.style.paddingRight = 5;
            this.style.paddingTop = 5;
            this.style.paddingBottom = 5;
            this.style.borderTopLeftRadius = 5;
            this.style.borderTopRightRadius = 5;
            this.style.borderBottomLeftRadius = 5;
            this.style.borderBottomRightRadius = 5;

            Label title = new Label("Multiple Choice Node");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            this.Add(title);

            this.Add(new Label("Choices"));

            Button addChoiceButton = new Button(() =>
            {
                _controller.AddChoiceToMultipleChoiceNode(_model);
                _choicesList.Rebuild();
            })
            {
                text = "+ Add Choice"
            };
            this.Add(addChoiceButton);

            _choicesList = new ListView(_model.Choices, 20, () => new Label(), (e, i) =>
            {
                (e as Label).text = _model.Choices[i];
            });
            this.Add(_choicesList);
        }
    }
}
