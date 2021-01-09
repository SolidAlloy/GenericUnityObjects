namespace GenericUnityObjects.Editor.Util
{
    using GenericUnityObjects.Util;
    using UnityEditor;

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