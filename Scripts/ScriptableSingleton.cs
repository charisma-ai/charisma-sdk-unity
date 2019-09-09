using System.Linq;
using UnityEngine;

namespace CharismaSDK
{
    public abstract class ScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;
                
                var instance = UnityEngine.Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();
                _instance = instance;

                return _instance;
            }
        }
    }
}