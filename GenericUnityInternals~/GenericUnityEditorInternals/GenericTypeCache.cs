namespace GenericUnityObjects.UnityEditorInternals
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using Util;

    internal static class GenericTypeCache
    {
        private static readonly Dictionary<string, Type> _cachedTypes = new Dictionary<string, Type>();

        private static Type GetGenericType(SerializedProperty property)
        {
            if (_cachedTypes.TryGetValue(property.type, out Type genericType))
                return genericType;

            // The method is apparently pretty performant, but caching types is still better
            ScriptAttributeUtility.GetFieldInfoFromProperty(property, out genericType);
            _cachedTypes.Add(property.type, genericType);
            return genericType;
        }

        public static string GetNiceTypeName(SerializedProperty property)
        {
            Type genericType = GetGenericType(property);
            return TypeUtility.GetGenericTypeNameWithBrackets(genericType);
        }

        public static Type GetConcreteType(Type genericType)
        {
            if (typeof(MonoBehaviour).IsAssignableFrom(genericType))
            {
                return BehavioursDatabase.TryGetConcreteType(genericType, out Type concreteType)
                    ? concreteType
                    : genericType;
            }

            if (typeof(GenericScriptableObject).IsAssignableFrom(genericType))
            {
                return ScriptableObjectsDatabase.TryGetConcreteType(genericType, out Type concreteType)
                    ? concreteType
                    : genericType;
            }

            throw new ArgumentException(
                $"Expected a type derived from MonoBehaviour or GenericScriptableObject. Got {genericType} instead.");
        }
    }
}