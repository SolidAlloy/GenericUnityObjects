namespace GenericUnityObjects.Util
{
    using System.Diagnostics;
    using Debug = UnityEngine.Debug;

    /// <summary>
    /// Holder of methods that are fired only if GENERIC_UNITY_OBJECTS_DEBUG is defined.
    /// </summary>
    public static class DebugUtility
    {
        [Conditional("GENERIC_UNITY_OBJECTS_DEBUG")]
        public static void Log(string message) => Debug.Log(message);
    }
}