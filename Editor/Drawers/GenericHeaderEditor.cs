namespace GenericUnityObjects.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using GenericUnityObjects.Util;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;
    
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector.Editor;
#endif

    /// <summary>
    /// An extension of Editor that changes name of <see cref="GenericScriptableObject"/> assets in the Inspector header.
    /// For all other assets, it draws header like before.
    /// </summary>
    public class GenericHeaderEditor : 
#if ODIN_INSPECTOR
        OdinEditor
#else
        Editor
#endif
    {
        private static readonly Dictionary<TargetInfo, string> _targetTitlesCache = new Dictionary<TargetInfo, string>();
        private static readonly Dictionary<Type, string> _typeNamesCache = new Dictionary<Type, string>();

        private static Func<Editor, string, Rect> _drawHeaderGUI;

        private static Func<Editor, string, Rect> DrawHeaderGUI
        {
            get
            {
                if (_drawHeaderGUI == null)
                {
                    var drawHeaderMethod = typeof(Editor).GetMethod("DrawHeaderGUI",
                        BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(Editor), typeof(string) },
                        null);

                    _drawHeaderGUI = (Func<Editor, string, Rect>) Delegate.CreateDelegate(typeof(Func<Editor, string, Rect>), drawHeaderMethod);
                }

                return _drawHeaderGUI;
            }
        }

        private static Func<Object, string> _getTypeName;

        private static Func<Object, string> GetTypeName
        {
            get
            {
                if (_getTypeName == null)
                {
                    var getTypeNameMethod = typeof(ObjectNames).GetMethod("GetTypeName",
                        BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(Object) }, null);

                    _getTypeName = (Func<Object, string>) Delegate.CreateDelegate(typeof(Func<Object, string>), getTypeNameMethod);
                }

                return _getTypeName;
            }
        }
        
#if ! ODIN_INSPECTOR
        protected virtual void OnEnable() { }
        
        protected virtual void OnDisable() { }
#endif

        protected override void OnHeaderGUI()
        {
            DrawHeaderGUI(this, GetTitle());
        }

        private string GetTitle()
        {
            Type genericType = target.GetType().BaseType;

            return targets.Length == 1
                ? GetOneTitle(genericType)
                : GetMixedTitle(genericType);
        }

        private string GetOneTitle(Type genericType)
        {
            if (genericType?.IsGenericType != true)
                return ObjectNames.GetInspectorTitle(target);
            
            var targetInfo = new TargetInfo(target);

            if (_targetTitlesCache.TryGetValue(targetInfo, out string title))
                return title;

            string typeName = GetGenericTypeName(genericType);

            // target.name is empty when a new asset is created interactively and has not been named yet.
            if (string.IsNullOrEmpty(target.name))
                return $"({typeName})";

            title = $"{ObjectNames.NicifyVariableName(target.name)} ({typeName})";
            _targetTitlesCache.Add(targetInfo, title);
            return title;
        }

        private string GetMixedTitle(Type genericType)
        {
            if (genericType?.IsGenericType != true)
                return targets.Length + " " + ObjectNames.NicifyVariableName(GetTypeName(target)) + "s";
            
            return $"{targets.Length} objects of type {GetGenericTypeName(genericType)}";
        }

        private static string GetGenericTypeName(Type genericType)
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