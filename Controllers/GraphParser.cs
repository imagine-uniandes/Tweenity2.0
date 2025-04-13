using System.Text;
using System.Collections.Generic;
using Models.Nodes;
using System.Text.RegularExpressions;
using Models;
using System;
using UnityEngine;
using System.Linq;

// Schema
// :: Title [type] {"position":"x,y","id":"node-id"}
// <descripción>                   ← Texto libre opcional (puede ser vacío)
// @
// <campo especial 1 o null>      ← e.g. DialogueText, Question, ReminderText, TimeoutCondition
// <campo especial 2 o null>      ← e.g. ReminderTimer, TimeoutDuration
// [{"label":"...", "trigger":"...", "target":"..."}, ...] ← lista JSON de paths salientes



namespace Controllers
{
    public static class GraphParser
    {
        public static string ExportToTwee(List<TweenityNodeModel> nodes)
        {
            StringBuilder twee = new StringBuilder();

            foreach (var node in nodes)
            {
                // Header: Title, Type, Metadata (position + id)
                string pos = $"{node.Position.x},{node.Position.y}";
                string id = node.NodeID;
                twee.Append(":: ").Append(node.Title)
                    .Append(" [").Append(node.Type.ToString().ToLower()).Append("]")
                    .Append($" {{\"position\":\"{pos}\",\"id\":\"{id}\"}}")
                    .AppendLine();

                // Body text (description)
                twee.AppendLine(node.Description ?? "");

                // Separator
                twee.AppendLine("@");

                // Special field 1 (question, dialogue, reminder text, timeout condition)
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
                twee.AppendLine(specialField1 ?? "null");

                // Special field 2 (reminder timer or timeout duration)
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
                twee.AppendLine(specialField2);

                // Outgoing paths as JSON list
                var pathJsons = node.OutgoingPaths.Select(path =>
                    $"{{\"label\":\"{path.Label}\",\"trigger\":\"{path.Trigger}\",\"target\":\"{path.TargetNodeID}\"}}"
                );
                twee.AppendLine("[" + string.Join(",", pathJsons) + "]");

                // Separate nodes
                twee.AppendLine();
            }

            return twee.ToString();
        }


        public static List<TweenityNodeModel> ImportFromTwee(string tweeContent)
        {
            var nodes = new List<TweenityNodeModel>();
            var passageRegex = new Regex(@":: (.*?) \[(.*?)\] \{\""position\"":\""(.*?),(.*?)\"".*?\""id\"":\""(.*?)\""\}", RegexOptions.Multiline);
            var allPassages = tweeContent.Split(new[] { ":: " }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var rawPassage in allPassages)
            {
                var lines = rawPassage.Split('\n').ToList();
                if (lines.Count == 0) continue;

                // Parse header
                var headerMatch = passageRegex.Match(":: " + lines[0]);
                if (!headerMatch.Success) continue;

                string title = headerMatch.Groups[1].Value.Trim();
                string typeStr = headerMatch.Groups[2].Value.Trim();
                string x = headerMatch.Groups[3].Value.Trim();
                string y = headerMatch.Groups[4].Value.Trim();
                string nodeId = headerMatch.Groups[5].Value.Trim();

                Enum.TryParse(typeStr, true, out NodeType type);
                Vector2 pos = new Vector2(float.Parse(x), float.Parse(y));

                // Break passage into blocks
                var bodySplit = rawPassage.Substring(lines[0].Length).Split(new[] { "\n@" }, StringSplitOptions.None);
                string description = bodySplit.Length > 0 ? bodySplit[0].Trim() : "";
                string special1 = "null";
                string special2 = "null";
                string pathJson = "[]";

                if (bodySplit.Length > 1)
                {
                    var afterAtLines = bodySplit[1].Split('\n');

                    if (afterAtLines.Length > 0)
                        special1 = afterAtLines[0].Trim();

                    if (afterAtLines.Length > 1)
                        special2 = afterAtLines[1].Trim();

                    if (afterAtLines.Length > 2)
                        pathJson = string.Join("\n", afterAtLines.Skip(2)).Trim();
                }

                // Parse paths
                var paths = new List<PathData>();
                var pathRegex = new Regex(@"\{""label"":""(.*?)"",""trigger"":""(.*?)"",""target"":""(.*?)""\}");
                foreach (Match m in pathRegex.Matches(pathJson))
                {
                    paths.Add(new PathData(m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value));
                }

                // Create model
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
                node.Position = pos;
                node.Description = description;
                node.OutgoingPaths = paths;

                // Parse special fields
                if (node is DialogueNodeModel d && special1 != "null") d.DialogueText = special1;
                if (node is MultipleChoiceNodeModel mc && special1 != "null") mc.Question = special1;
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
