using System.Collections.Generic;
using Models;
using Models.Nodes;
using System.Linq;

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
                    NodeID = node.NodeID,
                    Type = node.Type,
                    responses = new List<Response>(),
                    userActions = new List<Action>(),
                    simulatorActions = new List<Action>()
                };

                // --- Convert OutgoingPaths to Responses & UserActions
                foreach (var path in node.OutgoingPaths)
                {
                    var responseID = System.Guid.NewGuid().ToString();

                    runtimeNode.responses.Add(new Response
                    {
                        ResponseID = responseID,
                        DestinationNodeID = path.TargetNodeID
                    });

                    // Convert Trigger (Object:Script.Method) to userAction
                    if (!string.IsNullOrEmpty(path.Trigger) && path.Trigger.Contains(":"))
                    {
                        var parts = path.Trigger.Split(':');
                        if (parts.Length == 2)
                        {
                            var methodParts = parts[1].Split('.');
                            if (methodParts.Length == 2)
                            {
                                runtimeNode.userActions.Add(new Action
                                {
                                    ObjectAction = parts[0],
                                    ActionName = $"{methodParts[0]}.{methodParts[1]}",
                                    ActionParams = "", // You can support later
                                    ResponseID = responseID
                                });
                            }
                        }
                    }
                }

                // --- Convert Instruction objects to simulatorActions
                if (node.Instructions != null)
                {
                    foreach (var instr in node.Instructions)
                    {
                        runtimeNode.simulatorActions.Add(new Action
                        {
                            ObjectAction = instr.ObjectName,
                            ActionName = instr.MethodName,
                            ActionParams = instr.Params,
                            ResponseID = null
                        });
                    }
                }

                runtimeScript.AddNode(runtimeNode);
            }

            return runtimeScript;
        }
    }
}
