using System.Collections.Generic;
using UnityEngine;

namespace Models.Nodes
{
    public class ReminderNodeModel : TweenityNodeModel
    {
        public float ReminderTimer { get; set; }

        public ReminderNodeModel(string title) : base(title, NodeType.Reminder)
        {
            ReminderTimer = 0f;

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

    }
}
