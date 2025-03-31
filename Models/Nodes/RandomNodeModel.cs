using System.Collections.Generic;

namespace Models.Nodes
{
    public class RandomNodeModel : TweenityNodeModel
    {
        public List<string> PossiblePaths { get; private set; }

        public RandomNodeModel(string title) : base(title, NodeType.Random)
        {
            PossiblePaths = new List<string>();
        }

        public void AddPath(string path)
        {
            PossiblePaths.Add(path);
        }

        public void UpdatePath(int index, string newValue)
        {
            if (index >= 0 && index < PossiblePaths.Count)
            {
                PossiblePaths[index] = newValue;
            }
        }
    }
}
