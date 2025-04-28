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

                // --- Parse outgoing paths (Success + Reminder paths)
                foreach (var path in node.OutgoingPaths)
                {
                    runtimeNode.responses.Add(new Response
                    {
                        displayText = path.Trigger ?? "Continue",
                        destinationNode = GetNodeTitleById(graphModel, path.TargetNodeID)
                    });

                    // Only create UserActions for Success paths (which have a target)
                    if (!string.IsNullOrEmpty(path.TargetNodeID) && !string.IsNullOrEmpty(path.Trigger) && path.Trigger.Contains(":"))
                    {
                        var parts = path.Trigger.Split(':');
                        if (parts.Length == 2)
                        {
                            var methodParts = parts[1].Split('.');
                            if (methodParts.Length == 2)
                            {
                                runtimeNode.userActions.Add(new Action
                                {
                                    object2Action = parts[0],
                                    actionName = $"{methodParts[0]}.{methodParts[1]}",
                                    actionParams = ""
                                });
                            }
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
