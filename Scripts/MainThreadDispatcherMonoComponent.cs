using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    /// <summary>
    /// Coroutines run on this object when called from external scripts through
    /// </summary>
    public class MainThreadDispatcherMonoComponent : MonoBehaviour
    {
        private readonly Queue<Action> ExecuteOnMainThread = new();

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            while (ExecuteOnMainThread.Count > 0)
            {
                ExecuteOnMainThread.Dequeue()?.Invoke();
            }
        }

        public Coroutine ExternalStartCoroutine(IEnumerator routine)
        {
            return StartCoroutine(routine);
        }

        public void ExternalStopCoroutine(Coroutine coroutine)
        {
            StopCoroutine(coroutine);
        }

        public void EnqueueMainThreadAction(Action action)
        {
            if (action == null)
            {
                return;
            }

            ExecuteOnMainThread.Enqueue(action);
        }
    }
}
