using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Models;
using Models.Nodes;
using Views.MiddlePanel;

namespace Views{

    public class TweenityGraphView : GraphView
    {
        public TweenityGraphView()
        {
            this.style.flexGrow = 1;
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.Insert(0, new GridBackground());
        }

        public void AddNodeToView(TweenityNodeModel nodeModel)
        {
            var node = new TweenityNode(nodeModel.NodeID)
            {
                title = nodeModel.Title
            };
            node.SetPosition(new Rect(200, 200, 150, 200)); // Default position
            AddElement(node);
        }

        public void RemoveNodeFromView(string nodeId)
        {
            foreach (var element in this.Children())
            {
                if (element is TweenityNode node && node.NodeID == nodeId)
                {
                    RemoveElement(node);
                    break;
                }
            }
        }
    }
}
