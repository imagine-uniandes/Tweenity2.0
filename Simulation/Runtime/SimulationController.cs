using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

#if UNITY_EDITOR
using Models;
using Views;
using Controllers;
#endif

namespace Simulation.Runtime
{
    public class SimulationController
    {
        public bool debugCarga = false;
        public bool debugLectura = false;

        private SimulationScript currSim;
        private Node curNode;
        private Action curExpectedUserAction;
        private List<Action> curSimulatorActions;

        public UnityEvent<Node> onEnteredNode = new();

        private CancellationTokenSource tokenSource = new();

#if UNITY_EDITOR
        private TweenityGraphView graphView;
        public void SetGraphView(TweenityGraphView view)
        {
            graphView = view;
        }
#endif

        public Node GetCurrentNode() => curNode;

        private void PrintOnDebug(string msg)
        {
            if (debugLectura) Debug.Log("[Simulation] " + msg);
        }

        public void SetSimulation(SimulationScript simulation)
        {
            currSim = simulation;
            curNode = currSim.GetStartNode();

            if (curNode == null)
            {
                Debug.LogError("❌ Start node not found.");
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

            var nextId = curNode.responses[index].DestinationNodeID;
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
            curSimulatorActions = node.simulatorActions;
            curExpectedUserAction = null;

            PrintOnDebug($"➡ Entering node: {node.NodeID} [{node.Type}]");

#if UNITY_EDITOR
            graphView?.CenterOnNode(node.NodeID);
#endif

            onEnteredNode?.Invoke(node);

            if (node.simulatorActions.Any())
                await ExecuteSimulatorActions(node.simulatorActions);

            switch (node.Type)
            {
                case NodeType.Reminder:
                    if (node.userActions.Count >= 2)
                    {
                        var delay = node.simulatorActions.FirstOrDefault(a => !string.IsNullOrEmpty(a.ActionParams))?.ActionParams;
                        if (float.TryParse(delay, out float seconds))
                            _ = ReminderAfterDelay(seconds, tokenSource.Token);

                        curExpectedUserAction = node.userActions[1];
                    }
                    break;

                case NodeType.Timeout:
                    if (node.userActions.Count >= 2)
                    {
                        var delay = node.simulatorActions.FirstOrDefault(a => !string.IsNullOrEmpty(a.ActionParams))?.ActionParams;
                        if (float.TryParse(delay, out float seconds))
                            _ = TimeoutAfterDelay(seconds, tokenSource.Token);

                        curExpectedUserAction = node.userActions[1];
                    }
                    break;

                default:
                    curExpectedUserAction = node.userActions.FirstOrDefault();

                    if (!node.userActions.Any() && node.responses.Count == 1)
                        ChooseResponse(0);
                    break;
            }
        }

        private async Task ReminderAfterDelay(float seconds, CancellationToken token)
        {
            try
            {
                await Task.Delay((int)(seconds * 1000), token);
                ChooseResponse(1);
            }
            catch { }
        }

        private async Task TimeoutAfterDelay(float seconds, CancellationToken token)
        {
            try
            {
                await Task.Delay((int)(seconds * 1000), token);
                ChooseResponse(0);
            }
            catch { }
        }

        public async Task<MethodInfo> ExecuteSimulatorActions(List<Action> actions)
        {
            MethodInfo result = null;

            foreach (var act in actions)
            {
                if (string.IsNullOrEmpty(act.ObjectAction) || string.IsNullOrEmpty(act.ActionName)) continue;

                var obj = GameObject.Find(act.ObjectAction);
                if (obj == null)
                {
                    Debug.LogWarning($"🔍 Object not found: {act.ObjectAction}");
                    continue;
                }

                foreach (var script in obj.GetComponents<MonoBehaviour>())
                {
                    if (script == null) continue;

                    var method = script.GetType().GetMethod(act.ActionName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (method != null && method.GetParameters().Length == 0)
                    {
                        method.Invoke(script, null);
                        result = method;
                        break;
                    }
                }
            }

            await Task.Yield();
            return result;
        }

        public void VerifyUserAction(Action received)
        {
            if (received == null || curNode == null) return;

            var match = curNode.userActions.FirstOrDefault(a =>
                a.ObjectAction == received.ObjectAction &&
                a.ActionName == received.ActionName);

            if (match != null)
            {
                PrintOnDebug($"✅ User action matched: {received.ObjectAction}.{received.ActionName}");

                var response = curNode.GetResponseByID(match.ResponseID);
                if (response != null)
                {
                    var index = curNode.responses.IndexOf(response);
                    ChooseResponse(index);
                }
            }
            else
            {
                PrintOnDebug($"❌ User action mismatch: {received.ObjectAction}.{received.ActionName}");
            }
        }
    }
}
