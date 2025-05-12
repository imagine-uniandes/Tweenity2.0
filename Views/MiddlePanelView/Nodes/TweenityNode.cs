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
                NodeType.Start           => new Color32(129, 199, 132, 255), // Soft green
                NodeType.End             => new Color32(239, 83, 80, 255),   // Vivid red
                NodeType.Dialogue        => new Color32(186, 104, 200, 255), // Soft purple
                NodeType.MultipleChoice => new Color32(255, 179, 0, 255),    // Bold yellow-orange
                NodeType.Reminder        => new Color32(100, 181, 246, 255), // Bright neutral blue
                NodeType.Random          => new Color32(255, 213, 79, 255),  // Amber yellow
                NodeType.Timeout         => new Color32(144, 202, 249, 255), // Light blue / cyan
                _                        => new Color32(51, 51, 51, 255)      // Neutral fallback
            };

            this.mainContainer.style.backgroundColor = color;
        }

        private void SetTextColorForType(NodeType type)
        {
            Color textColor = type switch
            {
                NodeType.Start           => new Color32(27, 94, 32, 255),     // Deep green
                NodeType.End             => new Color32(255, 255, 255, 255),  // White
                NodeType.Dialogue        => new Color32(250, 250, 250, 255),  // Light gray
                NodeType.MultipleChoice => new Color32(66, 40, 0, 255),       // Brown
                NodeType.Reminder        => new Color32(13, 71, 161, 255),    // Indigo/dark blue
                NodeType.Random          => new Color32(66, 40, 0, 255),       // Brown
                NodeType.Timeout         => new Color32(30, 60, 80, 255),     // Slate blue
                _                        => new Color(0.85f, 0.85f, 0.85f)     // Default light gray
            };

            _typeLabel.style.color = textColor;
            _descriptionLabel.style.color = textColor;
        }
    }
}