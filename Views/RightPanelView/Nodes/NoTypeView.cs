using UnityEngine.UIElements;
using UnityEngine;
using Models.Nodes;
using Controllers;

namespace Views.RightPanel
{
    public class NoTypeView : TweenityNodeView
    {
        public NoTypeView(NoTypeNodeModel model, GraphController controller) : base(model, controller)
        {
            Add(new Label("Generic Node Details") { style = { unityFontStyleAndWeight = FontStyle.Bold } });

            Label label = new Label("This node has no specific properties.");
            label.style.unityFontStyleAndWeight = FontStyle.Italic;
            label.style.marginTop = 10;

            Add(label);
        }
    }
}
