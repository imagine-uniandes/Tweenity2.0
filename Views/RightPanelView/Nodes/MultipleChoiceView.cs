using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;


namespace Views.RightPanel{

    public class MultipleChoiceView : VisualElement
    {
        public MultipleChoiceView()
        {
            this.style.paddingLeft = 5;
            this.style.paddingRight = 5;
            this.style.paddingTop = 5;
            this.style.paddingBottom = 5;
            this.style.borderTopLeftRadius = 5;
            this.style.borderTopRightRadius = 5;
            this.style.borderBottomLeftRadius = 5;
            this.style.borderBottomRightRadius = 5;
            
            Label multipleChoiceLabel = new Label("Multiple Choice Node");
            multipleChoiceLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            this.Add(multipleChoiceLabel);

            // Response paths list
            this.Add(new Label("Response Paths"));
            Button addResponseButton = new Button(() => AddResponsePath()) { text = "+ Add Choice" };
            this.Add(addResponseButton);
            
            ListView responsePathsList = new ListView();
            this.Add(responsePathsList);
        }

        private void AddResponsePath()
        {
            Debug.Log("Add new multiple choice path");
            // Logic to dynamically add multiple choice paths
        }
    }
}
