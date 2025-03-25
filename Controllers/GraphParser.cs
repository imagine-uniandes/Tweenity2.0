using System.Text;
using System.Collections.Generic;
using Models.Nodes;
using System.Text.RegularExpressions;

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

                // Add metadata placeholder
                twee.Append(" {\"position\":\"0,0\",\"size\":\"100,100\"}");

                // Newline
                twee.AppendLine();

                // Add node description (as body)
                twee.AppendLine(node.Description);

                // Add divider (optional)
                twee.AppendLine("@");

                // Add connections
                foreach (var connectedId in node.ConnectedNodes)
                {
                    var connectedNode = nodes.Find(n => n.NodeID == connectedId);
                    if (connectedNode != null)
                    {
                        twee.Append("[[").Append(connectedNode.Title).Append("]]").AppendLine();
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

            // Match each passage block
            var passageRegex = new Regex(@":: (.*?)\s*(\[(.*?)\])?\s*(\{.*?\})?\s*\n(.*?)(?=(?:^::|\Z))", RegexOptions.Singleline | RegexOptions.Multiline);
            var linkRegex = new Regex(@"\[\[(.*?)\]\]");

            var matches = passageRegex.Matches(tweeContent);

            Dictionary<string, TweenityNodeModel> titleLookup = new();

            // First pass: create all nodes
            foreach (Match match in matches)
            {
                string title = match.Groups[1].Value.Trim();
                string tag = match.Groups[3].Success ? match.Groups[3].Value.Trim() : "notype";
                string body = match.Groups[5].Value.Trim();

                // Map tag to enum
                NodeType nodeType = Enum.TryParse<NodeType>(tag, true, out var parsed) ? parsed : NodeType.NoType;

                var node = new TweenityNodeModel(title, nodeType)
                {
                    Description = body
                };

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
                    string linkedTitle = link.Groups[1].Value.Trim();
                    if (titleLookup.TryGetValue(linkedTitle, out var linkedNode))
                    {
                        currentNode.ConnectedNodes.Add(linkedNode.NodeID);
                    }
                }
            }

            return nodes;
        }
    }
}
