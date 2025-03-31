using UnityEngine.UIElements;
using UnityEngine;
using Models.Nodes;
using Controllers;

namespace Views.RightPanel
{
    public class StartView : TweenityNodeView
    {
        public StartView(StartNodeModel model, GraphController controller) : base(model, controller)
        {
            Add(new Label("Start Node Details") { style = { unityFontStyleAndWeight = FontStyle.Bold } });

            Label note = new Label("This is a Start Node. It does not contain any editable properties.");
            note.style.unityFontStyleAndWeight = FontStyle.Italic;
            note.style.marginTop = 10;

            Add(note);
        }
    }
}
