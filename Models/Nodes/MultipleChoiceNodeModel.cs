using System.Collections.Generic;

namespace Models.Nodes
{
    public class MultipleChoiceNodeModel : TweenityNodeModel
    {
        public List<string> Choices { get; private set; }

        public MultipleChoiceNodeModel(string title) : base(title, NodeType.MultipleChoice)
        {
            Choices = new List<string>();
        }

        public void AddChoice(string choice)
        {
            Choices.Add(choice);
        }
    }
}
