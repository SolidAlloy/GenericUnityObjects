namespace GenericUnityObjects.Editor
{
    using System;
    using JetBrains.Annotations;
    using UnityEditor;
    using UnityEditorInternals;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public static class GenericObjectDrawer
    {
        /// <summary>
        /// Works the same as <see cref="EditorGUI.ObjectField(Rect, SerializedProperty)"/> except that it has support
        /// for generic Unity objects.
        /// </summary>
        /// <param name="rect">Rectangle of the object field.</param>
        /// <param name="property">Property that references a <see cref="UnityEngine.Object"/>.</param>
        [PublicAPI]
        public static void ObjectField(Rect rect, SerializedProperty property)
        {
            if (property.type.Contains("`"))
            {
                EditorGUIHelper.GenericObjectField(rect, property);
            }
            else
            {
                EditorGUI.PropertyField(rect, property);
            }
        }

        [PublicAPI]
        public static Object ObjectField(Rect rect, GUIContent label, Object currentTarget, Type objType,
            bool allowSceneObjects)
        {
            return objType.IsGenericType
                ? EditorGUIHelper.GenericObjectField(rect, label, currentTarget, objType, allowSceneObjects)
                : EditorGUI.ObjectField(rect, label, currentTarget, objType, allowSceneObjects);
        }

        [PublicAPI]
        public static Object ObjectField(string label, Object currentTarget, Type objType,
            bool allowSceneObjects)
        {
            return objType.IsGenericType
                ? EditorGUILayoutHelper.GenericObjectField(label, currentTarget, objType, allowSceneObjects)
                : EditorGUILayout.ObjectField(label, currentTarget, objType, allowSceneObjects);
        }
    }
}