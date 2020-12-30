namespace GenericUnityObjects.Editor.ScriptableObject
{
    using System;
    using GenericUnityObjects;
    using GenericUnityObjects.Util;
    using SolidUtilities.Editor.Extensions;
    using SolidUtilities.Editor.Helpers;
    using UnityEditor;
    using UnityEngine;
    using Util;

    /// <summary>
    /// The default object field drawn for types derived from <see cref="GenericScriptableObject"/> does not list
    /// available assets in the object picker window. This custom property drawer looks the same but lists the
    /// available assets.
    /// </summary>
    [CustomPropertyDrawer(typeof(GenericScriptableObject), true)]
    internal class GenericSODrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorDrawHelper.InPropertyWrapper(position, label, property, () =>
            {
                Rect posWithoutLabel = EditorGUI.PrefixLabel(position, label);
                Type filterType = GetFilterType(property);
                property.objectReferenceValue = EditorGUI.ObjectField(
                    posWithoutLabel, property.objectReferenceValue, filterType, false);
            });
        }

        private static Type GetFilterType(SerializedProperty property)
        {
            Type objectType = property.GetObjectType();

            if ( ! objectType.IsGenericType)
                return objectType;

            GenericObjectDatabase.TryGetValue(objectType, out Type concreteType);

            return concreteType ?? objectType;
        }
    }
}