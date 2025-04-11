namespace Models.Nodes
{
    public class StartNodeModel : TweenityNodeModel
    {
        public StartNodeModel(string title) : base(title, NodeType.Start)
        {
        }

        public void SetStartPath(string label, string targetNodeID)
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
