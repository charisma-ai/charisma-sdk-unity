using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CharismaSDK
{
    public class MainThreadDispatcher
    {
        private static MainThreadDispatcher _instance;
        private static MainThreadDispatcherMonoComponent _monoComponent;
        public static MainThreadDispatcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MainThreadDispatcher();
                    var coroutineObject = Object.Instantiate(new GameObject("ExternalCoroutineComponent"));
                    _monoComponent = coroutineObject.AddComponent<MainThreadDispatcherMonoComponent>();
                }

                return _instance;
            }
        }

        public Coroutine Consume(IEnumerator routine)
        {
            return _monoComponent.StartCoroutine(routine);
        }

        public void Consume(Action action)
        {
            _monoComponent.EnqueueMainThreadAction(action);
        }
    }
}
