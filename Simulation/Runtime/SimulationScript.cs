using System.Collections.Generic;
using System.Linq;
using Models;

namespace Simulation.Runtime
{
    public class SimulationScript
    {
        // All nodes indexed by their unique NodeID
        private Dictionary<string, Node> _nodes = new();

        public SimulationScript() { }

        /// <summary>
        /// Returns the first node of type Start.
        /// </summary>
        public Node GetStartNode()
        {
            return _nodes.Values.FirstOrDefault(n => n.Type == NodeType.Start);
        }

        /// <summary>
        /// Gets a node by its unique ID.
        /// </summary>
        public Node GetNode(string nodeId)
        {
            _nodes.TryGetValue(nodeId, out var node);
            return node;
        }

        /// <summary>
        /// Returns all nodes in the simulation.
        /// </summary>
        public IEnumerable<Node> GetAllNodes() => _nodes.Values;

        /// <summary>
        /// Adds or replaces a node in the graph.
        /// </summary>
        public void AddNode(Node node)
        {
            if (!string.IsNullOrEmpty(node.NodeID))
                _nodes[node.NodeID] = node;
        }

        /// <summary>
        /// Checks whether a node exists by ID.
        /// </summary>
        public bool HasNode(string nodeId) => _nodes.ContainsKey(nodeId);
    }
}
