namespace Models.Nodes
{
    public class NoTypeNodeModel : TweenityNodeModel
    {
        public NoTypeNodeModel(string title) : base(title, NodeType.NoType)
        {
        }

        public void SetConnection(string label, string targetNodeID)
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
