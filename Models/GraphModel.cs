using System.Collections.Generic;
using System.Linq;
using Models.Nodes;

namespace Models
{
    public class GraphModel
    {
        public List<TweenityNodeModel> Nodes { get; private set; }

        public GraphModel()
        {
            Nodes = new List<TweenityNodeModel>();
        }

        public bool AddNode(TweenityNodeModel node)
        {
            if (node.Type == NodeType.Start && Nodes.Any(n => n.Type == NodeType.Start))
            {
                return false;
            }

            Nodes.Add(node);
            return true;
        }

        public void RemoveNode(string nodeId)
        {
            Nodes.RemoveAll(n => n.NodeID == nodeId);

            foreach (var node in Nodes)
            {
                node.DisconnectFrom(nodeId);
            }
        }

        public TweenityNodeModel GetNode(string nodeId)
        {
            return Nodes.FirstOrDefault(n => n.NodeID == nodeId);
        }
        
        public void SetNodes(List<TweenityNodeModel> nodes)
        {
            Nodes.Clear();
            Nodes.AddRange(nodes);
        }

    }
}
