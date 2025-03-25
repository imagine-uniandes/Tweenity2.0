using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using Models.Nodes;
using Controllers;

namespace Views.RightPanel
{
    public class StartView : VisualElement
    {
        private StartNodeModel _model;
        private GraphController _controller;

        public StartView(StartNodeModel model, GraphController controller)
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

            Label startLabel = new Label("Start Node");
            startLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            this.Add(startLabel);

            this.Add(new Label("This is a Start Node. It does not contain any editable properties."));
        }
    }
}
