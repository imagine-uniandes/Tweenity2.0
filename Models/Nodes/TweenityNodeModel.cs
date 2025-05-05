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
    public class TweenityNodeModel
    {
        /// <summary>
        /// Unique identifier for the node, used internally to reference and connect nodes.
        /// </summary>
        public string NodeID { get; set; }

        /// <summary>
        /// Display name for the node (used for readability and UI).
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Optional description or main content shown in the editor.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The type of node (e.g., Start, Dialogue, Reminder, etc.).
        /// Determines editor behavior and runtime tagging.
        /// </summary>
        public NodeType Type { get; set; }

        /// <summary>
        /// 2D position of the node in the graph view layout.
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// List of outgoing paths to other nodes.
        /// Each path includes a label, trigger string, and target node reference.
        /// </summary>
        public List<PathData> OutgoingPaths { get; set; }

        /// <summary>
        /// Structured editor instructions for node-specific behavior (e.g., Remind, Wait).
        /// These are not executable yet, but will be translated to runtime actions.
        /// </summary>
        public List<ActionInstruction> Instructions { get; set; }

        /// <summary>
        /// Constructor to create a node with a title and type.
        /// </summary>
        public TweenityNodeModel(string title, NodeType type)
        {
            NodeID = Guid.NewGuid().ToString();
            Title = title;
            Type = type;
            Description = "";
            OutgoingPaths = new List<PathData>();
            Instructions = new List<ActionInstruction>();
            Position = Vector2.zero;
        }

        /// <summary>
        /// Connects this node to another via a labeled path.
        /// Will not add duplicate connections.
        /// </summary>
        public void ConnectTo(string targetNodeID, string label = "Next", string trigger = "")
        {
            if (!IsConnectedTo(targetNodeID))
            {
                OutgoingPaths.Add(new PathData(label, trigger, targetNodeID));
            }
        }

        /// <summary>
        /// Disconnects this node from another by removing its path.
        /// </summary>
        public void DisconnectFrom(string targetNodeID)
        {
            OutgoingPaths.RemoveAll(p => p.TargetNodeID == targetNodeID);
        }

        /// <summary>
        /// Checks if this node is already connected to the specified target.
        /// </summary>
        public bool IsConnectedTo(string targetNodeID)
        {
            return OutgoingPaths.Exists(p => p.TargetNodeID == targetNodeID);
        }

        /// <summary>
        /// Updates the target node of a path.
        /// </summary>
        public void UpdatePathTarget(string oldTargetId, string newTargetId)
        {
            var path = OutgoingPaths.Find(p => p.TargetNodeID == oldTargetId);
            if (path != null)
                path.TargetNodeID = newTargetId;
        }

        /// <summary>
        /// Updates the label of a path to a specific target.
        /// </summary>
        public void UpdatePathLabel(string targetId, string newLabel)
        {
            var path = OutgoingPaths.Find(p => p.TargetNodeID == targetId);
            if (path != null)
                path.Label = newLabel;
        }
    }
}
