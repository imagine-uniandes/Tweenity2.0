using System;

namespace Simulation.Runtime
{
    public static class SimulationEventManager
    {
        public static event Action<string> OnTriggerFired;

        public static void FireTrigger(string triggerName)
        {
            OnTriggerFired?.Invoke(triggerName);
            UnityEngine.Debug.Log($"[SimulationEventManager] Fired trigger: {triggerName}");
        }

        public static void ClearListeners()
        {
            OnTriggerFired = null;
        }
    }
}
