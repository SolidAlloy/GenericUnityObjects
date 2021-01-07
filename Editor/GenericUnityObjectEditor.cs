namespace GenericUnityObjects.Editor
{
    using System;
    using GenericUnityObjects;
    using SolidUtilities.Editor.Helpers;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Assertions;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.MonoBehaviour), true)]
    internal class MonoBehaviourEditor : GenericUnityObjectEditor { }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(GenericScriptableObject), true)]
    internal class GenericScriptableObjectEditor : GenericUnityObjectEditor { }

    internal class GenericUnityObjectEditor : Editor
    {
        private bool _drawForGeneric;
        private MonoScript _monoScript;
        private Type _genericType;

        private void OnEnable()
        {
            _genericType = target.GetType().BaseType;

            if (_genericType?.IsGenericType != true)
                return;

            _drawForGeneric = true;
            _monoScript = AssetSearcher.GetMonoScriptFromType(_genericType);
            Assert.IsNotNull(_monoScript);
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.UpdateIfRequiredOrScript();

            SerializedProperty iterator = serializedObject.GetIterator();
            for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                if (iterator.propertyPath == "m_Script")
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        if (_drawForGeneric)
                        {
                            EditorGUILayout.ObjectField("Script", _monoScript, _genericType, false, null);
                        }
                        else
                        {
                            EditorGUILayout.ObjectField(iterator, (GUILayoutOption[]) null);
                        }
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(iterator, true, null);
                }
            }

            serializedObject.ApplyModifiedProperties();
            EditorGUI.EndChangeCheck();
        }
    }
}