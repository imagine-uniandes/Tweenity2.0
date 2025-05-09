using System;

namespace Models.Nodes
{
    /// <summary>
    /// Enumerates all valid action instructions supported in Tweenity.
    /// 
    /// Current supported instructions:
    /// 
    /// - Remind:
    ///   Triggers a method on a GameObject (usually after a timeout).
    ///   - Requires: ObjectName, MethodName
    ///   - Optional: Params (can hold delay value or metadata)
    /// 
    /// - Wait:
    ///   Delays progression by a specified number of seconds.
    ///   - Requires: Params (float duration in seconds)
    ///   - Ignores: ObjectName, MethodName
    /// </summary>
    public enum ActionInstructionType
    {
        Remind,
        Wait,
        Action,
        AwaitAction
    }

    /// <summary>
    /// Represents a structured instruction inside a Tweenity node.
    /// These are used to define node-level simulator behaviors (e.g., reminders, delays).
    /// </summary>
    [Serializable]
    public class ActionInstruction
    {
        public ActionInstructionType Type;  // Semantic type of instruction (e.g., Remind, Wait)
        public string ObjectName;           // Optional: GameObject name to act on
        public string MethodName;           // Optional: Script.Method to call
        public string Params;               // Optional: duration, message, etc.

        public ActionInstruction(ActionInstructionType type, string objectName = "", string methodName = "", string parameters = "")
        {
            Type = type;
            ObjectName = objectName;
            MethodName = methodName;
            Params = parameters;
        }
    }
}
