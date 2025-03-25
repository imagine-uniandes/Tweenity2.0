using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using Models.Nodes;
using Controllers;

namespace Views.RightPanel
{
    public class RandomView : VisualElement
    {
        private RandomNodeModel _model;
        private GraphController _controller;
        private ListView _pathsList;

        public RandomView(RandomNodeModel model, GraphController controller)
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

            Label header = new Label("Random Node");
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            this.Add(header);

            this.Add(new Label("Possible Paths"));

            Button addPathButton = new Button(() =>
            {
                _controller.AddRandomPath(_model);
                _pathsList.Rebuild();
            })
            {
                text = "+ Add Path"
            };
            this.Add(addPathButton);

            _pathsList = new ListView(_model.PossiblePaths, 20, () => new Label(), (e, i) =>
            {
                (e as Label).text = _model.PossiblePaths[i];
            });

            this.Add(_pathsList);
        }
    }
}
