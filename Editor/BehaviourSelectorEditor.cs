namespace GenericScriptableObjects.Editor
{
    using System.Collections.Generic;
    using System.Linq;
    using SolidUtilities.Editor.Helpers;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(BehaviourSelector), true)]
    public class BehaviourSelectorEditor : Editor
    {
        private SerializedProperty _typesArray;
        private ContentCache _contentCache;
        private BehaviourSelector _targetSelector;

        private void OnEnable()
        {
            var targetSelector = target as BehaviourSelector;
            if (targetSelector == null)
                return;

            _targetSelector = targetSelector;
            _typesArray = serializedObject.FindProperty(nameof(BehaviourSelector.TypeRefs));
            _contentCache = new ContentCache();
        }

        public override void OnInspectorGUI()
        {
            for (int i = 0; i < _typesArray.arraySize; i++)
            {
                EditorGUILayout.PropertyField(
                    _typesArray.GetArrayElementAtIndex(i),
                    _contentCache.GetItem($"Type Parameter #{i+1}"));
            }

            if ( ! GUILayout.Button("Add Component"))
                return;

            if (_targetSelector.TypeRefs.Any(typeRef => typeRef.Type == null))
            {
                Debug.LogWarning("Choose all the type parameters first!");
            }
            else
            {
                // TODO: pass generic type and arguments to a method that will add the component.
                Debug.Log($"Adding component of type {_targetSelector.GenericBehaviourType} with args: {string.Join(", ", (IEnumerable<TypeReferenceWithBaseTypes>) _targetSelector.TypeRefs)}");
            }
        }
    }
}