using System.Collections.Generic;
using Models;
using Models.Nodes;
using System.Linq;
using UnityEngine;

namespace Simulation.Runtime
{
    public static class RuntimeGraphBuilder
    {
        public static SimulationScript FromGraphModel(GraphModel graphModel)
        {
            var runtimeScript = new SimulationScript();

            foreach (var model in graphModel.Nodes)
            {
                Node runtimeNode = model.Type switch
                {
                    NodeType.Timeout        => BuildTimeoutNode((TimeoutNodeModel)model),
                    NodeType.Reminder       => BuildReminderNode((ReminderNodeModel)model),
                    NodeType.MultipleChoice => BuildMultiPathNode(model),
                    NodeType.Dialogue       => BuildMultiPathNode(model),
                    NodeType.Random         => BuildMultiPathNoTriggerNode(model),
                    NodeType.NoType         => BuildSimpleNode(model),
                    NodeType.Start          => BuildStartNode(model),
                    NodeType.End            => BuildEndNode(model),
                    _                       => BuildSimpleNode(model)
                };

                runtimeScript.AddNode(runtimeNode);
            }

            Debug.Log($"✅ Runtime graph built with {runtimeScript.GetAllNodes().Count()} nodes.");
            return runtimeScript;
        }

        private static Node BuildTimeoutNode(TimeoutNodeModel model)
        {
            Debug.Log($"[TimeoutNode] ID: {model.NodeID}, Timeout: {model.TimeoutDuration}, Condition: {model.Condition}");

            var node = CreateBaseNode(model);

            for (int i = 0; i < model.OutgoingPaths.Count; i++)
            {
                var path = model.OutgoingPaths[i];
                var responseID = System.Guid.NewGuid().ToString();

                node.responses.Add(new Response { ResponseID = responseID, DestinationNodeID = path.TargetNodeID });
                Debug.Log($"  Path: {path.Label} → {path.TargetNodeID} (Trigger: {path.Trigger})");

                if (i == 1 && !string.IsNullOrEmpty(path.Trigger))
                {
                    TryParseTrigger(path.Trigger, out var obj, out var method);
                    node.userActions.Add(new Action { ObjectAction = obj, ActionName = method, ResponseID = responseID });
                }
            }

            return node;
        }

        private static Node BuildReminderNode(ReminderNodeModel model)
        {
            Debug.Log($"[ReminderNode] ID: {model.NodeID}, Timer: {model.ReminderTimer}");

            var node = CreateBaseNode(model);

            for (int i = 0; i < model.OutgoingPaths.Count; i++)
            {
                var path = model.OutgoingPaths[i];
                var responseID = System.Guid.NewGuid().ToString();

                node.responses.Add(new Response { ResponseID = responseID, DestinationNodeID = path.TargetNodeID });
                Debug.Log($"  Path: {path.Label} → {path.TargetNodeID} (Trigger: {path.Trigger})");

                if (!string.IsNullOrEmpty(path.Trigger))
                {
                    TryParseTrigger(path.Trigger, out var obj, out var method);
                    node.userActions.Add(new Action { ObjectAction = obj, ActionName = method, ActionParams = model.ReminderTimer.ToString(), ResponseID = responseID });
                }
            }

            return node;
        }

        private static Node BuildMultiPathNode(TweenityNodeModel model)
        {
            var node = CreateBaseNode(model);

            foreach (var path in model.OutgoingPaths)
            {
                var responseID = System.Guid.NewGuid().ToString();
                node.responses.Add(new Response { ResponseID = responseID, DestinationNodeID = path.TargetNodeID });

                if (!string.IsNullOrEmpty(path.Trigger))
                {
                    TryParseTrigger(path.Trigger, out var obj, out var method);
                    node.userActions.Add(new Action { ObjectAction = obj, ActionName = method, ResponseID = responseID });
                }
            }

            return node;
        }

        private static Node BuildMultiPathNoTriggerNode(TweenityNodeModel model)
        {
            var node = CreateBaseNode(model);

            foreach (var path in model.OutgoingPaths)
            {
                node.responses.Add(new Response
                {
                    ResponseID = System.Guid.NewGuid().ToString(),
                    DestinationNodeID = path.TargetNodeID
                });
            }

            return node;
        }

        private static Node BuildSimpleNode(TweenityNodeModel model)
        {
            var node = CreateBaseNode(model);

            if (model.OutgoingPaths.Count > 0)
            {
                node.responses.Add(new Response
                {
                    ResponseID = System.Guid.NewGuid().ToString(),
                    DestinationNodeID = model.OutgoingPaths[0].TargetNodeID
                });
            }

            return node;
        }

        private static Node BuildStartNode(TweenityNodeModel model)
        {
            var node = BuildSimpleNode(model);
            return node;
        }

        private static Node BuildEndNode(TweenityNodeModel model)
        {
            return CreateBaseNode(model);
        }

        private static Node CreateBaseNode(TweenityNodeModel model)
        {
            var node = new Node
            {
                NodeID = model.NodeID,
                Type = model.Type,
                responses = new List<Response>(),
                userActions = new List<Action>(),
                simulatorActions = new List<Action>()
            };

            if (model.Instructions != null)
            {
                foreach (var instr in model.Instructions)
                {
                    node.simulatorActions.Add(new Action
                    {
                        ObjectAction = instr.ObjectName,
                        ActionName = instr.MethodName,
                        ActionParams = instr.Params,
                        ResponseID = null
                    });
                }
            }

            return node;
        }

        private static void TryParseTrigger(string trigger, out string objectName, out string methodName)
        {
            objectName = "";
            methodName = "";

            var parts = trigger.Split(':');
            if (parts.Length == 2)
            {
                var methodParts = parts[1].Split('.');
                if (methodParts.Length == 2)
                {
                    objectName = parts[0];
                    methodName = $"{methodParts[0]}.{methodParts[1]}";
                }
            }
        }
    }
}
