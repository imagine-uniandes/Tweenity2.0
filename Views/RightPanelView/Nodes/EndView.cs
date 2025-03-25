using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;


namespace Views.RightPanel{
    
    public class EndView : VisualElement
    {
        public EndView()
        {
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

            // Since End nodes have no special characteristics, just display a label
            this.Add(new Label("This is an End Node."));
        }
    }
}
