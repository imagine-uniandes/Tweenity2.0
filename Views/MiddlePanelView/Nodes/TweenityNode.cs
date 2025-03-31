using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using Models;
using Models.Nodes;

namespace Views.MiddlePanel{

    public class TweenityNode : Node
    {
        public string NodeID { get; private set; }
        public string TitleText { get; private set; }
        public string DescriptionText { get; private set; }
        public NodeType NodeType { get; private set; }

        public TweenityNodeModel NodeModel { get; set; }

        private TextField titleField;
        private TextField descriptionField;

        public TweenityNode(string nodeID)
        {
            this.NodeID = nodeID;
            this.title = "New Node";

            // Set up UI Elements
            titleField = new TextField("Title") { value = "New Node" };
            descriptionField = new TextField("Description") { value = "" };
            Label typeLabel = new Label("Type: NoType");
            typeLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
            typeLabel.style.marginTop = 4;
            typeLabel.style.marginBottom = 4;
            typeLabel.style.marginLeft = 5;

            // Add elements to the node
            this.extensionContainer.Add(titleField);
            this.extensionContainer.Add(descriptionField);
            this.extensionContainer.Add(typeLabel);

            // Refresh node UI
            this.RefreshExpandedState();
            this.RefreshPorts();
        }
    }
}
