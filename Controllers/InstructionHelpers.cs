using System.Collections.Generic;
using System.Globalization;
using Models.Nodes;

namespace Controllers
{
    public static class InstructionHelpers
    {
        public static void AddAwaitTriggerInstruction(TweenityNodeModel node)
        {
            if (node == null) return;

            if (node.Instructions == null)
                node.Instructions = new List<string>();

            node.Instructions.Add("AwaitTrigger()");
        }

        public static void AddWaitInstruction(TweenityNodeModel node, float seconds)
        {
            if (node == null || seconds < 0) return;

            if (node.Instructions == null)
                node.Instructions = new List<string>();

            node.Instructions.Add($"Wait({seconds.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)})");
        }
    }
}