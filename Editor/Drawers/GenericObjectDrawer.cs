namespace GenericUnityObjects.Editor
{
    using System;
    using JetBrains.Annotations;
    using UnityEditor;
    using UnityEditorInternals;
    using UnityEngine;
    using Object = UnityEngine.Object;

    /// <summary>
    /// A class that holds overriden versions of EditorGUI and EditorGUILayout ObjectField methods that support generic types.
    /// </summary>
    public static class GenericObjectDrawer
    {
        /// <summary>
        /// Works the same way as <see cref="EditorGUI.ObjectField(Rect, SerializedProperty)"/> except that it has
        /// support for generic types.
        /// </summary>
        /// <param name="rect">Rectangle of the object field.</param>
        /// <param name="property">Property that references a <see cref="UnityEngine.Object"/>.</param>
        /// <param name="label">Label in front of the field, or <c>null</c> if no label should be set.</param>
        [PublicAPI]
        public static void ObjectField(Rect rect, SerializedProperty property, GUIContent label = null)
        {
            if (property.type.Contains("`"))
            {
                EditorGUIHelper.GenericObjectField(rect, property, label);
            }
            else
            {
                EditorGUI.PropertyField(rect, property, label);
            }
        }

        /// <summary>
        /// Works the same way as <see cref="EditorGUI.ObjectField(Rect, GUIContent, Object, Type, bool)"/> except
        /// that it has support for generic types.
        /// </summary>
        /// <param name="rect">Rectangle on the screen to use for the field.</param>
        /// <param name="label">Optional label in front of the field.</param>
        /// <param name="currentTarget">The object the field shows.</param>
        /// <param name="objType">The type of the objects that can be assigned.</param>
        /// <param name="allowSceneObjects">Allow assigning Scene objects. See Description for more info.</param>
        /// <returns>A new object assigned to the field.</returns>
        [PublicAPI]
        public static Object ObjectField(Rect rect, GUIContent label, Object currentTarget, Type objType,
            bool allowSceneObjects)
        {
            if (objType.IsGenericType || IsTargetGeneric(currentTarget))
                return EditorGUIHelper.GenericObjectField(rect, label, currentTarget, objType, allowSceneObjects);

            return EditorGUI.ObjectField(rect, label ?? GUIContent.none, currentTarget, objType, allowSceneObjects);
        }

        /// <summary>
        /// Works the same way as <see cref="EditorGUILayout.ObjectField(string, Object, Type, bool, GUILayoutOption[])"/>
        /// except that it has support for generic types.
        /// </summary>
        /// <param name="label">Optional label in front of the field.</param>
        /// <param name="currentTarget">The object the field shows.</param>
        /// <param name="objType">The type of the objects that can be assigned.</param>
        /// <param name="allowSceneObjects">Allow assigning Scene objects. See Description for more info.</param>
        /// <returns>A new object assigned to the field.</returns>
        [PublicAPI]
        public static Object ObjectField(string label, Object currentTarget, Type objType,
            bool allowSceneObjects)
        {
            if (objType.IsGenericType || IsTargetGeneric(currentTarget))
            {
                return EditorGUILayoutHelper.GenericObjectField(label, currentTarget, objType, allowSceneObjects);
            }

            return EditorGUILayout.ObjectField(label, currentTarget, objType, allowSceneObjects);
        }

        private static bool IsTargetGeneric(Object target)
        {
            if (target == null)
                return false;

            var targetBaseType = target.GetType().BaseType;

            if (targetBaseType == null)
                return false;

            return targetBaseType.IsGenericType;
        }
    }
}