namespace GenericUnityObjects.UnityEditorInternals
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using Util;

    public class GenericObjectInternalEditor : Editor
    {
        private static readonly Dictionary<UnityEngine.Object, string> _targetTitlesCache = new Dictionary<UnityEngine.Object, string>();
        private static readonly Dictionary<Type, string> _typeNamesCache = new Dictionary<Type, string>();

        internal override string targetTitle => GetTitle();

        private string GetTitle()
        {
            Type genericType = target.GetType().BaseType;

            if (genericType?.IsGenericType != true)
                return base.targetTitle;

            return m_Targets.Length == 1 || ! m_AllowMultiObjectAccess
                ? GetOneTitle(genericType)
                : GetMixedTitle(genericType);
        }

        private string GetOneTitle(Type genericType)
        {
            if (_targetTitlesCache.TryGetValue(target, out string title))
                return title;

            string typeName = GetTypeName(genericType);

            // target.name is empty when a new asset is created interactively and has not been named yet.
            if (string.IsNullOrEmpty(target.name))
                return $"({typeName})";

            title = $"{ObjectNames.NicifyVariableName(target.name)} ({typeName})";
            _targetTitlesCache.Add(target, title);
            return title;
        }

        private string GetMixedTitle(Type genericType)
        {
            return $"{m_Targets.Length} objects of type {GetTypeName(genericType)}";
        }

        private static string GetTypeName(Type genericType)
        {
            if (_typeNamesCache.TryGetValue(genericType, out string typeName))
                return typeName;

            typeName = TypeUtility.GetGenericTypeNameWithBrackets(genericType);
            _typeNamesCache.Add(genericType, typeName);
            return typeName;
        }
    }
}