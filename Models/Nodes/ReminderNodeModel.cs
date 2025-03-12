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
    }
}
