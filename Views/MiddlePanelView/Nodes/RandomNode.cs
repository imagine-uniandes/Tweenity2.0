using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;

namespace Nodes
{
    public class RandomNode : TweenityNode
    {
        private ListView pathsList;
        private List<string> paths;

        public RandomNode(string nodeID) : base(nodeID)
        {
            this.title = "Random Node";
            paths = new List<string>();
            
            // Add paths list UI
            Button addPathButton = new Button(() => AddPath()) { text = "+ Add Path" };
            this.extensionContainer.Add(addPathButton);
            
            pathsList = new ListView(paths, itemHeight: 20, () => new Label(), (element, i) => 
            {
                (element as Label).text = paths[i];
            });
            
            this.extensionContainer.Add(pathsList);
            this.RefreshExpandedState();
            this.RefreshPorts();
        }

        private void AddPath()
        {
            string newPath = "Path " + (paths.Count + 1);
            paths.Add(newPath);
            pathsList.Rebuild();
        }
    }
}