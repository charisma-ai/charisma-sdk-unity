using UnityEngine;

namespace CharismaSDK
{
    public static class Logger
    {
        private static UnityEngine.Logger logger = new UnityEngine.Logger(Debug.unityLogger.logHandler);

        public static bool logEnabled
        {
            get { return logger.logEnabled; }
            set { logger.logEnabled = value; }
        }

        public static void Log(object message)
        {
            logger.Log("Charisma", message);
        }

        public static void LogError(object message)
        {
            logger.LogError("Charisma", message);
        }

        public static void LogWarning(object message)
        {
            logger.LogWarning("Charisma", message);
        }
    }
}
