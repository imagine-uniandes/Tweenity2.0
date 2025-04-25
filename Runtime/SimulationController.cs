using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Events;
using Simulation.Runtime;

#if UNITY_EDITOR
using Models; // To recognize GraphModel
#endif

public class SimulationController : MonoBehaviour
{
    public bool debugCarga = false;
    public bool debugLectura = false;

    SimulationScript currSim;
    Node curNode;
    Action curReminder;
    Action curExpectedUserAction;
    List<Action> curSimulatorActions;

    public UnityEvent<Node> onEnteredNode = new();

    bool remember = true;
    bool timeout = true;

    CancellationTokenSource tokenSource;

    private void OnDisable() => tokenSource?.Cancel();

    public Node GetCurrentNode() => curNode;

    private void PrintOnDebug(string msg)
    {
        if (debugLectura) Debug.Log(msg);
    }

    void Start()
    {
        tokenSource = new CancellationTokenSource();
        onEnteredNode.AddListener(OnNodeEntered);
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

        onEnteredNode.Invoke(curNode);
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
            onEnteredNode.Invoke(nextNode);
        }
    }

    private async void OnNodeEntered(Node newNode)
    {
        PrintOnDebug("Entering node: " + newNode.title);
        curExpectedUserAction = new Action();
        curSimulatorActions = null;

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
            Invoke("ActivateReminder", float.Parse(curReminder.actionParams.Split(';')[0]));
            curExpectedUserAction = node.userActions[1];
        }
        else if (node.userActions.Count == 1)
        {
            curExpectedUserAction = node.userActions[0];
        }

        if (node.userActions.Count > 1 && node.tags.Contains("timeout"))
        {
            timeout = true;
            Invoke("Timeout", float.Parse(node.userActions[0].actionParams));
            curExpectedUserAction = node.userActions[1];
        }
    }

    public async Task<MethodInfo> ExecuteSimulatorActions(List<Action> simulatorActions)
    {
        MethodInfo taskObject = null;
        foreach (var action in simulatorActions)
        {
            GameObject obj = GameObject.Find(action.object2Action);
            if (obj != null)
                taskObject = await obj.GetComponent<ObjectController>().MethodAccess(action.actionName, action.actionParams);
            else
                Debug.LogWarning($"[Sim] Object not found: {action.object2Action}");
        }
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
        if (remember)
        {
            GameObject obj = GameObject.Find(curReminder.object2Action);
            if (obj != null)
                _ = obj.GetComponent<ObjectController>().MethodAccess(curReminder.actionName, curReminder.actionParams);
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
