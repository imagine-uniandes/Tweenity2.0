using System;
using Simulation;

namespace Tweenity {
    /// <summary>
    /// Marker attribute to indicate which methods are valid Tweenity simulation triggers.
    /// 
    /// Methods marked with [TweenityTrigger] should internally call:
    ///     TweenityEvents.ReportAction(gameObject.name, nameof(MethodName), parameters);
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class TweenityEventAttribute : Attribute
    {
    }
}
