namespace GenericUnityObjects.UnityEditorInternals
{
    using System;
    using System.Collections.Generic;
    using SolidUtilities.Helpers;
    using UnityEditor;
    using Util;
    using Object = UnityEngine.Object;

    /// <summary>
    /// An extension of Editor that changes name of <see cref="GenericScriptableObject"/> assets in the Inspector header.
    /// For all other assets, it draws header like before.
    /// </summary>
    public class GenericHeaderEditor : Editor
    {
        private static readonly Dictionary<TargetInfo, string> _targetTitlesCache = new Dictionary<TargetInfo, string>();
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
            var targetInfo = new TargetInfo(target);

            if (_targetTitlesCache.TryGetValue(targetInfo, out string title))
                return title;

            string typeName = GetTypeName(genericType);

            // target.name is empty when a new asset is created interactively and has not been named yet.
            if (string.IsNullOrEmpty(target.name))
                return $"({typeName})";

            title = $"{ObjectNames.NicifyVariableName(target.name)} ({typeName})";
            _targetTitlesCache.Add(targetInfo, title);
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

            typeName = TypeUtility.GetNiceNameOfGenericType(genericType);
            _typeNamesCache.Add(genericType, typeName);
            return typeName;
        }
    }

    internal readonly struct TargetInfo : IEquatable<TargetInfo>
    {
        public readonly Object Target;
        public readonly string Name;

        public TargetInfo(Object target)
        {
            Target = target;
            Name = target.name;
        }

        public override bool Equals(object obj)
        {
            return obj is TargetInfo info && Equals(info);
        }

        public bool Equals(TargetInfo p)
        {
            return (Target == p.Target) && (Name == p.Name);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            // ReSharper disable once Unity.NoNullPropagation
            hash = hash * 23 + Target?.GetHashCode() ?? 0;
            hash = hash * 23 + Name.GetHashCode();
            return hash;
        }

        public static bool operator ==(TargetInfo lhs, TargetInfo rhs) => lhs.Equals(rhs);

        public static bool operator !=(TargetInfo lhs, TargetInfo rhs) => ! lhs.Equals(rhs);
    }
}