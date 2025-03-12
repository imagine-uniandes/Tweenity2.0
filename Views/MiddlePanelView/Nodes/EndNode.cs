using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace Nodes
{
    public class EndNode : TweenityNode
    {
        public EndNode(string nodeID) : base(nodeID)
        {
            this.title = "End Node";
        }
    }
}
