using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharismaSdk
{
    public class MainThreadConsumer : MonoBehaviour
    {
        public static MainThreadConsumer Instance { get; private set; }
        private static readonly Queue<Action> ExecuteOnMainThread = new Queue<Action>();
        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(this.gameObject);
            else
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
                
        }

        private void Update()
        {
            while (ExecuteOnMainThread.Count > 0) 
                ExecuteOnMainThread.Dequeue()?.Invoke();
        }

        public void Consume(IEnumerator routine)
        {
            StartCoroutine(routine);
        }

        public void Enqueue(Action act)
        {
            if(act == null) return;
            
            ExecuteOnMainThread.Enqueue(act);    
        }

        public static void Destroy()
        {
            Destroy(Instance.gameObject);
        }
    }
}
