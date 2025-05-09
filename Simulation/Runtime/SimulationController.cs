// ACTUALIZADO: usa TweenityNodeModel directamente, navegación por trigger o tipo
using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using Models;
using Models.Nodes;
using Views;
using Controllers;
#endif

namespace Simulation.Runtime
{
    public class SimulationController
    {
        public bool debugCarga = false;
        public bool debugLectura = false;

        private List<TweenityNodeModel> allNodes;
        private TweenityNodeModel curNode;

        private ExecutionQueue _executionQueue;
        private CancellationTokenSource tokenSource = new();
        private TaskCompletionSource<bool> userActionAwaiter;

        public UnityEvent<TweenityNodeModel> onEnteredNode = new();

#if UNITY_EDITOR
        private TweenityGraphView graphView;
        public void SetGraphView(TweenityGraphView view)
        {
            graphView = view;
        }
#endif

        /// <summary>
        /// Retorna el nodo actual en ejecución.
        /// </summary>
        public TweenityNodeModel GetCurrentNode() => curNode;

        private void PrintOnDebug(string msg)
        {
            if (debugLectura)
                Debug.Log("[Simulation] " + msg);
        }

        /// <summary>
        /// Inicializa la simulación cargando el grafo y arrancando desde el nodo Start.
        /// </summary>
        public void SetSimulationFromGraph(GraphModel model)
        {
            Debug.Log("🛠 [SimulationController] Using graph model directly...");
            allNodes = model.Nodes;
            _executionQueue = new ExecutionQueue();

            curNode = allNodes.FirstOrDefault(n => n.Type == NodeType.Start);

            if (curNode == null)
            {
                Debug.LogError("❌ Start node not found.");
                return;
            }

#if UNITY_EDITOR
            EditorApplication.delayCall += () =>
            {
                EditorApplication.delayCall += () => EnterNode(curNode);
            };
#else
            EnterNode(curNode);
#endif
        }

        /// <summary>
        /// Devuelve la lista de paths disponibles desde el nodo actual.
        /// </summary>
        public List<PathData> GetCurrentResponses() => curNode.OutgoingPaths;

        /// <summary>
        /// Carga el nodo especificado, reinicia la cola de ejecución e inicia las instrucciones.
        /// </summary>
        private async void EnterNode(TweenityNodeModel node)
        {
            Debug.Log($"➡️ [Simulation] EnterNode: {node.NodeID} [{node.Type}]");

            tokenSource?.Cancel();
            tokenSource = new CancellationTokenSource();
            _executionQueue.Clear();
            userActionAwaiter = null;
            curNode = node;

        #if UNITY_EDITOR
            graphView?.CenterOnNode(node.NodeID);
        #endif

            onEnteredNode?.Invoke(node);

            // Ejecutar todas las instrucciones en orden
            foreach (var instr in node.Instructions)
            {
                _executionQueue.AddLast(() => ExecuteInstruction(instr));
            }

            // Si el nodo requiere esperar input del usuario, forzamos AwaitAction al final
            if (node.Type == NodeType.Reminder || node.Type == NodeType.Timeout ||
                node.Type == NodeType.MultipleChoice || node.Type == NodeType.Dialogue)
            {
                var awaitInstruction = new ActionInstruction(ActionInstructionType.AwaitAction, "", "", "");
                _executionQueue.AddLast(() => ExecuteInstruction(awaitInstruction));
            }

            // Auto-avanza si solo hay un path y no requiere trigger.
            if (!node.OutgoingPaths.Any(p => !string.IsNullOrEmpty(p.Trigger)) && node.OutgoingPaths.Count == 1)
            {
                Debug.Log("📤 [Auto] Advancing to single response...");
                try { NavigateFirstAvailable(); }
                catch (System.Exception e) { Debug.LogError("❌ Auto-advance failed: " + e.Message); }
            }
        }

        /// <summary>
        /// Ejecuta una instrucción del nodo. Puede ser Wait, Action, Remind o AwaitAction.
        /// </summary>
        private async Task ExecuteInstruction(ActionInstruction instr)
        {
            switch (instr.Type)
            {
                case ActionInstructionType.Wait:
                    if (float.TryParse(instr.Params, out var delay))
                        await Task.Delay((int)(delay * 1000));
                    break;

                case ActionInstructionType.Action:
                case ActionInstructionType.Remind:
                    var obj = GameObject.Find(instr.ObjectName);
                    if (obj != null)
                    {
                        var components = obj.GetComponents<MonoBehaviour>();
                        foreach (var comp in components)
                        {
                            var method = comp.GetType().GetMethod(instr.MethodName);
                            if (method != null && method.GetParameters().Length == 0)
                            {
                                method.Invoke(comp, null);
                                break;
                            }
                        }
                    }
                    break;

                case ActionInstructionType.AwaitAction:
                    Debug.Log("⏸ Awaiting user action...");
                    userActionAwaiter = new TaskCompletionSource<bool>();
                    await userActionAwaiter.Task;
                    break;
            }
        }

        /// <summary>
        /// Inicia un temporizador que, si no es cancelado, redirige al path de fallo por timeout.
        /// </summary>
        private async Task TimeoutAfterDelay(float seconds, CancellationToken token)
        {
            try
            {
                await Task.Delay((int)(seconds * 1000), token);
                Debug.Log("⏰ Timeout fired.");
                _executionQueue.ForceStop();
                NavigateTimeoutFailure();
            }
            catch { Debug.Log("⚠️ Timeout cancelled."); }
        }

        /// <summary>
        /// Recibe acciones desde el exterior (por ejemplo: un clic en el mundo)
        /// y verifica si hay un trigger que coincida. Si lo hay, navega.
        /// </summary>
        public void VerifyUserAction(ActionInstruction received)
        {
            if (received == null || curNode == null)
            {
                Debug.LogWarning("❌ [VerifyUserAction] Received null action or no current node.");
                return;
            }

            var trigger = $"{received.ObjectName}:{received.MethodName}";
            Debug.Log($"📥 [VerifyUserAction] Received trigger: {trigger}");

            try
            {
                var targetId = GetTriggerTargetNode(trigger);
                _executionQueue.ForceStop();
                userActionAwaiter?.TrySetResult(true);
                NavigateToNodeID(targetId);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"❌ [VerifyUserAction] {e.Message}");
            }
        }

        /// <summary>
        /// Devuelve el ID del nodo de destino asociado a un trigger.
        /// Lanza excepción si no se encuentra.
        /// </summary>
        public string GetTriggerTargetNode(string trigger)
        {
            var match = curNode.OutgoingPaths.FirstOrDefault(p => p.Trigger == trigger);
            if (match == null)
                throw new System.Exception($"No trigger match for '{trigger}' in node {curNode.NodeID}.");
            return match.TargetNodeID;
        }

        /// <summary>
        /// Navega a un nodo específico por su ID.
        /// </summary>
        private void NavigateToNodeID(string nodeId)
        {
            var nextNode = allNodes.FirstOrDefault(n => n.NodeID == nodeId);
            if (nextNode == null)
                throw new System.Exception($"❌ Cannot navigate: node '{nodeId}' not found.");
            EnterNode(nextNode);
        }

        /// <summary>
        /// Navega al path índice 0 del nodo actual (usado como Timeout Failure).
        /// </summary>
        private void NavigateTimeoutFailure()
        {
            var nodeId = curNode.OutgoingPaths[0].TargetNodeID;
            NavigateToNodeID(nodeId);
        }

        /// <summary>
        /// Navega al path índice 1 del nodo actual (usado como Timeout Success).
        /// </summary>
        private void NavigateTimeoutSuccess()
        {
            var nodeId = curNode.OutgoingPaths[1].TargetNodeID;
            NavigateToNodeID(nodeId);
        }

        /// <summary>
        /// Navega al primer path disponible con TargetNodeID no vacío.
        /// </summary>
        private void NavigateFirstAvailable()
        {
            var valid = curNode.OutgoingPaths.FirstOrDefault(p => !string.IsNullOrEmpty(p.TargetNodeID));
            if (valid == null)
                throw new System.Exception($"❌ No available path from node {curNode.NodeID}.");
            NavigateToNodeID(valid.TargetNodeID);
        }
    }
}
