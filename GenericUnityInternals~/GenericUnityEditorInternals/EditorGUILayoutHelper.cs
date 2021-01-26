namespace GenericUnityObjects.UnityEditorInternals
{
    using System;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public static class EditorGUILayoutHelper
    {
        public static Object GenericObjectField(string label, Object oldTarget, Type objType, bool allowSceneObjects)
        {
            EditorGUILayout.s_LastRect = EditorGUILayout.GetControlRect(
                true,
                EditorGUI.kSingleLineHeight,
                (GUILayoutOption[]) null);

            return EditorGUIHelper.GenericObjectField(
                EditorGUILayout.s_LastRect,
                EditorGUIUtility.TempContent(label),
                oldTarget,
                objType,
                allowSceneObjects);
        }
    }
}