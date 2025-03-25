using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using Models.Nodes;
using Controllers;

namespace Views.RightPanel
{
    public class EndView : VisualElement
    {
        private EndNodeModel _model;
        private GraphController _controller;

        public EndView(EndNodeModel model, GraphController controller)
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

            Label endLabel = new Label("End Node");
            endLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            this.Add(endLabel);

            this.Add(new Label("This is an End Node. No editable properties."));
        }
    }
}
