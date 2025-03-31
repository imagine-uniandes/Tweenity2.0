using System.Collections.Generic;

namespace Models.Nodes
{
    public class DialogueNodeModel : TweenityNodeModel
    {
        public string DialogueText { get; set; }
        public List<string> Responses { get; private set; }

        public DialogueNodeModel(string title) : base(title, NodeType.Dialogue)
        {
            DialogueText = "";
            Responses = new List<string>();
        }

        public void AddResponse(string response)
        {
            Responses.Add(response);
        }

        public void RemoveResponseAt(int index)
        {
            if (index >= 0 && index < Responses.Count)
                Responses.RemoveAt(index);
        }

        public void UpdateResponse(int index, string newValue)
        {
            if (index >= 0 && index < Responses.Count)
                Responses[index] = newValue;
        }
    }
}
