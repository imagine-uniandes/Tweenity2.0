using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;


namespace Views.RightPanel{
    
    public class NoTypeView : VisualElement
    {
        public NoTypeView()
        {
            this.style.paddingLeft = 5;
            this.style.paddingRight = 5;
            this.style.paddingTop = 5;
            this.style.paddingBottom = 5;
            this.style.borderTopLeftRadius = 5;
            this.style.borderTopRightRadius = 5;
            this.style.borderBottomLeftRadius = 5;
            this.style.borderBottomRightRadius = 5;
            
            Label noTypeLabel = new Label("Basic Node");
            noTypeLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            this.Add(noTypeLabel);

            this.Add(new Label("Title"));
            TextField titleField = new TextField();
            this.Add(titleField);

            this.Add(new Label("Description"));
            TextField descriptionField = new TextField();
            this.Add(descriptionField);

            this.Add(new Label("Notes"));
            TextField notesField = new TextField();
            this.Add(notesField);
        }
    }
}
