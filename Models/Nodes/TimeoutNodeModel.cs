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
        }

        public void SetTimeoutPath(string label, string targetNodeID)
        {
            var existing = OutgoingPaths.Find(p => p.Trigger == "timeout");
            if (existing != null)
            {
                existing.Label = label;
                existing.TargetNodeID = targetNodeID;
            }
            else
            {
                OutgoingPaths.Add(new PathData(label, "timeout", targetNodeID));
            }
        }

        public void SetSuccessPath(string label, string targetNodeID)
        {
            var existing = OutgoingPaths.Find(p => p.Trigger == "success");
            if (existing != null)
            {
                existing.Label = label;
                existing.TargetNodeID = targetNodeID;
            }
            else
            {
                OutgoingPaths.Add(new PathData(label, "success", targetNodeID));
            }
        }
    }
}
