using System.Collections.Generic;

namespace Models.Nodes
{
    public class MultipleChoiceNodeModel : TweenityNodeModel
    {
        // New inner class for detailed choice info
        public class ChoiceData
        {
            public string AnswerText;
            // Future expansion:
            // public string TriggerName;
            // public string ConnectedNodeID;
        }

        public string Question { get; set; }
        public List<ChoiceData> Choices { get; private set; }

        public MultipleChoiceNodeModel(string title) : base(title, NodeType.MultipleChoice)
        {
            Question = "";
            Choices = new List<ChoiceData>();
        }

        public void AddChoice(string answer)
        {
            Choices.Add(new ChoiceData { AnswerText = answer });
        }
    }
}
