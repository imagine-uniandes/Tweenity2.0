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
            Add(new Label("Random Node Details") { style = { unityFontStyleAndWeight = FontStyle.Bold } });

            var typedModel = (RandomNodeModel)_model;

            Add(new Label("Possible Paths"));

            Button addPathButton = new Button(() =>
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
