﻿namespace GenericUnityObjects.Editor
{
    using System;
    using SolidUtilities.Editor.Helpers;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    /// <summary>
    /// A class that places the correct script in the MonoScript field of generic UnityEngine.Objects.
    /// </summary>
    public class GenericUnityObjectHelper
    {
        private readonly bool _drawForGeneric;
        private readonly MonoScript _monoScript;
        private readonly Type _genericType;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericUnityObjectHelper"/> class. Recommended to use in OnEnable.
        /// </summary>
        /// <param name="target">Target object of custom editor.</param>
        public GenericUnityObjectHelper(Object target)
        {
            _genericType = target.GetType().BaseType;

            if (_genericType?.IsGenericType != true)
                return;

            _drawForGeneric = true;
            _monoScript = AssetSearcher.GetMonoScriptFromType(_genericType);
        }

        /// <summary>
        /// Draws MonoScript field and places the correct generic UnityEngine.Object script into it.
        /// </summary>
        /// <param name="monoScriptProperty">A MonoScript property.</param>
        /// <exception cref="ArgumentException">If the passed property path is not m_Script.</exception>
        public void DrawMonoScript(SerializedProperty monoScriptProperty)
        {
            if (monoScriptProperty.propertyPath != "m_Script")
            {
                throw new ArgumentException(
                    $"Expected a MonoScript property but {monoScriptProperty.propertyPath} was passed as an argument instead.");
            }

            using (new EditorGUI.DisabledScope(true))
            {
                if (_drawForGeneric && ! (_monoScript is null))
                {
                    EditorGUILayout.ObjectField("Script", _monoScript, _genericType, false, null);
                }
                else
                {
                    EditorGUILayout.ObjectField(monoScriptProperty, (GUILayoutOption[]) null);
                }
            }

            if (_drawForGeneric && _monoScript is null)
            {
                EditorGUILayout.HelpBox(
                    "The associated script that contains the generic type could not be found.\n" +
                    "Please check that its file name matches the class name.",
                    MessageType.Warning);
            }
        }
    }
}