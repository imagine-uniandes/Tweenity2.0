using System;
using System.Collections;
using System.Linq;
using Models;
using Models.Nodes;
using UnityEngine;

namespace Simulation.Runtime
{
    public class SimulationRuntimeEngine
    {
        private readonly GraphModel graph;
        private readonly GraphController controller;
        private readonly Action<string> highlightCallback;

        private TweenityNodeModel currentNode;
        private ExecutionGraphController executionGraph;

        private Coroutine currentRoutine;
        private bool isExecuting = false;

        public SimulationRuntimeEngine(
            GraphModel graph,
            GraphController controller,
            Action<string> onHighlightNode)
        {
            this.graph = graph;
            this.controller = controller;
            this.highlightCallback = onHighlightNode;

            executionGraph = new ExecutionGraphController(graph);
            SimulationEventManager.OnTriggerFired += OnTriggerFired;
        }

        public void RunFrom(TweenityNodeModel startNode)
        {
            if (startNode == null)
            {
                Debug.LogError("[RuntimeEngine] Start node is null.");
                return;
            }

            RunNode(startNode);
        }

        private void RunNode(TweenityNodeModel node)
        {
            currentNode = node;
            highlightCallback?.Invoke(node.NodeID);
            isExecuting = true;

            currentRoutine = CoroutineRunner.Run(
                SimulatorInstructionInterpreter.ExecuteInstructions(
                    node.RuntimeInstructions,
                    node,
                    OnCurrentNodeFinished));
        }

        private void OnCurrentNodeFinished()
        {
            isExecuting = false;

            if (currentNode.OutgoingPaths.Count > 0)
            {
                var nextId = currentNode.OutgoingPaths[0].TargetNodeID;
                var nextNode = graph.GetNode(nextId);

                if (nextNode != null)
                {
                    RunNode(nextNode);
                }
                else
                {
                    Debug.LogWarning($"[RuntimeEngine] Target node not found: {nextId}");
                }
            }
            else
            {
                Debug.Log("[RuntimeEngine] No more outgoing paths. Execution ends.");
            }
        }

        private void OnTriggerFired(string triggerName)
        {
            if (currentNode == null || executionGraph == null) return;

            var nextNode = executionGraph.GetClosestNodeByTrigger(currentNode.NodeID, triggerName);

            if (nextNode != null)
            {
                Debug.Log($"[RuntimeEngine] Trigger '{triggerName}' matched. Jumping to node: {nextNode.Title}");

                if (currentRoutine != null)
                {
                    CoroutineRunner.Stop(currentRoutine);
                }

                RunNode(nextNode);
            }
            else
            {
                Debug.Log($"[RuntimeEngine] Trigger '{triggerName}' received but no valid target found. Ignoring.");
            }
        }
    }
}
