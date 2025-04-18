using UnityEngine;

namespace Simulation.Runtime
{
    /// <summary>
    /// Este bootstrap se encarga de crear el XRGlobalEventWatcher automáticamente
    /// al cargar una escena, sin requerir edición en el editor ni en la jerarquía.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public static class SimulationRuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            // Evita duplicados si ya existe uno manualmente
            if (GameObject.FindObjectOfType<XRGlobalEventWatcher>() != null)
                return;

            var watcherGO = new GameObject("XRGlobalEventWatcher (Auto)");
            watcherGO.AddComponent<XRGlobalEventWatcher>();
            Object.DontDestroyOnLoad(watcherGO);

            Debug.Log("[SimulationBootstrap] XRGlobalEventWatcher inicializado automáticamente.");
        }
    }
}
