namespace GenericUnityObjects.Editor
{
    using System.Linq;
    using GenericUnityObjects.Util;
    using MonoBehaviours;
    using SolidUtilities.Editor.Helpers;
    using SolidUtilities.Extensions;
    using SolidUtilities.Helpers;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(BehaviourSelector), true)]
    internal class BehaviourSelectorEditor : Editor
    {
        private SerializedProperty _typesArray;
        private ContentCache _contentCache;
        private BehaviourSelector _targetSelector;
        private string _genericTypeNameWithoutSuffix;
        private string[] _argumentNames;
        private bool _triggerDropdown;
        private string[] _genericArgNames;

        private void OnEnable()
        {
            if ( ! (target is BehaviourSelector targetSelector))
                return;

            _targetSelector = targetSelector;
            _typesArray = serializedObject.FindProperty(nameof(BehaviourSelector.TypeRefs));
            _contentCache = new ContentCache();
            _genericTypeNameWithoutSuffix = _targetSelector.GenericBehaviourType.Name.StripGenericSuffix();
            _argumentNames = new string[_targetSelector.TypeRefs.Length];

            if (targetSelector.JustBeenAdded && _targetSelector.TypeRefs.Length == 1)
            {
                _targetSelector.TypeRefs[0].TriggerDropdownImmediately = true;
                targetSelector.JustBeenAdded = false;
            }

            _genericArgNames = TypeUtility.GetNiceArgsOfGenericTypeDefinition(_targetSelector.GenericBehaviourType);
        }

        public override void OnInspectorGUI()
        {
            for (int i = 0; i < _typesArray.arraySize; i++)
            {
                EditorGUILayout.PropertyField(
                    _typesArray.GetArrayElementAtIndex(i),
                    _contentCache.GetItem(_genericArgNames[i]),
                    null);
            }

            if (_triggerDropdown)
            {
                // trigger dropdown
                _triggerDropdown = false;
            }

            if ( ! GUILayout.Button(GetButtonName()))
                return;

            if (_targetSelector.TypeRefs.Any(typeRef => typeRef.Type == null))
            {
                Debug.LogWarning("Choose all the type parameters first!");
            }
            else
            {
                GenericBehaviourCreator.AddComponent(
                    _targetSelector.GetType(),
                    _targetSelector.gameObject,
                    _targetSelector.GenericBehaviourType,
                    _targetSelector.TypeRefs.CastToType());
            }
        }

        private string GetButtonName()
        {
            for (int i = 0; i < _targetSelector.TypeRefs.Length; i++)
            {
                TypeReferenceWithBaseTypes typeRef = _targetSelector.TypeRefs[i];
                string fullName = typeRef.Type == null ? string.Empty : typeRef.Type.FullName;
                fullName = fullName.ReplaceWithBuiltInName();
                _argumentNames[i] = fullName.GetSubstringAfterLast('.');
            }

            return $"Add {_genericTypeNameWithoutSuffix}<{string.Join(",", _argumentNames)}>";
        }
    }
}