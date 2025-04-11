namespace Models.Nodes
{
    [System.Serializable]
    public class PathData
    {
        public string Label;         // Texto visible
        public string Trigger;       // (Opcional) nombre de evento disparador
        public string TargetNodeID;  // ID del nodo destino

        public PathData(string label = "New Path", string trigger = "", string targetNodeID = "")
        {
            Label = label;
            Trigger = trigger;
            TargetNodeID = targetNodeID;
        }
    }
}
