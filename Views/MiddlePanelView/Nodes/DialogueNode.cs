using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;

namespace Views.MiddlePanel
{
    public class DialogueNode : TweenityNode
    {
        private ListView responsePathsList;
        private List<string> responsePaths;

        public DialogueNode(string nodeID) : base(nodeID)
        {
            this.title = "Dialogue Node";
            responsePaths = new List<string>();
            
            // Dialogue Text Field
            this.extensionContainer.Add(new Label("Dialogue Text"));
            TextField dialogueTextField = new TextField();
            this.extensionContainer.Add(dialogueTextField);

            // Response Paths List
            Button addResponseButton = new Button(() => AddResponsePath()) { text = "+ Add Response" };
            this.extensionContainer.Add(addResponseButton);
            
            responsePathsList = new ListView(responsePaths, itemHeight: 20, () => new Label(), (element, i) => 
            {
                (element as Label).text = responsePaths[i];
            });
            
            this.extensionContainer.Add(responsePathsList);
            this.RefreshExpandedState();
            this.RefreshPorts();
        }

        private void AddResponsePath()
        {
            string newPath = "Response " + (responsePaths.Count + 1);
            responsePaths.Add(newPath);
            responsePathsList.Rebuild();
        }
    }
}
