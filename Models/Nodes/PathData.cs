namespace Models.Nodes
{
    [System.Serializable]
    public class PathData
    {
        public string Label;
        public string Trigger;
        public string TargetNodeID;

        public PathData(string label = "New Path", string trigger = "", string targetNodeID = "")
        {
            Label = label;
            Trigger = trigger;
            TargetNodeID = targetNodeID;
        }
    }
}
