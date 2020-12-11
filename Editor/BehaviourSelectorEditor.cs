namespace GenericScriptableObjects.Editor
{
    using System.Linq;
    using AssetCreation;
    using GenericScriptableObjects.Util;
    using SolidUtilities.Editor.Helpers;
    using SolidUtilities.Extensions;
    using SolidUtilities.Helpers;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(BehaviourSelector), true)]
    public class BehaviourSelectorEditor : Editor
    {
        private SerializedProperty _typesArray;
        private ContentCache _contentCache;
        private BehaviourSelector _targetSelector;
        private string _genericTypeNameWithoutSuffix;

        private void OnEnable()
        {
            var targetSelector = target as BehaviourSelector;
            if (targetSelector == null)
                return;

            _targetSelector = targetSelector;
            _typesArray = serializedObject.FindProperty(nameof(BehaviourSelector.TypeRefs));
            _contentCache = new ContentCache();
            _genericTypeNameWithoutSuffix = _targetSelector.GenericBehaviourType.Name.StripGenericSuffix();
        }

        public override void OnInspectorGUI()
        {
            for (int i = 0; i < _typesArray.arraySize; i++)
            {
                EditorGUILayout.PropertyField(
                    _typesArray.GetArrayElementAtIndex(i),
                    _contentCache.GetItem($"Type Parameter #{i+1}"));
            }

            if ( ! GUILayout.Button($"Add {GetComponentName()}"))
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

        private string GetComponentName()
        {
            var argumentNames = _targetSelector.TypeRefs
                .Select(typeRef => typeRef.Type == null ? string.Empty : typeRef.Type.FullName)
                .Select(fullName => fullName.ReplaceWithBuiltInName())
                .Select(fullName => fullName.GetSubstringAfterLast('.'));

            return $"{_genericTypeNameWithoutSuffix}<{string.Join(",", argumentNames)}>";
        }
    }
}