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
    }
}
