using System;
using System.Collections.Generic;
using UnityEngine;

namespace Models.Nodes
{
    /// <summary>
    /// Represents a node in the Tweenity editor graph.
    /// This class stores all the data necessary to visualize, edit, and export a graph node,
    /// including connections, type metadata, instructions, and editor position.
    /// </summary>
    [System.Serializable]
    public class TweenityNodeModel
    {
        [SerializeField] private string nodeID;
        [SerializeField] private string title;
        [SerializeField] private string description;
        [SerializeField] private NodeType type;
        [SerializeField] private Vector2 position;
        [SerializeField] private List<PathData> outgoingPaths = new();
        [SerializeField] private List<ActionInstruction> instructions = new();

        public string NodeID
        {
            get => nodeID;
            set => nodeID = value;
        }

        public string Title
        {
            get => title;
            set => title = value;
        }

        public string Description
        {
            get => description;
            set => description = value;
        }

        public NodeType Type
        {
            get => type;
            set => type = value;
        }

        public Vector2 Position
        {
            get => position;
            set => position = value;
        }

        public List<PathData> OutgoingPaths => outgoingPaths;

        public List<ActionInstruction> Instructions
        {
            get => instructions;
            set => instructions = value ?? new List<ActionInstruction>();
        }

        public TweenityNodeModel(string title, NodeType type)
        {
            this.nodeID = Guid.NewGuid().ToString();
            this.title = title;
            this.type = type;
            this.description = "";
            this.position = Vector2.zero;
        }

        public void ConnectTo(string targetNodeID, string label = "Next", string trigger = "")
        {
            if (!IsConnectedTo(targetNodeID))
            {
                outgoingPaths.Add(new PathData(label, trigger, targetNodeID));
            }
        }

        public void DisconnectFrom(string targetNodeID)
        {
            outgoingPaths.RemoveAll(p => p.TargetNodeID == targetNodeID);
        }

        public bool IsConnectedTo(string targetNodeID)
        {
            return outgoingPaths.Exists(p => p.TargetNodeID == targetNodeID);
        }

        public void UpdatePathTarget(string oldTargetId, string newTargetId)
        {
            var path = outgoingPaths.Find(p => p.TargetNodeID == oldTargetId);
            if (path != null)
                path.TargetNodeID = newTargetId;
        }

        public void UpdatePathLabel(string targetId, string newLabel)
        {
            var path = outgoingPaths.Find(p => p.TargetNodeID == targetId);
            if (path != null)
                path.Label = newLabel;
        }
    }
}
