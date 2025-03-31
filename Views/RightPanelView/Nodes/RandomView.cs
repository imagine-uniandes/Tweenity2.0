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
            var title = new Label("Random Node Details");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.whiteSpace = WhiteSpace.Normal;
            Add(title);

            var typedModel = (RandomNodeModel)_model;

            var pathsLabel = new Label("Possible Paths");
            pathsLabel.style.whiteSpace = WhiteSpace.Normal;
            Add(pathsLabel);

            var addPathButton = new Button(() =>
            {
                _controller.AddRandomPath(typedModel);
                _pathsList.Rebuild();
            })
            {
                text = "+ Add Path"
            };
            Add(addPathButton);

            _pathsList = new ListView(typedModel.PossiblePaths, 20, () => new Label(), (e, i) =>
            {
                (e as Label).text = typedModel.PossiblePaths[i];
            });

            Add(_pathsList);
        }
    }
}
