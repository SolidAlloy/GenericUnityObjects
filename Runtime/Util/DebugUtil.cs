namespace GenericUnityObjects.Util
{
    using UnityEngine;

    public static class DebugUtil
    {
        public static void Log(string message)
        {
            if (Config.Debug)
                Debug.Log(message);
        }
    }
}