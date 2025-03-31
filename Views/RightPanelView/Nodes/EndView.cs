using UnityEngine.UIElements;
using UnityEngine;
using Models.Nodes;
using Controllers;

namespace Views.RightPanel
{
    public class EndView : TweenityNodeView
    {
        public EndView(EndNodeModel model, GraphController controller) : base(model, controller)
        {
            Add(new Label("End Node Details") { style = { unityFontStyleAndWeight = FontStyle.Bold } });

            Label note = new Label("This is an End Node. No editable properties.");
            note.style.unityFontStyleAndWeight = FontStyle.Italic;
            note.style.marginTop = 10;

            Add(note);
        }
    }
}
