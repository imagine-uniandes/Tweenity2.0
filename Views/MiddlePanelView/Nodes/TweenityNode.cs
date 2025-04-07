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

        public Port InputPort { get; private set; }
        public Port OutputPort { get; private set; }

        public TweenityNode(string nodeID)
        {
            this.NodeID = nodeID;
            this.title = "New Node";

            // Info labels
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

            CreateMinimalConnectionPorts();

            this.RefreshExpandedState();
            this.RefreshPorts();
        }

        private void CreateMinimalConnectionPorts()
        {
            InputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            InputPort.name = "InPort";
            InputPort.visible = false;
            InputPort.style.width = 8;
            InputPort.style.height = 8;
            InputPort.style.backgroundColor = new Color(0, 0, 0, 0);
            InputPort.portName = "";

            OutputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
            OutputPort.name = "OutPort";
            OutputPort.visible = false;
            OutputPort.style.width = 8;
            OutputPort.style.height = 8;
            OutputPort.style.backgroundColor = new Color(0, 0, 0, 0);
            OutputPort.portName = "";

            inputContainer.Add(InputPort);
            outputContainer.Add(OutputPort);
        }
        public void UpdateFromModel()
        {
            if (NodeModel == null) return;

            this.title = NodeModel.Title;
            _typeLabel.text = $"Type: {NodeModel.Type}";
            _descriptionLabel.text = string.IsNullOrWhiteSpace(NodeModel.Description)
                ? "(No description)"
                : NodeModel.Description;

            SetBackgroundColorForType(NodeModel.Type);
            SetTextColorForType(NodeModel.Type);
        }

        private void SetBackgroundColorForType(NodeType type)
        {
            Color color = type switch
            {
                NodeType.Start           => new Color32(165, 214, 167, 255), // Soft green
                NodeType.End             => new Color32(217, 77, 77, 255),   // Red
                NodeType.Dialogue        => new Color32(77, 153, 217, 255),  // Light blue
                NodeType.MultipleChoice => new Color32(255, 204, 128, 255), // Peach orange
                NodeType.Reminder        => new Color32(255, 249, 196, 255), // Pastel yellow
                NodeType.Random          => new Color32(206, 147, 216, 255), // Soft purple
                NodeType.Timeout         => new Color32(207, 216, 220, 255), // Light blue gray
                _                        => new Color32(51, 51, 51, 255)      // Dark gray fallback
            };

            this.mainContainer.style.backgroundColor = color;
        }

        private void SetTextColorForType(NodeType type)
        {
            Color textColor = type switch
            {
                NodeType.Start           => new Color32(46, 125, 50, 255),   // Forest green
                NodeType.Reminder        => new Color32(121, 85, 72, 255),   // Warm brown
                NodeType.MultipleChoice => new Color32(93, 64, 55, 255),     // Chocolate
                NodeType.Timeout         => new Color32(55, 71, 79, 255),    // Slate blue
                NodeType.End             => new Color(0.98f, 0.98f, 0.98f),   // Light gray
                NodeType.Dialogue        => new Color(0.98f, 0.98f, 0.98f),   // Light gray
                NodeType.Random          => new Color(0.98f, 0.98f, 0.98f),   // Light gray
                _                        => new Color(0.8f, 0.8f, 0.8f)       // Default light gray
            };

            _typeLabel.style.color = textColor;
            _descriptionLabel.style.color = textColor;
        }

    }
}
