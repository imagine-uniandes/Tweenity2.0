using Simulation.Runtime;
using UnityEngine;

namespace Simulation
{
    public static class TweenityEvents
    {
        private static SimulationController _simulationController;

        /// <summary>
        /// Registers the active SimulationController to handle interaction events.
        /// This is called automatically by the GraphController when simulation starts.
        /// </summary>
        public static void RegisterSimulationController(SimulationController controller)
        {
            _simulationController = controller;
        }

        /// <summary>
        /// Reports a user action to the simulation runtime.
        /// </summary>
        /// <param name="objectName">The name of the GameObject the action belongs to (e.g., "Cube")</param>
        /// <param name="scriptName">The name of the script (MonoBehaviour) that implements the action (e.g., "CubeClickTest")</param>
        /// <param name="methodName">The method name being triggered (e.g., "GrabCube")</param>
        /// <param name="parameters">Optional action parameters</param>
        public static void ReportAction(string objectName, string scriptName, string methodName, string parameters = "")
        {
            if (_simulationController == null)
            {
                Debug.LogWarning("⚠️ [TweenityEvents] No SimulationController registered. " +
                    "Please open the Tweenity Graph Editor and start the simulation before reporting actions.");
                return;
            }

            var action = new Action
            {
                ObjectAction = objectName,
                ActionName = $"{scriptName}.{methodName}",
                ActionParams = parameters
            };

            Debug.Log($"📨 [TweenityEvents] Reporting action: {action.ObjectAction}.{action.ActionName}");
            _simulationController.VerifyUserAction(action);
        }

        /// <summary>
        /// Shorthand version for compatibility — not recommended unless necessary.
        /// </summary>
        public static void ReportAction(string objectName, string methodName)
        {
            ReportAction(objectName, "UnknownScript", methodName);
        }

        /// <summary>
        /// ✅ How to call this from your MonoBehaviour:
        /// TweenityEvents.ReportAction(gameObject.name, GetType().Name, nameof(MyMethod));
        /// </summary>
    }
}
