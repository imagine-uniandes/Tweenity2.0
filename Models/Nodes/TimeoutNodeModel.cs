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
            OutgoingPaths.Add(new PathData("Timeout Path", "", "")); // path 0
            OutgoingPaths.Add(new PathData("Success Path", "", "")); // path 1
        }

        public void SetTimeoutPath(string label, string targetNodeID)
        {
            OutgoingPaths[0].Label = label;
            OutgoingPaths[0].TargetNodeID = targetNodeID;
        }

        public void SetSuccessPath(string label, string targetNodeID)
        {
            OutgoingPaths[1].Label = label;
            OutgoingPaths[1].TargetNodeID = targetNodeID;
        }
        public void SetTriggerForTimeout(string trigger)
        {
            if (OutgoingPaths.Count >= 1)
                OutgoingPaths[0].Trigger = trigger;
        }

        public void SetTriggerForSuccess(string trigger)
        {
            if (OutgoingPaths.Count >= 2)
                OutgoingPaths[1].Trigger = trigger;
        }

    }
}
