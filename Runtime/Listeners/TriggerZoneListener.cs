using UnityEngine;

namespace Simulation.Runtime
{
    public class TriggerZoneListener : MonoBehaviour
    {
        public System.Action triggerEnter;
        public System.Action triggerExit;

        private void OnTriggerEnter(Collider other)
        {
            triggerEnter?.Invoke();
        }

        private void OnTriggerExit(Collider other)
        {
            triggerExit?.Invoke();
        }
    }
}
