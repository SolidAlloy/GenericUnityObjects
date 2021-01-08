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
            Assert.IsNotNull(_monoScript);
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
                if (_drawForGeneric)
                {
                    EditorGUILayout.ObjectField("Script", _monoScript, _genericType, false, null);
                }
                else
                {
                    EditorGUILayout.ObjectField(monoScriptProperty, (GUILayoutOption[]) null);
                }
            }
        }
    }
}