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

            onEnteredNode?.Invoke(curNode);
        }

#if UNITY_EDITOR
        public void SetSimulationFromGraph(GraphModel model)
        {
            var runtimeGraph = RuntimeGraphBuilder.FromGraphModel(model);
            SetSimulation(runtimeGraph);
        }
#endif

        public List<Response> GetCurrentResponses() => curNode.responses;

        public void ChooseResponse(int responseIndex)
        {
            if (!curNode.tags.Contains("end"))
            {
                string nextNodeID = curNode.responses[responseIndex].destinationNode;
                Node nextNode = currSim.GetNode(nextNodeID);
                curNode = nextNode;
                onEnteredNode?.Invoke(nextNode);
            }
        }

        private async void OnNodeEntered(Node newNode)
        {
            tokenSource?.Cancel(); // Cancel any previous timers
            tokenSource = new CancellationTokenSource();

            PrintOnDebug("Entering node: " + newNode.title);
            curExpectedUserAction = new Action();
            curSimulatorActions = null;

#if UNITY_EDITOR
            EditorGraphHelper.TryCenterNode(newNode.NodeID);
#endif

            if (newNode.tags.Contains("end"))
            {
                await ExecuteSimulatorActions(newNode.simulatorActions);
                return;
            }

            if (newNode.userActions.Count == 0)
            {
                var taskObject = await ExecuteSimulatorActions(newNode.simulatorActions);

                if (newNode.tags.Contains("random"))
                    ChooseResponse(Random.Range(0, newNode.responses.Count));
                else if (newNode.responses.Count == 1 && taskObject != null && !newNode.tags.Contains("dialogue"))
                    ChooseResponse(0);
            }
            else
            {
                HandleUserActionNode(newNode);
                curSimulatorActions = newNode.simulatorActions;
            }
        }

        private void HandleUserActionNode(Node node)
        {
            if (node.userActions.Count > 1 && node.tags.Contains("reminder"))
            {
                curReminder = node.userActions[0];
                remember = true;
                _ = ActivateReminderAfterDelay(float.Parse(curReminder.actionParams), tokenSource.Token);
                curExpectedUserAction = node.userActions[1];
            }
            else if (node.userActions.Count == 1)
            {
                curExpectedUserAction = node.userActions[0];
            }

            if (node.userActions.Count > 1 && node.tags.Contains("timeout"))
            {
                timeout = true;
                _ = TimeoutAfterDelay(float.Parse(node.userActions[0].actionParams), tokenSource.Token);
                curExpectedUserAction = node.userActions[1];
            }
        }

        private async Task TimeoutAfterDelay(float seconds, CancellationToken token)
        {
            try
            {
                await Task.Delay((int)(seconds * 1000), token);
                Timeout();
            }
            catch (TaskCanceledException) { /* Timer cancelled */ }
        }

        private async Task ActivateReminderAfterDelay(float seconds, CancellationToken token)
        {
            try
            {
                await Task.Delay((int)(seconds * 1000), token);
                ActivateReminder();
            }
            catch (TaskCanceledException) { /* Timer cancelled */ }
        }

        public async Task<MethodInfo> ExecuteSimulatorActions(List<Action> simulatorActions)
        {
            MethodInfo taskObject = null;

            foreach (var action in simulatorActions)
            {
                GameObject obj = GameObject.Find(action.object2Action);
                if (obj != null)
                {
                    var scripts = obj.GetComponents<MonoBehaviour>();

                    foreach (var script in scripts)
                    {
                        if (script == null) continue;

                        // Find method by action name
                        var method = script.GetType().GetMethod(action.actionName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (method != null && method.GetParameters().Length == 0)
                        {
                            method.Invoke(script, null);
                            taskObject = method;
                            break;
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[SimulationController] Object not found: {action.object2Action}");
                }
            }

            await Task.Yield(); // Still async, even if immediate
            return taskObject;
        }

        public async void VerifyUserAction(Action receivedAction)
        {
            PrintOnDebug($"Expected: {curExpectedUserAction.object2Action}.{curExpectedUserAction.actionName}");
            PrintOnDebug($"Received: {receivedAction.object2Action}.{receivedAction.actionName}");

            if (receivedAction.Equals(curExpectedUserAction) || curNode.userActions.Contains(receivedAction))
            {
                remember = false;
                timeout = false;

                tokenSource?.Cancel(); // Cancel any pending reminders or timeouts

                if (curSimulatorActions.Count > 0)
                {
                    var taskObject = await ExecuteSimulatorActions(curSimulatorActions);
                    if (taskObject != null)
                        SelectNextNode(receivedAction);
                }
                else
                {
                    SelectNextNode(receivedAction);
                }
            }
            else
            {
                PrintOnDebug("User action was not expected.");
            }
        }

        public void SelectNextNode(Action act)
        {
            if (curNode.tags.Contains("random"))
            {
                ChooseResponse(Random.Range(0, curNode.responses.Count));
            }
            else if (curNode.tags.Contains("multiplechoice"))
            {
                ChooseResponse(GetPositionOfResponse($"{act.object2Action}.{act.actionName}"));
            }
            else
            {
                if (curNode.responses.Count == 1)
                    ChooseResponse(0);
                else if (curNode.tags.Contains("timeout"))
                    ChooseResponse(GetActionResponse(curNode.responses, GetPositionOfResponse("timeout")));
            }
        }

        public void Timeout()
        {
            if (timeout)
                ChooseResponse(GetPositionOfResponse("timeout"));
        }

        public void ActivateReminder()
        {
            if (!remember) return;

            if (curNode == null)
            {
                Debug.LogWarning("No current node during reminder activation.");
                return;
            }

            // Find a simulator action specifically tagged as "reminder"
            var reminderAction = curNode.simulatorActions
                .FirstOrDefault(a => a.actionParams == "reminder");

            if (reminderAction == null)
            {
                Debug.LogWarning("No ReminderBehavior action found in simulatorActions.");
                return;
            }

            if (string.IsNullOrEmpty(reminderAction.object2Action) || string.IsNullOrEmpty(reminderAction.actionName))
            {
                Debug.LogWarning("ReminderBehavior action has missing object or method name.");
                return;
            }

            GameObject obj = GameObject.Find(reminderAction.object2Action);
            if (obj != null)
            {
                var scripts = obj.GetComponents<MonoBehaviour>();

                foreach (var script in scripts)
                {
                    if (script == null) continue;

                    var method = script.GetType().GetMethod(reminderAction.actionName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (method != null && method.GetParameters().Length == 0)
                    {
                        method.Invoke(script, null);
                        Debug.Log($"✅ Reminder behavior '{reminderAction.actionName}' invoked on '{reminderAction.object2Action}'");
                        break;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[SimulationController] Reminder target object not found: {reminderAction.object2Action}");
            }
        }

        private int GetActionResponse(List<Response> responses, int timeoutIndex)
        {
            return responses.Count - 1 - timeoutIndex;
        }

        public int GetPositionOfResponse(string actionResponseText)
        {
            for (int i = 0; i < curNode.responses.Count; i++)
            {
                if (curNode.responses[i].destinationNode.StartsWith(actionResponseText))
                    return i;
            }
            return 0;
        }
    }
}
