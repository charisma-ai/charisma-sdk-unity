using UnityEngine;

namespace CharismaSDK
{
    public static class CharismaLogger
    {
        public static bool IsActive { get; set; }

        public static void Log(string message)
        {
            // TODO: Add filtering of messages
            // TODO: Add message types

            if (IsActive)
            {
                Debug.Log("CharismaLogger: " + message);
            }
        }
    }
}
