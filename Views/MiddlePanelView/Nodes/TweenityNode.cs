using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using Models;
using Models.Nodes;

namespace Views.MiddlePanel
{
    public class TweenityNode : Node
    {
        public string NodeID { get; private set; }
        public TweenityNodeModel NodeModel { get; set; }

        private Label _typeLabel;
        private Label _descriptionLabel;

        public TweenityNode(string nodeID)
        {
            this.NodeID = nodeID;
            this.title = "New Node"; // visible en encabezado gris

            _typeLabel = new Label("Type: NoType")
            {
                style = { unityFontStyleAndWeight = FontStyle.Italic, marginTop = 4, marginLeft = 5 }
            };

            _descriptionLabel = new Label("(No description)")
            {
                style = { whiteSpace = WhiteSpace.Normal, marginLeft = 5, marginTop = 2 }
            };

            this.extensionContainer.Add(_typeLabel);
            this.extensionContainer.Add(_descriptionLabel);

            this.RefreshExpandedState();
            this.RefreshPorts();
        }

        public void UpdateFromModel()
        {
            if (NodeModel == null) return;

            this.title = NodeModel.Title;
            _typeLabel.text = $"Type: {NodeModel.Type}";
            _descriptionLabel.text = string.IsNullOrWhiteSpace(NodeModel.Description)
                ? "(No description)"
                : NodeModel.Description;
        }
    }
}
