namespace GenericUnityObjects.Editor
{
    using UnityEditor;
    using UnityEngine;

#if ! DISABLE_GENERIC_OBJECT_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScriptableObject), true)]
#endif
    public class ScriptableObjectEditor : UnityObjectEditor { }
}