using System.Collections.Generic;
using System.Linq;
using Models;
using Models.Nodes;
using Simulation.Runtime;

namespace Simulation.Runtime
{
    public static class RuntimeGraphBuilder
    {
        public static SimulationScript FromGraphModel(GraphModel graphModel)
        {
            var runtimeScript = new SimulationScript();

            foreach (var node in graphModel.Nodes)
            {
                var runtimeNode = new Node
                {
                    title = node.Title,
                    text = node.Description ?? "",
                    tags = new List<string> { node.Type.ToString().ToLower() },
                    userActions = new List<Action>(),
                    simulatorActions = new List<Action>(),
                    responses = new List<Response>()
                };

                foreach (var path in node.OutgoingPaths)
                {
                    runtimeNode.responses.Add(new Response
                    {
                        displayText = path.Label ?? "Continue",
                        destinationNode = GetNodeTitleById(graphModel, path.TargetNodeID)
                    });

                    if (!string.IsNullOrEmpty(path.Trigger) && path.Trigger.Contains(":"))
                    {
                        var parts = path.Trigger.Split(':');
                        if (parts.Length == 2)
                        {
                            runtimeNode.userActions.Add(new Action
                            {
                                object2Action = parts[0],
                                actionName = parts[1],
                                actionParams = ""
                            });
                        }
                    }
                }

                runtimeScript.AddNode(runtimeNode);
            }

            return runtimeScript;
        }

        private static string GetNodeTitleById(GraphModel graph, string nodeId)
        {
            return graph.Nodes.FirstOrDefault(n => n.NodeID == nodeId)?.Title ?? "(Missing)";
        }
    }
}
