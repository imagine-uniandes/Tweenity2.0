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
            Add(new Label("Multiple Choice Node Details") { style = { unityFontStyleAndWeight = FontStyle.Bold } });

            var choiceModel = (MultipleChoiceNodeModel)_model;

            Add(new Label("Choices"));

            Button addChoiceButton = new Button(() =>
            {
                _controller.AddChoiceToMultipleChoiceNode(choiceModel);
                _choicesList.Rebuild();
            })
            {
                text = "+ Add Choice"
            };
            Add(addChoiceButton);

            _choicesList = new ListView(choiceModel.Choices, 20, () => new Label(), (e, i) =>
            {
                (e as Label).text = choiceModel.Choices[i];
            });

            Add(_choicesList);
        }
    }
}
