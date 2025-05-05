using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Simulation.Runtime;
using System.Linq;

#if UNITY_EDITOR
using Models;
using Tweenity2.EditorHelpers;
#endif

namespace Simulation.Runtime
{
    public class SimulationController
    {
        public bool debugCarga = false;
        public bool debugLectura = false;

        private SimulationScript currSim;
        private Node curNode;
        private Action curReminder;
        private Action curExpectedUserAction;
        private List<Action> curSimulatorActions;

        public UnityEvent<Node> onEnteredNode = new();

        private bool remember = true;
        private bool timeout = true;

        private CancellationTokenSource tokenSource = new();

        public Node GetCurrentNode() => curNode;

        private void PrintOnDebug(string msg)
        {
            if (debugLectura) Debug.Log(msg);
        }

        public void SetSimulation(SimulationScript simulation)
        {
            currSim = simulation;
            curNode = currSim.GetStartNode();

            if (curNode == null)
            {
                Debug.LogError("❌ Simulation start node not found.");
                return;
            }

            EnterNode(curNode);
        }

#if UNITY_EDITOR
        public void SetSimulationFromGraph(GraphModel model)
        {
            var runtimeGraph = RuntimeGraphBuilder.FromGraphModel(model);
            SetSimulation(runtimeGraph);
        }
#endif

        public List<Response> GetCurrentResponses() => curNode.responses;

        public void ChooseResponse(int index)
        {
            if (index < 0 || index >= curNode.responses.Count) return;

            string nextId = curNode.responses[index].DestinationNodeID;
            var nextNode = currSim.GetNode(nextId);
            if (nextNode == null)
            {
                Debug.LogError($"Node with ID {nextId} not found.");
                return;
            }

            EnterNode(nextNode);
        }

        private async void EnterNode(Node node)
        {
            tokenSource?.Cancel();
            tokenSource = new CancellationTokenSource();

            curNode = node;
            curExpectedUserAction = null;
            curSimulatorActions = node.simulatorActions;

            PrintOnDebug($"Entering node: {node.NodeID} [{node.Type}]");

#if UNITY_EDITOR
            EditorGraphHelper.TryCenterNode(node.NodeID);
#endif

            onEnteredNode?.Invoke(node);

            if (node.simulatorActions.Any())
            {
                await ExecuteSimulatorActions(node.simulatorActions);
            }

            if (node.userActions.Any())
            {
                curExpectedUserAction = node.userActions.First();
            }
            else if (node.responses.Count == 1)
            {
                ChooseResponse(0);
            }
        }

        public async Task<MethodInfo> ExecuteSimulatorActions(List<Action> actions)
        {
            MethodInfo taskResult = null;

            foreach (var act in actions)
            {
                if (string.IsNullOrEmpty(act.ObjectAction) || string.IsNullOrEmpty(act.ActionName))
                    continue;

                var obj = GameObject.Find(act.ObjectAction);
                if (obj == null)
                {
                    Debug.LogWarning($"Object not found: {act.ObjectAction}");
                    continue;
                }

                foreach (var script in obj.GetComponents<MonoBehaviour>())
                {
                    if (script == null) continue;
                    var method = script.GetType().GetMethod(act.ActionName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (method != null && method.GetParameters().Length == 0)
                    {
                        method.Invoke(script, null);
                        taskResult = method;
                        break;
                    }
                }
            }

            await Task.Yield();
            return taskResult;
        }

        public void VerifyUserAction(Action action)
        {
            if (action == null || curNode == null) return;

            var matching = curNode.userActions.FirstOrDefault(a =>
                a.ObjectAction == action.ObjectAction && a.ActionName == action.ActionName);

            if (matching != null)
            {
                PrintOnDebug("User action verified.");

                if (!string.IsNullOrEmpty(matching.ResponseID))
                {
                    var response = curNode.GetResponseByID(matching.ResponseID);
                    if (response != null)
                    {
                        int index = curNode.responses.IndexOf(response);
                        ChooseResponse(index);
                    }
                }
            }
            else
            {
                PrintOnDebug("User action invalid.");
            }
        }
    }
}
