namespace GenericUnityObjects.Editor
{
    using System;
    using SolidUtilities.Editor.Helpers;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Assertions;
    using Object = UnityEngine.Object;

    public class GenericUnityObjectHelper
    {
        private readonly bool _drawForGeneric;
        private readonly MonoScript _monoScript;
        private readonly Type _genericType;

        public GenericUnityObjectHelper(Object target)
        {
            _genericType = target.GetType().BaseType;

            if (_genericType?.IsGenericType != true)
                return;

            _drawForGeneric = true;
            _monoScript = AssetSearcher.GetMonoScriptFromType(_genericType);
        }

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