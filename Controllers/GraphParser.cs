using System.Text;
using System.Collections.Generic;
using Models.Nodes;
using System.Text.RegularExpressions;
using Models;
using System;
using UnityEngine;

namespace Controllers
{
    public static class GraphParser
    {
        public static string ExportToTwee(List<TweenityNodeModel> nodes)
        {
            StringBuilder twee = new StringBuilder();

            foreach (var node in nodes)
            {
                // Add passage header
                twee.Append(":: ").Append(node.Title);

                // Add tag
                twee.Append(" [").Append(node.Type.ToString().ToLower()).Append("]");

                // Add metadata with position
                string pos = node.Position != null ?
                    $"{node.Position.x},{node.Position.y}" : "0,0";
                twee.Append($" {{\"position\":\"{pos}\",\"size\":\"100,100\"}}");

                // Newline
                twee.AppendLine();

                // Add node description (as body)
                twee.AppendLine(node.Description);

                // Add divider (optional)
                twee.AppendLine("@");

                // Add connections from OutgoingPaths
                foreach (var path in node.OutgoingPaths)
                {
                    var targetNode = nodes.Find(n => n.NodeID == path.TargetNodeID);
                    if (targetNode != null)
                    {
                        twee.Append("[[").Append(path.Label ?? targetNode.Title);

                        if (!string.IsNullOrEmpty(path.Trigger))
                        {
                            twee.Append($":{path.Trigger}");
                        }

                        twee.Append(":" + targetNode.Title).AppendLine("]]");
                    }
                }

                // Add simulator/action blocks (placeholder for now)
                twee.AppendLine("{}");
                twee.AppendLine("<>");
                twee.AppendLine();
            }

            return twee.ToString();
        }

        public static List<TweenityNodeModel> ImportFromTwee(string tweeContent)
        {
            var nodes = new List<TweenityNodeModel>();
            var passageRegex = new Regex(@":: (.*?)\s*(\[(.*?)\])?\s*(\{.*?\})?\s*\n(.*?)(?=(?:^::|\Z))", RegexOptions.Singleline | RegexOptions.Multiline);
            var linkRegex = new Regex(@"\[\[(.*?)(?::(.*?))?(?::(.*?))?\]\]");

            var matches = passageRegex.Matches(tweeContent);
            Dictionary<string, TweenityNodeModel> titleLookup = new();

            // First pass: create all nodes
            foreach (Match match in matches)
            {
                string title = match.Groups[1].Value.Trim();
                string tag = match.Groups[3].Success ? match.Groups[3].Value.Trim() : "notype";
                string metadata = match.Groups[4].Value.Trim();
                string body = match.Groups[5].Value.Trim();

                NodeType nodeType = Enum.TryParse<NodeType>(tag, true, out var parsed) ? parsed : NodeType.NoType;

                TweenityNodeModel node = nodeType switch
                {
                    NodeType.Dialogue => new DialogueNodeModel(title),
                    NodeType.Reminder => new ReminderNodeModel(title),
                    NodeType.MultipleChoice => new MultipleChoiceNodeModel(title),
                    NodeType.Random => new RandomNodeModel(title),
                    NodeType.Start => new StartNodeModel(title),
                    NodeType.End => new EndNodeModel(title),
                    NodeType.Timeout => new TimeoutNodeModel(title),
                    _ => new NoTypeNodeModel(title),
                };

                node.Description = body;

                // Parse position from metadata
                if (!string.IsNullOrEmpty(metadata))
                {
                    var posMatch = Regex.Match(metadata, "\\\"position\\\":\\\"(\\d+),(\\d+)\\\"");
                    if (posMatch.Success && float.TryParse(posMatch.Groups[1].Value, out float x) && float.TryParse(posMatch.Groups[2].Value, out float y))
                    {
                        node.Position = new Vector2(x, y);
                    }
                }

                nodes.Add(node);
                titleLookup[title] = node;
            }

            // Second pass: resolve connections
            foreach (Match match in matches)
            {
                string currentTitle = match.Groups[1].Value.Trim();
                string body = match.Groups[5].Value;

                if (!titleLookup.TryGetValue(currentTitle, out var currentNode))
                    continue;

                var linkMatches = linkRegex.Matches(body);
                foreach (Match link in linkMatches)
                {
                    string label = link.Groups[1].Value.Trim();
                    string trigger = link.Groups[2].Success ? link.Groups[2].Value.Trim() : "";
                    string targetTitle = link.Groups[3].Success ? link.Groups[3].Value.Trim() : label;

                    if (titleLookup.TryGetValue(targetTitle, out var targetNode))
                    {
                        currentNode.OutgoingPaths.Add(new PathData(label, trigger, targetNode.NodeID));
                    }
                }
            }

            return nodes;
        }
    }
}