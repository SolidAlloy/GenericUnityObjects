namespace GenericUnityObjects.UnityEditorInternals
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using Util;

    internal static class GenericTypeHelper
    {
        private static Type GetGenericType(SerializedProperty property)
        {
            ScriptAttributeUtility.GetFieldInfoFromProperty(property, out Type genericType);
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