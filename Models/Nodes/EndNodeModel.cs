namespace Models.Nodes
{
    public class EndNodeModel : TweenityNodeModel
    {
        public EndNodeModel(string title) : base(title, NodeType.End)
        {
        }

        public void ClearOutgoingPaths()
        {
            OutgoingPaths.Clear();
        }
    }
}
