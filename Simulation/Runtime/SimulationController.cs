// ACTUALIZADO: usa simulatorActions con ExecutionQueue
using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
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
        private ExecutionQueue _executionQueue;

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
            if (debugLectura)
                Debug.Log("[Simulation] " + msg);
        }

        public void SetSimulation(SimulationScript simulation)
        {
            currSim = simulation;

            if (currSim == null)
            {
                Debug.LogError("❌ SimulationScript is null!");
                return;
            }

            curNode = currSim.GetStartNode();
            Debug.Log($"🧠 [SetSimulation] Start node = {(curNode == null ? "null" : curNode.NodeID)}");

            if (curNode == null)
            {
                Debug.LogError("❌ Start node not found.");
                return;
            }

#if UNITY_EDITOR
            EditorApplication.delayCall += () =>
            {
                EditorApplication.delayCall += () =>
                {
                    Debug.Log($"⏳ [Simulation] Delayed enter to: {curNode.NodeID}");
                    EnterNode(curNode);
                };
            };
#else
            EnterNode(curNode);
#endif
        }

#if UNITY_EDITOR
        public void SetSimulationFromGraph(GraphModel model)
        {
            Debug.Log("🛠 [SimulationController] Building runtime graph from model...");
            var runtimeGraph = RuntimeGraphBuilder.FromGraphModel(model);
            SetSimulation(runtimeGraph);
        }
#endif

        public List<Response> GetCurrentResponses() => curNode.responses;

        public void ChooseResponse(int index)
        {
            _executionQueue.Clear();

            if (index < 0 || index >= curNode.responses.Count) return;

            var nextId = curNode.responses[index].DestinationNodeID;
            var nextNode = currSim.GetNode(nextId);
            if (nextNode == null)
            {
                Debug.LogError($"❌ Node with ID {nextId} not found.");
                return;
            }

            EnterNode(nextNode);
        }

        private async void EnterNode(Node node)
        {
            Debug.Log($"➡️ [Simulation] EnterNode called for: {node.NodeID} [{node.Type}]");

            tokenSource?.Cancel();
            tokenSource = new CancellationTokenSource();
            _executionQueue.Clear();

            curNode = node;
            curSimulatorActions = node.simulatorActions;
            curExpectedUserAction = null;

#if UNITY_EDITOR
            graphView?.CenterOnNode(node.NodeID);
#endif

            onEnteredNode?.Invoke(node);

            foreach (var act in node.simulatorActions)
            {
                _executionQueue.AddLast(() => ExecuteSimulatorInstruction(act));
            }

            switch (node.Type)
            {
                case NodeType.Reminder:
                    if (node.userActions.Count >= 2)
                    {
                        var delay = node.simulatorActions.FirstOrDefault(a => !string.IsNullOrEmpty(a.ActionParams))?.ActionParams;
                        if (float.TryParse(delay, out float seconds))
                        {
                            Debug.Log($"⏱ [Reminder] Waiting {seconds}s before reminder...");
                            _ = ReminderAfterDelay(seconds, tokenSource.Token);
                        }
                        curExpectedUserAction = node.userActions[1];
                    }
                    break;

                case NodeType.Timeout:
                    if (node.userActions.Count >= 2)
                    {
                        var delay = node.simulatorActions.FirstOrDefault(a => !string.IsNullOrEmpty(a.ActionParams))?.ActionParams;
                        if (float.TryParse(delay, out float seconds))
                        {
                            Debug.Log($"⏱ [Timeout] Waiting {seconds}s before timeout...");
                            _ = TimeoutAfterDelay(seconds, tokenSource.Token);
                        }
                        curExpectedUserAction = node.userActions[1];
                    }
                    break;

                default:
                    curExpectedUserAction = node.userActions.FirstOrDefault();
                    if (!node.userActions.Any() && node.responses.Count == 1)
                    {
                        Debug.Log("📤 [Auto] Advancing to single response...");
                        ChooseResponse(0);
                    }
                    break;
            }
        }

        private async Task ExecuteSimulatorInstruction(Action act)
        {
            if (string.IsNullOrEmpty(act.ObjectAction) || string.IsNullOrEmpty(act.ActionName)) return;

            if (act.Type == ActionInstructionType.Wait && float.TryParse(act.ActionParams, out float delay))
            {
                await Task.Delay((int)(delay * 1000));
                return;
            }

            var obj = GameObject.Find(act.ObjectAction);
            if (obj == null)
            {
                Debug.LogWarning($"🔍 Object not found: {act.ObjectAction}");
                return;
            }

            foreach (var script in obj.GetComponents<MonoBehaviour>())
            {
                if (script == null) continue;

                var method = script.GetType().GetMethod(act.ActionName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (method != null && method.GetParameters().Length == 0)
                {
                    Debug.Log($"✅ Invoking method: {act.ActionName} on {act.ObjectAction}");
                    method.Invoke(script, null);
                    break;
                }
            }
        }

        private async Task ReminderAfterDelay(float seconds, CancellationToken token)
        {
            try
            {
                await Task.Delay((int)(seconds * 1000), token);
                Debug.Log("🔔 Reminder fired.");
                ChooseResponse(1);
            }
            catch { Debug.Log("⚠️ Reminder cancelled."); }
        }

        private async Task TimeoutAfterDelay(float seconds, CancellationToken token)
        {
            try
            {
                await Task.Delay((int)(seconds * 1000), token);
                Debug.Log("⏰ Timeout fired.");
                ChooseResponse(0);
            }
            catch { Debug.Log("⚠️ Timeout cancelled."); }
        }

        public void VerifyUserAction(Action received)
        {
            if (received == null || curNode == null)
            {
                Debug.LogWarning("❌ [VerifyUserAction] Received null action or no current node.");
                return;
            }

            Debug.Log($"📥 [VerifyUserAction] Received: {received.ObjectAction}.{received.ActionName}");
            Debug.Log($"🧠 [VerifyUserAction] Current node: {curNode.NodeID} [{curNode.Type}]");
            Debug.Log("📋 [VerifyUserAction] Registered userActions in this node:");
            foreach (var a in curNode.userActions)
            {
                Debug.Log($" - {a.ObjectAction}.{a.ActionName} (ResponseID: {a.ResponseID})");
            }

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
                Debug.LogWarning($"❌ [VerifyUserAction] No matching user action for: {received.ObjectAction}.{received.ActionName}");
            }
        }
    }
}