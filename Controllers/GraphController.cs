using Models;
using Models.Nodes;
using System;
using Views;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Controllers
{
    public class GraphController
    {
        public GraphModel Graph { get; private set; }
        public TweenityGraphView GraphView { get; private set; }

        public GraphController(TweenityGraphView graphView)
        {
            Graph = new GraphModel();
            GraphView = graphView;
        }

        public bool AddNode(TweenityNodeModel node)
        {
            if (Graph.AddNode(node))
            {
                GraphView.AddNodeToView(node);
                return true;
            }
            return false;
        }

        public void RemoveNode(string nodeId)
        {
            Graph.RemoveNode(nodeId);
            GraphView.RemoveNodeFromView(nodeId);
        }

        public TweenityNodeModel GetNode(string nodeId)
        {
            return Graph.GetNode(nodeId);
        }

        public List<TweenityNodeModel> GetAllNodes()
        {
            return Graph.Nodes;
        }
    }
}
