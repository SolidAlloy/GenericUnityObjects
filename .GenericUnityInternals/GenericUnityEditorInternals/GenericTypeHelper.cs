namespace GenericUnityObjects.UnityEditorInternals
{
    using System;
    using UnityEditor;
    using UnityEngine;
    using Util;

    /// <summary>
    /// A class that accumulates methods that allow working with a generic type.
    /// </summary>
    internal static class GenericTypeHelper
    {
        public static Type GetGenericType(SerializedProperty property)
        {
            ScriptAttributeUtility.GetFieldInfoFromProperty(property, out Type genericType);
            return genericType;
        }

        public static string GetNiceTypeName(ObjectFieldHelper fieldHelper)
        {
            Type type = fieldHelper.CurrentTarget is null
                ? fieldHelper.ObjType
                : fieldHelper.CurrentTarget.GetType().BaseType;

            return TypeUtility.GetNiceNameOfGenericType(type);
        }

        public static Type GetConcreteType(Type genericType)
        {
            if (typeof(MonoBehaviour).IsAssignableFrom(genericType))
            {
                return GenericTypesDatabase<MonoBehaviour>.TryGetConcreteType(genericType, out Type concreteType)
                    ? concreteType
                    : genericType;
            }

            if (typeof(ScriptableObject).IsAssignableFrom(genericType))
            {
                return GenericTypesDatabase<ScriptableObject>.TryGetConcreteType(genericType, out Type concreteType)
                    ? concreteType
                    : genericType;
            }

            throw new ArgumentException(
                $"Expected a type derived from MonoBehaviour or ScriptableObject. Got {genericType} instead.");
        }
    }
}