namespace Simulation.Runtime
{
    public class SimulationScript
    {
        public Dictionary<string, Node> nodes = new();

        public Node GetStartNode()
        {
            return nodes.Values.FirstOrDefault(n => n.tags.Contains("start"));
        }

        public Node GetNode(string title)
        {
            nodes.TryGetValue(title, out var node);
            return node;
        }

        public void AddNode(Node node)
        {
            if (!string.IsNullOrEmpty(node.title))
                nodes[node.title] = node;
        }
    }
}
