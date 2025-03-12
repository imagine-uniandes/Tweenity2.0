using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;

namespace Nodes
{
    public class DialogueView : VisualElement
    {
        public DialogueView()
        {
            this.style.paddingLeft = 5;
            this.style.paddingRight = 5;
            this.style.paddingTop = 5;
            this.style.paddingBottom = 5;
            this.style.borderTopLeftRadius = 5;
            this.style.borderTopRightRadius = 5;
            this.style.borderBottomLeftRadius = 5;
            this.style.borderBottomRightRadius = 5;
            
            Label dialogueLabel = new Label("Dialogue Node Details");
            dialogueLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

            this.Add(dialogueLabel);

            // Dialogue text input
            this.Add(new Label("Dialogue Text"));
            TextField dialogueTextField = new TextField();
            this.Add(dialogueTextField);

            // Response paths list
            this.Add(new Label("Response Paths"));
            Button addResponseButton = new Button(() => AddResponsePath()) { text = "+ Add Response" };
            this.Add(addResponseButton);
            
            ListView responsePathsList = new ListView();
            this.Add(responsePathsList);
        }

        private void AddResponsePath()
        {
            Debug.Log("Add new response path");
            // Logic to dynamically add response paths
        }
    }
}
