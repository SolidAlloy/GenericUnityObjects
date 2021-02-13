﻿namespace GenericUnityObjects.UnityEditorInternals
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

        public static string GetNiceTypeName(Type genericType) => TypeUtility.GetNiceNameOfGenericType(genericType);

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