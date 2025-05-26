using UnityEngine;
using Models.Nodes;
using Simulation.Runtime;
using Simulation;

namespace Tweenity
{
    public static class TweenityEvents
    {
        private static SimulationController _simulationController;

        /// <summary>
        /// Registers the active SimulationController to handle interaction events.
        /// </summary>
        public static void RegisterSimulationController(SimulationController controller)
        {
            _simulationController = controller;
        }

        /// <summary>
        /// Reports a user action to the simulation runtime using trigger-compatible format.
        /// </summary>
        public static void ReportAction(string objectName, string scriptName, string methodName, string parameters = "")
        {
            if (_simulationController == null)
            {
                Debug.LogWarning("⚠️ [TweenityEvents] No SimulationController registered. " +
                                 "Please open the Tweenity Graph Editor and start the simulation before reporting actions.");
                return;
            }

            var instruction = new ActionInstruction(
                type: ActionInstructionType.Action,
                objectName: objectName,
                methodName: $"{scriptName}.{methodName}",
                parameters: parameters
            );

            var triggerString = $"{instruction.ObjectName}:{instruction.MethodName}";

            _simulationController.VerifyUserAction(instruction);
        }

        /// <summary>
        /// Shorthand version for compatibility — assumes unknown script.
        /// </summary>
        public static void ReportAction(string objectName, string methodName)
        {
            ReportAction(objectName, "UnknownScript", methodName);
        }
    }
}
