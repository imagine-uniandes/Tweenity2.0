using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace Nodes
{
    public class StartNode : TweenityNode
    {
        public StartNode(string nodeID) : base(nodeID)
        {
            this.title = "Start Node";
        }
    }
}
