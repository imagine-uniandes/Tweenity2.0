using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using Models;

namespace Views.MiddlePanel{
    
    public class TweenityNode : Node
    {
        public string NodeID { get; private set; }
        public string TitleText { get; private set; }
        public string DescriptionText { get; private set; }
        public NodeType NodeType { get; private set; }

        private TextField titleField;
        private TextField descriptionField;
        private DropdownField nodeTypeDropdown;

        public TweenityNode(string nodeID)
        {
            this.NodeID = nodeID;
            this.title = "New Node";

            // Set up UI Elements
            titleField = new TextField("Title") { value = "New Node" };
            descriptionField = new TextField("Description") { value = "" };
            nodeTypeDropdown = new DropdownField("Node Type", new System.Collections.Generic.List<string>(System.Enum.GetNames(typeof(NodeType))), 0);
            nodeTypeDropdown.value = NodeType.NoType.ToString();

            // Add elements to the node
            this.extensionContainer.Add(titleField);
            this.extensionContainer.Add(descriptionField);
            this.extensionContainer.Add(nodeTypeDropdown);
            
            // Refresh node UI
            this.RefreshExpandedState();
            this.RefreshPorts();
        }
    }
}
