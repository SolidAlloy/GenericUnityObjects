namespace GenericUnityObjects.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using GenericUnityObjects.Util;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    /// <summary>
    /// An extension of Editor that changes name of generic <see cref="ScriptableObject"/> assets in the Inspector header.
    /// For all other assets, it draws header like before.
    /// </summary>
    public static class GenericHeaderUtility
    {
        private static readonly Dictionary<Object, string> _targetTitlesCache = new Dictionary<Object, string>();
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

        public static void OnHeaderGUI(Editor editor)
        {
            DrawHeaderGUI(editor, GetTitle(editor));
        }

        private static string GetTitle(Editor editor)
        {
            Type genericType = editor.target.GetType().BaseType;

            return editor.targets.Length == 1
                ? GetOneTitle(genericType, editor.target)
                : GetMixedTitle(genericType, editor.targets);
        }

        private static string GetOneTitle(Type genericType, Object target)
        {
            if (genericType?.IsGenericType != true)
                return ObjectNames.GetInspectorTitle(target);

            if (_targetTitlesCache.TryGetValue(target, out string title))
                return title;

            string typeName = GetGenericTypeName(genericType);

            // target.name is empty when a new asset is created interactively and has not been named yet.
            if (string.IsNullOrEmpty(target.name))
                return $"({typeName})";

            title = $"{ObjectNames.NicifyVariableName(target.name)} ({typeName})";
            _targetTitlesCache.Add(target, title);
            return title;
        }

        private static string GetMixedTitle(Type genericType, Object[] targets)
        {
            if (genericType?.IsGenericType != true)
                return targets.Length + " " + ObjectNames.NicifyVariableName(GetTypeName(targets[0])) + "s";

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
}