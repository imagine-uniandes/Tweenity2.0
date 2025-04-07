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
                NodeType.Start => new Color(0.3f, 0.85f, 0.3f),         // Green
                NodeType.End => new Color(0.85f, 0.3f, 0.3f),           // Red
                NodeType.Dialogue => new Color(0.3f, 0.6f, 0.85f),      // Light Blue
                NodeType.MultipleChoice => new Color(1.0f, 0.6f, 0.2f), // Orange
                NodeType.Reminder => new Color(1.0f, 0.85f, 0.3f),      // Yellow
                NodeType.Random => new Color(0.7f, 0.4f, 1.0f),         // Purple
                NodeType.Timeout => new Color(0.75f, 0.75f, 0.75f),     // Light Gray
                _ => new Color(0.2f, 0.2f, 0.2f)                        // Fallback: Dark Gray
            };

            this.mainContainer.style.backgroundColor = color;
        }
        private void SetTextColorForType(NodeType type)
        {
            // Use dark text for light backgrounds
            bool useBlackText = type == NodeType.Start 
                            || type == NodeType.Reminder 
                            || type == NodeType.Timeout;

            Color textColor = useBlackText 
                ? Color.black 
                : new Color(0.85f, 0.85f, 0.85f); // Light gray default

            _typeLabel.style.color = textColor;
            _descriptionLabel.style.color = textColor;
        }
    }
}
