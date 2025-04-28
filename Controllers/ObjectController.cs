using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using Simulation;

namespace Controllers
{
    /// <summary>
    /// ObjectController scans a GameObject for methods marked with [TweenityTrigger].
    /// 
    /// Contract:
    ///  - Method must be marked with [TweenityTrigger]
    ///  - Inside the method, you must call:
    ///      TweenityEvents.ReportAction(gameObject.name, nameof(MethodName), parameters);
    /// 
    ///  This ensures:
    ///      - Object name matches at runtime
    ///      - Action name matches exactly with method name
    /// </summary>
    public static class ObjectController
    {
        /// <summary>
        /// Scans the given GameObject and returns a list of valid action trigger methods.
        /// Only methods marked with [TweenityTrigger] are considered.
        /// </summary>
        public static List<string> GetAvailableTriggerMethods(GameObject target)
        {
            List<string> validMethods = new();

            if (target == null) return validMethods;

            // Check all attached MonoBehaviours
            MonoBehaviour[] scripts = target.GetComponents<MonoBehaviour>();

            foreach (var script in scripts)
            {
                if (script == null) continue; // Can happen with missing scripts

                MethodInfo[] methods = script.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

                foreach (var method in methods)
                {
                    if (method.IsSpecialName) continue; // Skip property getters/setters etc.

                    // Check if method is marked with [TweenityTrigger]
                    if (method.GetCustomAttribute(typeof(TweenityTriggerAttribute)) != null)
                    {
                        validMethods.Add($"{script.GetType().Name}.{method.Name}");
                    }
                }
            }

            return validMethods;
        }
    }
}
