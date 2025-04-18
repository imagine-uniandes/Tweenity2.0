using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Models.Nodes;
using UnityEngine;

namespace Simulation.Runtime
{
    public static class SimulatorInstructionInterpreter
    {
        public static IEnumerator ExecuteInstructions(
            List<string> instructions,
            TweenityNodeModel node,
            Action onComplete)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.StartsWith("Wait(", StringComparison.OrdinalIgnoreCase))
                {
                    float seconds = ParseWait(instruction);
                    if (seconds > 0)
                    {
                        yield return new WaitForSeconds(seconds);
                    }
                }
                else if (instruction.Equals("AwaitTrigger()", StringComparison.OrdinalIgnoreCase))
                {
                    var allTriggers = node.OutgoingPaths
                        .Where(p => !string.IsNullOrEmpty(p.Trigger))
                        .Select(p => p.Trigger)
                        .ToHashSet();

                    if (allTriggers.Count == 0)
                    {
                        Debug.LogWarning($"[Interpreter] AwaitTrigger() called but no triggers defined in OutgoingPaths of node {node.NodeID}.");
                        continue;
                    }

                    bool done = false;

                    void OnTriggerReceived(string triggered)
                    {
                        if (allTriggers.Contains(triggered))
                        {
                            done = true;
                            SimulationEventManager.OnTriggerFired -= OnTriggerReceived;
                        }
                    }

                    SimulationEventManager.OnTriggerFired += OnTriggerReceived;

                    while (!done)
                        yield return null;
                }
                else
                {
                    Debug.LogWarning($"[Interpreter] Unknown instruction: {instruction}");
                }
            }

            onComplete?.Invoke();
        }

        private static float ParseWait(string instruction)
        {
            var match = Regex.Match(instruction, @"Wait\((.*?)\)");
            if (match.Success && float.TryParse(match.Groups[1].Value, out float result))
            {
                return result;
            }

            Debug.LogWarning($"[Interpreter] Failed to parse Wait(): {instruction}");
            return 0f;
        }
    }
}
