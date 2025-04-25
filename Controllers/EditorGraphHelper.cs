#if UNITY_EDITOR
using Controllers;
#endif

namespace Tweenity2.EditorHelpers
{
    public static class EditorGraphHelper
    {
        /// <summary>
        /// Intenta centrar la vista en un nodo dado su ID. Solo funciona en el editor.
        /// </summary>
        public static void TryCenterNode(string nodeId)
        {
#if UNITY_EDITOR
            var controller = GraphController.ActiveEditorGraphController;
            controller?.GraphView?.CenterOnNode(nodeId);
#endif
        }
    }
}
