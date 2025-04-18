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
                string pos = $"{node.Position.x},{node.Position.y}";
                string id = node.NodeID;
                twee.Append(":: ").Append(node.Title)
                    .Append(" [").Append(node.Type.ToString().ToLower()).Append("] ")
                    .Append($"{{\"position\":\"{pos}\",\"id\":\"{id}\"}}")
                    .AppendLine();

                twee.AppendLine(node.Description ?? "");
                twee.AppendLine("@");

                string specialField1 = "null";
                switch (node)
                {
                    case MultipleChoiceNodeModel multi:
                        specialField1 = multi.Question;
                        break;
                    case DialogueNodeModel dialogue:
                        specialField1 = dialogue.DialogueText;
                        break;
                    case ReminderNodeModel reminder:
                        specialField1 = reminder.ReminderText;
                        break;
                    case TimeoutNodeModel timeout:
                        specialField1 = timeout.Condition;
                        break;
                }
                twee.AppendLine($"- {specialField1 ?? "null"}");

                string specialField2 = "null";
                switch (node)
                {
                    case ReminderNodeModel reminder:
                        specialField2 = reminder.ReminderTimer.ToString();
                        break;
                    case TimeoutNodeModel timeout:
                        specialField2 = timeout.TimeoutDuration.ToString();
                        break;
                }
                twee.AppendLine($"- {specialField2}");

                var pathJsons = node.OutgoingPaths.Select(path =>
                    $"{{\"label\":\"{path.Label}\",\"trigger\":\"{path.Trigger}\",\"target\":\"{path.TargetNodeID}\"}}"
                );
                twee.AppendLine("[" + string.Join(",", pathJsons) + "]");
                twee.AppendLine();
            }

            return twee.ToString();
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
                string pathJson = "[]";

                for (int i = atIndex + 1; i < lines.Count; i++)
                {
                    string line = lines[i].Trim();
                    if (line.StartsWith("- "))
                    {
                        if (special1 == "null") special1 = line.Substring(2).Trim();
                        else if (special2 == "null") special2 = line.Substring(2).Trim();
                    }
                    else if (line.StartsWith("{") || line.StartsWith("["))
                    {
                        pathJson = string.Join("\n", lines.Skip(i)).Trim();
                        break;
                    }
                    else
                    {
                        if (special2 != "null") special2 += "\n" + line;
                        else if (special1 != "null") special1 += "\n" + line;
                    }
                }

                var paths = new List<PathData>();
                var pathRegex = new Regex(@"\{""label"":""(.*?)"",""trigger"":""(.*?)"",""target"":""(.*?)""\}");
                foreach (Match m in pathRegex.Matches(pathJson))
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
                node.OutgoingPaths = paths;

                if (node is DialogueNodeModel d && special1 != "null")
                    d.DialogueText = special1;

                if (node is MultipleChoiceNodeModel mc && special1 != "null")
                    mc.Question = special1;

                if (node is ReminderNodeModel r)
                {
                    if (special1 != "null") r.ReminderText = special1;
                    if (float.TryParse(special2, out var timer)) r.ReminderTimer = timer;
                }

                if (node is TimeoutNodeModel to)
                {
                    if (special1 != "null") to.Condition = special1;
                    if (float.TryParse(special2, out var timer)) to.TimeoutDuration = timer;
                }

                nodes.Add(node);
            }

            return nodes;
        }
    }
}
