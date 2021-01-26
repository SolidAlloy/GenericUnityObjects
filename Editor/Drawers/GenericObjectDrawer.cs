namespace GenericUnityObjects.Editor
{
    using JetBrains.Annotations;
    using UnityEditor;
    using UnityEditorInternals;
    using UnityEngine;

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
    }
}