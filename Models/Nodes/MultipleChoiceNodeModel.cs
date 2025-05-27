using System.Collections.Generic;

namespace Models.Nodes
{
    [System.Serializable]
    public class MultipleChoiceNodeModel : TweenityNodeModel
    {
        [UnityEngine.SerializeField]
        private string question;

        public string Question
        {
            get => question;
            set => question = value;
        }

        public MultipleChoiceNodeModel(string title) : base(title, NodeType.MultipleChoice)
        {
            question = "";
        }

        public void AddChoice(string answerText)
        {
            OutgoingPaths.Add(new PathData(label: answerText));
        }

        public void UpdateChoice(int index, string newLabel)
        {
            if (index >= 0 && index < OutgoingPaths.Count)
                OutgoingPaths[index].Label = newLabel;
        }

        public void ConnectChoiceTo(int index, string targetNodeID)
        {
            if (index >= 0 && index < OutgoingPaths.Count)
                OutgoingPaths[index].TargetNodeID = targetNodeID;
        }

        public void SetTriggerForOption(int index, string trigger)
        {
            if (index >= 0 && index < OutgoingPaths.Count)
            {
                OutgoingPaths[index].Trigger = trigger;
            }
        }
    }

}
