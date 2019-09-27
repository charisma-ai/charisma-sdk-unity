using System.Collections;
using UnityEngine;

namespace CharismaSDK
{
    public class CoroutineConsumer : MonoBehaviour
    {
        public static CoroutineConsumer Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            } else {
                Instance = this;
            }
        }

        public void Consume(IEnumerator routine)
        {
            StartCoroutine(routine);
        }
    }
}
