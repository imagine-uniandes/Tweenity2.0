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

        public void UpdateFromModel()
        {
            if (NodeModel == null) return;

            this.title = NodeModel.Title;
            _typeLabel.text = $"Type: {NodeModel.Type}";
            _descriptionLabel.text = string.IsNullOrWhiteSpace(NodeModel.Description)
                ? "(No description)"
                : NodeModel.Description;
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
    }
}
