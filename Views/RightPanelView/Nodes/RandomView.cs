using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;

namespace Views.RightPanel{

    public class RandomView : VisualElement
    {
        public RandomView()
        {
            this.style.paddingLeft = 5;
            this.style.paddingRight = 5;
            this.style.paddingTop = 5;
            this.style.paddingBottom = 5;
            this.style.borderTopLeftRadius = 5;
            this.style.borderTopRightRadius = 5;
            this.style.borderBottomLeftRadius = 5;
            this.style.borderBottomRightRadius = 5;
            
            Label randomLabel = new Label("Random Node");
            randomLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            this.Add(randomLabel);

            this.Add(new Label("Possible Paths"));
            Button addPathButton = new Button(() => AddRandomPath()) { text = "+ Add Path" };
            this.Add(addPathButton);
            
            ListView pathsList = new ListView();
            this.Add(pathsList);
        }

        private void AddRandomPath()
        {
            Debug.Log("Add new random path");
            // Logic to dynamically add random paths
        }
    }
}
