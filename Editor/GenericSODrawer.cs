namespace GenericScriptableObjects.Editor
{
    using System;
    using System.Collections.Generic;
    using SolidUtilities.Editor.Extensions;
    using SolidUtilities.Editor.Helpers;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// The default object field drawn for types derived from <see cref="GenericScriptableObject"/> does not list
    /// available assets in the object picker window. This custom property drawer looks the same but lists the
    /// available assets.
    /// </summary>
    [CustomPropertyDrawer(typeof(GenericScriptableObject), true)]
    public class GenericSODrawer : PropertyDrawer
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

            TypeCache.TryGetValue(objectType, out Type concreteType);
            return concreteType ?? objectType;
        }
    }

    internal static class TypeCache
    {
        private static readonly Dictionary<Type, Type> Dict = new Dictionary<Type, Type>();

        public static void TryGetValue(Type key, out Type value)
        {
            // GenericSODatabase is not going to get a new type at runtime, so it is wise to cache its value once.
            // Calling GenericSODatabase.TryGetValue is apparently a tremendously heavy operation that takes 200-800 ms.
            // That's because the TypeReference constructor takes about 80 ms to find some GUIDs in AssetDatabase.
            // TODO: 1. Optimize the TypeReference constructor.
            // TODO: 2. Make GenericSODrawer's private dictionaries use System.Type instead of TypeReference.
            
            if (Dict.ContainsKey(key))
            {
                value = Dict[key];
                return;
            }

            GenericSODatabase.TryGetValue(key, out Type rawValue);
            Dict.Add(key, rawValue);
            value = rawValue;
        }
    }
}