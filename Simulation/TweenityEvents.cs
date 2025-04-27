using Simulation.Runtime;
using UnityEngine;

namespace Simulation{
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
        /// Reports an interaction event to the current simulation.
        /// </summary>
        public static void ReportAction(string objectName, string actionName, string parameters = "")
        {
            if (_simulationController == null)
            {
                Debug.LogWarning("⚠️ [TweenityEvents] No SimulationController registered. "
                    + "Please open the Tweenity Graph Editor and start the simulation before reporting actions.");
                return;
            }

            var action = new Action
            {
                object2Action = objectName,
                actionName = actionName,
                actionParams = parameters
            };

            _simulationController.VerifyUserAction(action);
        }
    }
}
