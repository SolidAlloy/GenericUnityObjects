namespace GenericUnityObjects.UnityEditorInternals
{
    extern alias UnityEditorCore;
    extern alias UnityEngineCore;
    extern alias IMGUIModule;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using SolidUtilities.Editor.Extensions;
    using UnityEditorCore::UnityEditor;
    using UnityEditorInternal;
    using UnityEngineCore::UnityEngine;
    using Object = UnityEngineCore::UnityEngine.Object;
    using SolidUtilities.Extensions;

    internal readonly struct ObjectField
    {
        public readonly string Name;
        public readonly Type Type;

        public ObjectField(string name, Type type)
        {
            Name = name;
            Type = type;
        }
    }

    internal class InternalMonoImporterEditor : MonoScriptImporterInspector
    {
        private MonoImporter _targetImporter;
        private bool _isTypeCompatible;

        private ObjectField[] _objectFields;
        private Object[] _newTargets;

        public override void OnEnable()
        {
            base.OnEnable();

            _targetImporter = (MonoImporter) target;
            MonoScript script = _targetImporter.GetScript();

            if (script is null)
                return;

            Type scriptType = script.GetClassType();
            _isTypeCompatible = MonoScriptImporterInspector.IsTypeCompatible(scriptType);
            _objectFields = GetObjectFields(scriptType);
            _newTargets = new Object[_objectFields.Length];
        }

        public override void OnInspectorGUI()
        {
            if ( ! _isTypeCompatible)
            {
                if ( ! InternalEditorUtility.IsInEditorFolder(_targetImporter.assetPath))
                {
                    EditorGUILayout.HelpBox(
                        "No MonoBehaviour scripts in the file, or their names do not match the file name.",
                        MessageType.Info);
                }

                return;
            }

            bool didModify = false;

            // Make default reference fields show small icons
            using (new EditorGUIUtility.IconSizeScope(new Vector2(16f, 16f)))
            {
                for (int i = 0; i < _objectFields.Length; i++)
                {
                    var field = _objectFields[i];

                    Object oldTarget = _targetImporter.GetDefaultReference(field.Name);
                    Object newTarget = EditorGUILayout.ObjectField(ObjectNames.NicifyVariableName(field.Name), oldTarget, field.Type, false);
                    didModify |= oldTarget != newTarget;

                    _newTargets[i] = newTarget;
                }
            }

            if (_objectFields.Length != 0)
                EditorGUILayout.HelpBox("Default references will only be applied in edit mode.", MessageType.Info);

            if ( ! didModify)
                return;

            _targetImporter.SetDefaultReferences(_objectFields.Select(field => field.Name).ToArray(), _newTargets);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_targetImporter));
        }

        private ObjectField[] GetObjectFields(Type scriptType)
        {
            Type type = scriptType;
            var fields = new List<ObjectField>();

            while (MonoScriptImporterInspector.IsTypeCompatible(type))
            {
                AddObjectFields(type, fields);
                // ReSharper disable once PossibleNullReferenceException
                type = type.BaseType;
            }

            return fields.ToArray();
        }


        private void AddObjectFields(Type type, List<ObjectField> fields)
        {
            foreach (FieldInfo field in type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if ( ! (field.IsPublic || field.HasAttribute<SerializeField>()))
                {
                    continue;
                }

                if (typeof(Object).IsAssignableFrom(field.FieldType) && field.FieldType.FullName != null)
                {
                    fields.Add(new ObjectField(field.Name, field.FieldType));
                }
            }
        }

        private Object ObjectField(Object oldTarget, ObjectField field)
        {
            string niceName = ObjectNames.NicifyVariableName(field.Name);

            return field.Type.IsGenericType
                ? EditorGUILayoutHelper.GenericObjectField(niceName, oldTarget, field.Type, false)
                : EditorGUILayout.ObjectField(niceName, oldTarget, field.Type, false);
        }
    }
}