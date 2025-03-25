using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;

namespace Views.MiddlePanel{
    public class MultipleChoiceNode : TweenityNode
    {
        private ListView choicesList;
        private List<string> choices;

        public MultipleChoiceNode(string nodeID) : base(nodeID)
        {
            this.title = "Multiple Choice Node";
            choices = new List<string>();
            
            // Add choices list UI
            Button addChoiceButton = new Button(() => AddChoice()) { text = "+ Add Choice" };
            this.extensionContainer.Add(addChoiceButton);
            
            choicesList = new ListView(choices, itemHeight: 20, () => new Label(), (element, i) => 
            {
                (element as Label).text = choices[i] + " (Trigger)";
            });
            
            this.extensionContainer.Add(choicesList);
            this.RefreshExpandedState();
            this.RefreshPorts();
        }

        private void AddChoice()
        {
            string newChoice = "Choice " + (choices.Count + 1);
            choices.Add(newChoice);
            choicesList.Rebuild();
        }
    }
}

