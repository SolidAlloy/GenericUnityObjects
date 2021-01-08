namespace GenericUnityObjects.Util
{
    using System.Diagnostics;
    using Debug = UnityEngine.Debug;

    public static class DebugUtility
    {
        [Conditional("GENERIC_UNITY_OBJECTS_DEBUG")]
        public static void Log(string message) => Debug.Log(message);
    }
}