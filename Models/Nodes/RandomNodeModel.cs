using System.Collections.Generic;

namespace Models.Nodes
{
    public class RandomNodeModel : TweenityNodeModel
    {
        public RandomNodeModel(string title) : base(title, NodeType.Random)
        {
        }

        public void AddPath(string label = null)
        {
            OutgoingPaths.Add(new PathData(label ?? $"Path {OutgoingPaths.Count + 1}"));
        }

        public void UpdatePath(int index, string newValue)
        {
            if (index >= 0 && index < OutgoingPaths.Count)
            {
                OutgoingPaths[index].Label = newValue;
            }
        }

        public void ConnectPathTo(int index, string targetNodeID)
        {
            if (index >= 0 && index < OutgoingPaths.Count)
            {
                OutgoingPaths[index].TargetNodeID = targetNodeID;
            }
        }
    }
}
