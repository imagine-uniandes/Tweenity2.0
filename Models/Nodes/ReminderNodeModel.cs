using System.Collections.Generic;
using UnityEngine;

namespace Models.Nodes
{
    public class ReminderNodeModel : TweenityNodeModel
    {
        
        public ReminderNodeModel(string title) : base(title, NodeType.Reminder)
        {

            // Add default Remind instruction (empty but typed)
            Instructions.Add(new ActionInstruction(ActionInstructionType.Remind));

            // Initialize two paths immediately
            OutgoingPaths.Add(new PathData("Success", "", ""));
            OutgoingPaths.Add(new PathData("Reminder", "", ""));
        }

        public void SetSuccessPath(string objectName, string scriptName, string methodName, string targetNodeID)
        {
            string trigger = $"{objectName}:{scriptName}.{methodName}";
            OutgoingPaths[0].Label = "Success";
            OutgoingPaths[0].Trigger = trigger;
            OutgoingPaths[0].TargetNodeID = targetNodeID;
        }

        public void SetReminderPath(string objectName, string scriptName, string methodName)
        {
            string trigger = $"{objectName}:{scriptName}.{methodName}";

            while (OutgoingPaths.Count <= 1)
            {
                OutgoingPaths.Add(new PathData("Reminder", "", ""));
            }

            OutgoingPaths[1].Label = "Reminder";
            OutgoingPaths[1].Trigger = trigger;
            OutgoingPaths[1].TargetNodeID = "";
        }
        
        /// <summary>
        /// Ensures there is exactly one Remind instruction and updates it.
        /// </summary>
        public void SetReminderInstruction(string objectName, string methodName, string parameters = "")
        {
            var instruction = Instructions.Find(i => i.Type == ActionInstructionType.Remind);
            if (instruction == null)
            {
                instruction = new ActionInstruction(ActionInstructionType.Remind);
                Instructions.Add(instruction);
            }

            instruction.ObjectName = objectName;
            instruction.MethodName = methodName;
            instruction.Params = parameters;
        }

    }
}
