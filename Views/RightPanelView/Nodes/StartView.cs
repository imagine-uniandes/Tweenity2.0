using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;


namespace Nodes
{
    public class StartView : VisualElement
    {
        public StartView()
        {
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

            // Start nodes have no special characteristics, just display a label
            this.Add(new Label("This is a Start Node."));
        }
    }
}