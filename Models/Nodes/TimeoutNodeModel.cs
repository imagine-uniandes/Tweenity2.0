using System.Collections.Generic;

namespace Models.Nodes
{
    public class TimeoutNodeModel : TweenityNodeModel
    {
        public string Condition { get; set; }
        public float TimeoutDuration { get; set; }

        public TimeoutNodeModel(string title) : base(title, NodeType.Timeout)
        {
            Condition = "";
            TimeoutDuration = 0f;

            EnsureMinimumPaths();
        }

        /// <summary>
        /// Ensures that OutgoingPaths has at least 2 paths: Timeout (index 0) and Success (index 1)
        /// </summary>
        private void EnsureMinimumPaths()
        {
            while (OutgoingPaths.Count < 2)
            {
                string defaultLabel = OutgoingPaths.Count == 0 ? "Timeout Path" : "Success Path";
                OutgoingPaths.Add(new PathData(defaultLabel, "", ""));
            }
        }

        public void SetTimeoutPath(string label, string targetNodeID)
        {
            EnsureMinimumPaths();
            OutgoingPaths[0].Label = label;
            OutgoingPaths[0].TargetNodeID = targetNodeID;
        }

        public void SetSuccessPath(string label, string targetNodeID)
        {
            EnsureMinimumPaths();
            OutgoingPaths[1].Label = label;
            OutgoingPaths[1].TargetNodeID = targetNodeID;
        }

        public void SetTriggerForTimeout(string trigger)
        {
            EnsureMinimumPaths();
            OutgoingPaths[0].Trigger = trigger;
        }

        public void SetTriggerForSuccess(string trigger)
        {
            EnsureMinimumPaths();
            OutgoingPaths[1].Trigger = trigger;
        }
    }
}
