namespace Models.Nodes
{
    public class ReminderNodeModel : TweenityNodeModel
    {
        public string ReminderText { get; set; }
        public float ReminderTimer { get; set; }

        public ReminderNodeModel(string title) : base(title, NodeType.Reminder)
        {
            ReminderText = "";
            ReminderTimer = 0f;
        }
        public void SetReminderPath(string label, string targetNodeID)
        {
            if (OutgoingPaths.Count == 0)
            {
                OutgoingPaths.Add(new PathData(label, "", targetNodeID));
            }
            else
            {
                OutgoingPaths[0].Label = label;
                OutgoingPaths[0].TargetNodeID = targetNodeID;
            }
        }
    }
}
