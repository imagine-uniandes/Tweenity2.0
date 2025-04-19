using System.Collections.Generic;

namespace Models.Nodes
{
    public class DialogueNodeModel : TweenityNodeModel
    {
        public string DialogueText { get; set; }

        public DialogueNodeModel(string title) : base(title, NodeType.Dialogue)
        {
            DialogueText = "";
        }

        public void AddResponse(string responseText)
        {
            OutgoingPaths.Add(new PathData(label: responseText));
        }

        public void RemoveResponseAt(int index)
        {
            if (index >= 0 && index < OutgoingPaths.Count)
                OutgoingPaths.RemoveAt(index);
        }

        public void UpdateResponse(int index, string newValue)
        {
            if (index >= 0 && index < OutgoingPaths.Count)
                OutgoingPaths[index].Label = newValue;
        }

        public void ConnectResponseTo(int index, string targetNodeID)
        {
            if (index >= 0 && index < OutgoingPaths.Count)
                OutgoingPaths[index].TargetNodeID = targetNodeID;
        }
        public void SetTriggerForResponse(int index, string trigger)
        {
            if (index >= 0 && index < OutgoingPaths.Count)
            {
                OutgoingPaths[index].Trigger = trigger;
            }
        }

    }
}
