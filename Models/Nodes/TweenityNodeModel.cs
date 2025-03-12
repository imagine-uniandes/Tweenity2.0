using System;
using System.Collections.Generic;

namespace Models.Nodes
{
    public class TweenityNodeModel
    {
        public string NodeID { get; private set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public NodeType Type { get; set; }
        public List<string> ConnectedNodes { get; set; }

        public TweenityNodeModel(string title, NodeType type)
        {
            NodeID = Guid.NewGuid().ToString(); // Unique ID for each node
            Title = title;
            Type = type;
            Description = "";
            ConnectedNodes = new List<string>();
        }
    }
}
