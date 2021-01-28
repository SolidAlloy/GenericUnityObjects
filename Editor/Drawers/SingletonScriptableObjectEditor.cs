namespace GenericUnityObjects.Editor
{
    using GenericUnityObjects.Util;
    using UnityEditor;

    /// <summary>
    /// Serialized fields of SingletonScriptableObjects are not supposed to be changed from inspector.
    /// They are hidden completely from plugin users, and are shown in read-only mode for debugging purposes.
    /// </summary>
    [CustomEditor(typeof(SingletonScriptableObject), true)]
    internal class SingletonScriptableObjectEditor : Editor
    {
        public override void OnInspectorGUI()
        {
#if GENERIC_UNITY_OBJECTS_DEBUG
            using var disabledScope = new EditorGUI.DisabledScope(true);
            DrawDefaultInspector();
#endif
        }
    }
}