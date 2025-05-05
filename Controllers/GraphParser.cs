// FILE: Controllers/GraphParser.cs

using System.Text;
using System.Collections.Generic;
using Models.Nodes;
using System.Text.RegularExpressions;
using Models;
using System;
using UnityEngine;
using System.Linq;

namespace Controllers
{
    public static class GraphParser
    {
        public static string ExportToTwee(List<TweenityNodeModel> nodes)
        {
            StringBuilder twee = new StringBuilder();

            foreach (var node in nodes)
            {
                switch (node)
                {
                    case DialogueNodeModel dialogue:
                        SaveDialogueNode(twee, dialogue);
                        break;
                    case ReminderNodeModel reminder:
                        SaveReminderNode(twee, reminder);
                        break;
                    case TimeoutNodeModel timeout:
                        SaveTimeoutNode(twee, timeout);
                        break;
                    case MultipleChoiceNodeModel multiple:
                        SaveMultipleChoiceNode(twee, multiple);
                        break;
                    case StartNodeModel:
                    case EndNodeModel:
                    case NoTypeNodeModel:
                    case RandomNodeModel:
                        SaveGenericNode(twee, node);
                        break;
                    default:
                        Debug.LogWarning($"Unknown node type during export: {node.GetType().Name}");
                        break;
                }
            }

            return twee.ToString();
        }

        private static void SaveDialogueNode(StringBuilder twee, DialogueNodeModel node)
        {
            WriteHeader(twee, node);
            WriteDescription(twee, node);
            WriteSpecialFields(twee, node.DialogueText ?? "null", "null");
            WritePaths(twee, node);
            WriteInstructions(twee, node);
        }

        private static void SaveReminderNode(StringBuilder twee, ReminderNodeModel node)
        {
            WriteHeader(twee, node);
            WriteDescription(twee, node);
            WriteSpecialFields(twee, "null", node.ReminderTimer.ToString());
            WritePaths(twee, node);
            WriteInstructions(twee, node);
        }

        private static void SaveTimeoutNode(StringBuilder twee, TimeoutNodeModel node)
        {
            WriteHeader(twee, node);
            WriteDescription(twee, node);
            WriteSpecialFields(twee, node.Condition ?? "null", node.TimeoutDuration.ToString());
            WritePaths(twee, node);
            WriteInstructions(twee, node);
        }

        private static void SaveMultipleChoiceNode(StringBuilder twee, MultipleChoiceNodeModel node)
        {
            WriteHeader(twee, node);
            WriteDescription(twee, node);
            WriteSpecialFields(twee, node.Question ?? "null", "null");
            WritePaths(twee, node);
            WriteInstructions(twee, node);
        }

        private static void SaveGenericNode(StringBuilder twee, TweenityNodeModel node)
        {
            WriteHeader(twee, node);
            WriteDescription(twee, node);
            WriteSpecialFields(twee, "null", "null");
            WritePaths(twee, node);
            WriteInstructions(twee, node);
        }

        private static void WriteHeader(StringBuilder twee, TweenityNodeModel node)
        {
            string pos = $"{node.Position.x},{node.Position.y}";
            string id = node.NodeID;
            twee.Append(":: ").Append(node.Title)
                .Append(" [").Append(node.Type.ToString().ToLower()).Append("] ")
                .Append($"{{\"position\":\"{pos}\",\"id\":\"{id}\"}}")
                .AppendLine();
        }

        private static void WriteDescription(StringBuilder twee, TweenityNodeModel node)
        {
            twee.AppendLine(node.Description ?? "");
            twee.AppendLine("@");
        }

        private static void WriteSpecialFields(StringBuilder twee, string specialField1, string specialField2)
        {
            twee.AppendLine($"- {specialField1}");
            twee.AppendLine($"- {specialField2}");
        }

        private static void WritePaths(StringBuilder twee, TweenityNodeModel node)
        {
            var pathJsons = node.OutgoingPaths.Select(path =>
                $"{{\"label\":\"{path.Label}\",\"trigger\":\"{path.Trigger}\",\"target\":\"{path.TargetNodeID}\"}}"
            );
            twee.AppendLine("[" + string.Join(",", pathJsons) + "]");
        }

        private static void WriteInstructions(StringBuilder twee, TweenityNodeModel node)
        {
            twee.AppendLine("<");
            foreach (var instr in node.Instructions ?? new List<ActionInstruction>())
            {
                string line = instr.Type switch
                {
                    ActionInstructionType.Remind => $"Remind({instr.ObjectName}:{instr.MethodName})",
                    ActionInstructionType.Wait => $"Wait({instr.Params})",
                    _ => "// Unknown"
                };
                twee.AppendLine(line + ",");
            }
            twee.AppendLine(">");
            twee.AppendLine();
        }

        public static List<TweenityNodeModel> ImportFromTwee(string tweeContent)
        {
            var nodes = new List<TweenityNodeModel>();
            var nodeBlocks = tweeContent.Split(new[] { ":: " }, StringSplitOptions.RemoveEmptyEntries);
            var headerRegex = new Regex(@"^(.*?) \[(.*?)\] \{\""position\"":\""(.*?),(.*?)\"".*?\""id\"":\""(.*?)\""\}");

            foreach (var rawBlock in nodeBlocks)
            {
                var lines = rawBlock.Split('\n').ToList();
                if (lines.Count == 0) continue;

                var headerMatch = headerRegex.Match(lines[0]);
                if (!headerMatch.Success) continue;

                string title = headerMatch.Groups[1].Value.Trim();
                string typeStr = headerMatch.Groups[2].Value.Trim();
                float x = float.Parse(headerMatch.Groups[3].Value.Trim());
                float y = float.Parse(headerMatch.Groups[4].Value.Trim());
                string nodeId = headerMatch.Groups[5].Value.Trim();

                Enum.TryParse(typeStr, true, out NodeType type);
                Vector2 position = new Vector2(x, y);

                int atIndex = lines.FindIndex(l => l.Trim() == "@");
                string description = atIndex >= 1
                    ? string.Join("\n", lines.Skip(1).Take(atIndex - 1)).Trim()
                    : "";

                string special1 = "null";
                string special2 = "null";
                string pathJsonLine = "";
                var instructions = new List<ActionInstruction>();

                for (int i = atIndex + 1; i < lines.Count; i++)
                {
                    string line = lines[i].Trim();

                    if (line.StartsWith("- "))
                    {
                        if (special1 == "null") special1 = line.Substring(2).Trim();
                        else if (special2 == "null") special2 = line.Substring(2).Trim();
                    }
                    else if (line.StartsWith("["))
                    {
                        pathJsonLine = line;
                    }
                    else if (line == "<")
                    {
                        i++;
                        while (i < lines.Count && lines[i].Trim() != ">")
                        {
                            var raw = lines[i].Trim().TrimEnd(',');
                            if (raw.StartsWith("Wait("))
                            {
                                var content = raw.Replace("Wait(", "").Replace(")", "");
                                instructions.Add(new ActionInstruction(ActionInstructionType.Wait, "", "", content));
                            }
                            else if (raw.StartsWith("Remind("))
                            {
                                var content = raw.Replace("Remind(", "").Replace(")", "");
                                var parts = content.Split(':');
                                if (parts.Length == 2)
                                    instructions.Add(new ActionInstruction(ActionInstructionType.Remind, parts[0], parts[1]));
                            }
                            i++;
                        }
                    }
                }

                var paths = new List<PathData>();
                var pathRegex = new Regex(@"\{""label"":""(.*?)"",""trigger"":""(.*?)"",""target"":""(.*?)""\}");
                foreach (Match m in pathRegex.Matches(pathJsonLine))
                {
                    paths.Add(new PathData(m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value));
                }

                TweenityNodeModel node = type switch
                {
                    NodeType.Dialogue       => new DialogueNodeModel(title),
                    NodeType.MultipleChoice => new MultipleChoiceNodeModel(title),
                    NodeType.Random         => new RandomNodeModel(title),
                    NodeType.Reminder       => new ReminderNodeModel(title),
                    NodeType.Timeout        => new TimeoutNodeModel(title),
                    NodeType.Start          => new StartNodeModel(title),
                    NodeType.End            => new EndNodeModel(title),
                    _                       => new NoTypeNodeModel(title),
                };

                node.NodeID = nodeId;
                node.Position = position;
                node.Description = description;
                node.Instructions = instructions;

                if (node is DialogueNodeModel d && special1 != "null")
                    d.DialogueText = special1;

                if (node is MultipleChoiceNodeModel mc && special1 != "null")
                    mc.Question = special1;

                if (node is ReminderNodeModel r)
                {
                    if (float.TryParse(special1, out var timer))
                        r.ReminderTimer = timer;

                    r.OutgoingPaths= paths;
                }
                else if (node is TimeoutNodeModel to)
                {
                    if (special1 != "null") to.Condition = special1;
                    if (float.TryParse(special2, out var timer)) to.TimeoutDuration = timer;
                    to.OutgoingPaths = paths;
                }
                else
                {
                    node.OutgoingPaths = paths;
                }

                nodes.Add(node);
            }

            return nodes;
        }
    }
}
