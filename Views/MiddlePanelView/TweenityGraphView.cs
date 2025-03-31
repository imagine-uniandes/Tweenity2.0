using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Models.Nodes;
using System.Linq;
using System;
using Views.MiddlePanel;
using Models;

namespace Views
{
    public class TweenityGraphView : GraphView
    {
        public Action<TweenityNodeModel> OnNodeSelected;
        private GridBackground gridBackground;

        public TweenityGraphView()
        {
            this.style.flexGrow = 1;
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            gridBackground = new GridBackground();
            this.Insert(0, gridBackground);
        }

        public void RenderNode(TweenityNodeModel nodeModel)
        {
            TweenityNode visualNode;

            switch (nodeModel.Type)
            {
                case NodeType.Dialogue:
                    visualNode = new DialogueNode(nodeModel.NodeID);
                    break;
                default:
                    visualNode = new TweenityNode(nodeModel.NodeID);
                    break;
            }

            visualNode.NodeModel = nodeModel;
            visualNode.UpdateFromModel(); // 🔁 ya no seteamos manualmente title aquí
            visualNode.userData = nodeModel;

            visualNode.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == 0)
                {
                    OnNodeSelected?.Invoke(nodeModel);
                }
            });

            visualNode.SetPosition(new Rect(200, 200, 150, 200));
            AddElement(visualNode);
        }

        public void RemoveNodeFromView(string nodeId)
        {
            var target = this.Children()
                .OfType<TweenityNode>()
                .FirstOrDefault(n => n.NodeID == nodeId);

            if (target != null)
            {
                RemoveElement(target);
            }
        }

        public void UpdateNodeTitle(string nodeId, string newTitle)
        {
            var target = this.Children()
                .OfType<TweenityNode>()
                .FirstOrDefault(n => n.NodeID == nodeId);

            if (target != null)
            {
                target.title = newTitle;
            }
        }

        public void RefreshNodeVisual(string nodeId)
        {
            var target = this.Children()
                .OfType<TweenityNode>()
                .FirstOrDefault(n => n.NodeID == nodeId);

            if (target != null && target.NodeModel != null)
            {
                target.UpdateFromModel(); // ✅ refresca toda la UI desde el modelo
                target.RefreshExpandedState();
                target.RefreshPorts();
            }
        }

        public void ToggleGridVisibility()
        {
            gridBackground.visible = !gridBackground.visible;
        }

        public void ZoomIn()
        {
            this.transform.scale *= 1.1f;
        }

        public void ZoomOut()
        {
            this.transform.scale *= 0.9f;
        }

        public void ResetView()
        {
            this.transform.position = Vector3.zero;
            this.transform.scale = Vector3.one;
        }
    }
}
