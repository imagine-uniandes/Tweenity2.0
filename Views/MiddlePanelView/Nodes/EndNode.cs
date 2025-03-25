using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace Views.MiddlePanel{

    public class EndNode : TweenityNode
    {
        public EndNode(string nodeID) : base(nodeID)
        {
            this.title = "End Node";
        }
    }
}
