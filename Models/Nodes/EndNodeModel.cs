namespace Models.Nodes
{
    public class EndNodeModel : TweenityNodeModel
    {
        public EndNodeModel(string title) : base(title, NodeType.End)
        {
        }

        // Por claridad, se puede prevenir explícitamente agregar caminos
        public void ClearOutgoingPaths()
        {
            OutgoingPaths.Clear();
        }
    }
}
