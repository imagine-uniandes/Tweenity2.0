using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Simulation.Runtime
{
    public class XRGlobalEventWatcher : MonoBehaviour
    {
        private void Start()
        {
            SubscribeToInteractables();
            SubscribeToSockets();
            SubscribeToZones();
        }

        private void SubscribeToInteractables()
        {
            foreach (var interactable in FindObjectsOfType<XRBaseInteractable>())
            {
                var name = interactable.gameObject.name;

                interactable.selectEntered.AddListener(_ =>
                {
                    SimulationEventManager.FireTrigger($"{name}.OnGrab");
                });

                interactable.selectExited.AddListener(_ =>
                {
                    SimulationEventManager.FireTrigger($"{name}.OnRelease");
                });

                interactable.activated.AddListener(_ =>
                {
                    SimulationEventManager.FireTrigger($"{name}.OnPress");
                });

                interactable.deactivated.AddListener(_ =>
                {
                    SimulationEventManager.FireTrigger($"{name}.OnReleasePress");
                });
            }
        }

        private void SubscribeToSockets()
        {
            foreach (var socket in FindObjectsOfType<XRSocketInteractor>())
            {
                var name = socket.gameObject.name;

                socket.selectEntered.AddListener(_ =>
                {
                    SimulationEventManager.FireTrigger($"{name}.OnPlace");
                });

                socket.selectExited.AddListener(_ =>
                {
                    SimulationEventManager.FireTrigger($"{name}.OnRemove");
                });
            }
        }

        private void SubscribeToZones()
        {
            foreach (var zone in FindObjectsOfType<Collider>())
            {
                if (!zone.isTrigger) continue;

                var zoneName = zone.gameObject.name;
                var listener = zone.gameObject.AddComponent<TriggerZoneListener>();
                listener.triggerEnter = () => SimulationEventManager.FireTrigger($"{zoneName}.OnEnter");
                listener.triggerExit  = () => SimulationEventManager.FireTrigger($"{zoneName}.OnExit");
            }
        }
    }
}
