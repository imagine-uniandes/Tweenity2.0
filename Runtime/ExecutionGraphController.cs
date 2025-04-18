using System.Collections.Generic;
using System.Linq;
using Models;
using Models.Nodes;

namespace Simulation.Runtime
{
    public class ExecutionGraphController
    {
        private readonly GraphModel graph;
        private readonly Dictionary<string, List<string>> adjacency;

        public ExecutionGraphController(GraphModel graph)
        {
            this.graph = graph;
            adjacency = new Dictionary<string, List<string>>();
            BuildExecutionGraph();
        }

        private void BuildExecutionGraph()
        {
            foreach (var node in graph.Nodes)
            {
                if (!adjacency.ContainsKey(node.NodeID))
                    adjacency[node.NodeID] = new List<string>();

                foreach (var path in node.OutgoingPaths)
                {
                    if (!string.IsNullOrEmpty(path.TargetNodeID))
                        adjacency[node.NodeID].Add(path.TargetNodeID);
                }
            }
        }

        public TweenityNodeModel GetClosestNodeByTrigger(string fromNodeId, string trigger)
        {
            var visited = new HashSet<string>();
            var queue = new Queue<string>();
            queue.Enqueue(fromNodeId);

            while (queue.Count > 0)
            {
                var currentId = queue.Dequeue();
                if (visited.Contains(currentId)) continue;
                visited.Add(currentId);

                var node = graph.GetNode(currentId);
                if (node == null) continue;

                bool hasAwaitTrigger = node.RuntimeInstructions.Any(instr =>
                    instr.Trim().Equals("AwaitTrigger()", System.StringComparison.OrdinalIgnoreCase));

                // ✅ Caso base: si estamos empezando y ya estamos en un nodo con AwaitTrigger()
                if (currentId == fromNodeId && hasAwaitTrigger)
                {
                    foreach (var path in node.OutgoingPaths)
                    {
                        if (path.Trigger == trigger)
                        {
                            return node;
                        }
                    }

                    // Si está esperando trigger pero no matchea → no seguir
                    return null;
                }

                // ✅ Caso general: si encontramos nodo con AwaitTrigger()
                if (hasAwaitTrigger)
                {
                    foreach (var path in node.OutgoingPaths)
                    {
                        if (path.Trigger == trigger)
                        {
                            return node;
                        }
                    }

                    // Nodo con AwaitTrigger pero no matchea → no seguir desde aquí
                    continue;
                }

                // Si no es nodo de espera, expandimos vecinos
                if (adjacency.TryGetValue(currentId, out var neighbors))
                {
                    foreach (var neighborId in neighbors)
                    {
                        if (!visited.Contains(neighborId))
                            queue.Enqueue(neighborId);
                    }
                }
            }

            // No se encontró nodo con trigger coincidente
            return null;
        }
    }
}
