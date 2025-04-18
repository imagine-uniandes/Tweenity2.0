using System;
using System.Collections.Generic;
using UnityEngine;

namespace Models.Nodes
{
    public class TweenityNodeModel
    {
        public string NodeID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public NodeType Type { get; set; }

        public Vector2 Position { get; set; }

        public List<PathData> OutgoingPaths { get; set; }

        public List<string> RuntimeInstructions { get; set; }

        public TweenityNodeModel(string title, NodeType type)
        {
            NodeID = Guid.NewGuid().ToString();
            Title = title;
            Type = type;
            Description = "";
            OutgoingPaths = new List<PathData>();
            Position = Vector2.zero;
            RuntimeInstructions = new List<string>();
        }

        public void ConnectTo(string targetNodeID, string label = "Next", string trigger = "")
        {
            if (!IsConnectedTo(targetNodeID))
            {
                OutgoingPaths.Add(new PathData(label, trigger, targetNodeID));
            }
        }

        public void DisconnectFrom(string targetNodeID)
        {
            OutgoingPaths.RemoveAll(p => p.TargetNodeID == targetNodeID);
        }

        public bool IsConnectedTo(string targetNodeID)
        {
            return OutgoingPaths.Exists(p => p.TargetNodeID == targetNodeID);
        }

        public void UpdatePathTarget(string oldTargetId, string newTargetId)
        {
            var path = OutgoingPaths.Find(p => p.TargetNodeID == oldTargetId);
            if (path != null)
                path.TargetNodeID = newTargetId;
        }

        public void UpdatePathLabel(string targetId, string newLabel)
        {
            var path = OutgoingPaths.Find(p => p.TargetNodeID == targetId);
            if (path != null)
                path.Label = newLabel;
        }
    }
}
