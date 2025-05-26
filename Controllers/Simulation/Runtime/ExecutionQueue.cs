// ExecutionQueue: versión robusta y secuencial sin MonoBehaviour
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Simulation.Runtime
{
    public class ExecutionQueue
    {
        private readonly Queue<Func<Task>> _queue = new();
        private CancellationTokenSource _cts;
        private bool _isExecuting;

        /// <summary>
        /// Agrega una tarea al final de la cola.
        /// </summary>
        public void AddLast(Func<Task> action)
        {
            _queue.Enqueue(action);
            StartExecutionIfNeeded();
        }

        /// <summary>
        /// Agrega una tarea al inicio de la cola.
        /// </summary>
        public void AddFirst(Func<Task> action)
        {
            var current = _queue.ToList();
            _queue.Clear();
            _queue.Enqueue(action);
            foreach (var item in current)
                _queue.Enqueue(item);
            StartExecutionIfNeeded();
        }

        /// <summary>
        /// Elimina todas las tareas pendientes y cancela la ejecución actual.
        /// </summary>
        public void Clear()
        {
            _queue.Clear();
            _cts?.Cancel();
            _isExecuting = false;
        }

        /// <summary>
        /// Fuerza la detención inmediata de la ejecución actual.
        /// </summary>
        public void ForceStop()
        {
            _cts?.Cancel();
            _isExecuting = false;
        }

        /// <summary>
        /// Inicia la ejecución si no hay otra en curso.
        /// </summary>
        private void StartExecutionIfNeeded()
        {
            if (!_isExecuting)
            {
                _isExecuting = true;
                _cts = new CancellationTokenSource();
                _ = ExecuteAllAsync(_cts.Token);
            }
        }

        /// <summary>
        /// Ejecuta todas las tareas en la cola de forma secuencial.
        /// </summary>
        private async Task ExecuteAllAsync(CancellationToken token)
        {
            try
            {
                while (_queue.Count > 0 && !token.IsCancellationRequested)
                {
                    var action = _queue.Dequeue();
                    try
                    {
                        await action();
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError("[ExecutionQueue] Error ejecutando acción: " + e.Message);
                    }
                }
            }
            finally
            {
                _isExecuting = false;
            }
        }
        public Task WaitUntilComplete(CancellationToken token)
        {
            return Task.Run(async () =>
            {
                try
                {
                    while (_isExecuting && !token.IsCancellationRequested)
                        await Task.Delay(50, token);
                }
                catch (TaskCanceledException)
                {
                    
                }
            });
        }
    }
}
