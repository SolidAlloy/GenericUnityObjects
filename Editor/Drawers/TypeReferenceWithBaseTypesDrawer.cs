namespace GenericUnityObjects.Editor
{
    using System;
    using GenericUnityObjects.Util;
    using MonoBehaviours;
    using TypeReferences.Editor.Drawers;
    using TypeReferences.Editor.Util;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Assertions;
    using Util;
    using TypeCache = TypeReferences.Editor.Util.TypeCache;

    [CustomPropertyDrawer(typeof(TypeReferenceWithBaseTypes))]
    internal class TypeReferenceWithBaseTypesDrawer : TypeReferencePropertyDrawer
    {
        private static readonly NonGenericAttribute _attribute = new NonGenericAttribute(null);

        public void TriggerDropdownImmediately(Rect position, SerializedProperty property, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);
            DrawTypeReferenceField(position, property, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Assert.IsFalse(label is null || label == GUIContent.none);
            position = EditorGUI.PrefixLabel(position, label);
            DrawTypeReferenceField(position, property, false);
        }

        private void DrawTypeReferenceField(Rect position, SerializedProperty property, bool triggerDropdownImmediately)
        {
            NonGenericAttribute typeOptionsAttribute = GetAttribute(property);

            var serializedTypeRef = new SerializedTypeReference(property);

            Type selectedType = TypeCache.GetType(serializedTypeRef.TypeNameAndAssembly);

            if (selectedType != null && ! typeOptionsAttribute.MatchesRequirements(selectedType))
            {
                Debug.Log($"{property.name} had the {selectedType} value but the type does not match " +
                          "constraints set in the attribute, so it was set to null.");
                selectedType = null;
                serializedTypeRef.TypeNameAndAssembly = string.Empty;
            }

            var dropdownDrawer = new TypeDropdownDrawer(selectedType, typeOptionsAttribute, null);

            Action<Type> onTypeSelected;

            if (triggerDropdownImmediately)
            {
                onTypeSelected = type => OnTypeSelected(type, property);
            }
            else
            {
                onTypeSelected = null;
            }

            var fieldDrawer = new TypeFieldDrawer(
                serializedTypeRef,
                position,
                dropdownDrawer,
                typeOptionsAttribute.ShortName,
                onTypeSelected,
                triggerDropdownImmediately);

            fieldDrawer.Draw();
        }

        private void OnTypeSelected(Type type, SerializedProperty property)
        {
            var targetSelector = (BehaviourSelector) property.serializedObject.targetObject;

            GenericBehaviourCreator.AddComponent(
                targetSelector.GetType(),
                targetSelector.gameObject,
                targetSelector.GenericBehaviourType,
                new[] { type });
        }

        private static NonGenericAttribute GetAttribute(SerializedProperty property)
        {
            SerializedProperty baseTypesProperty =
                property.FindPropertyRelative(nameof(TypeReferenceWithBaseTypes.BaseTypeNames));

            var baseTypes = new Type[baseTypesProperty.arraySize];

            for (int i = 0; i < baseTypesProperty.arraySize; i++)
            {
                SerializedProperty typeRefProperty = baseTypesProperty.GetArrayElementAtIndex(i);
                Type baseType = TypeCache.GetType(typeRefProperty.stringValue);
                baseTypes[i] = baseType;
            }

            _attribute.ChangeBaseTypes(baseTypes);
            return _attribute;
        }
    }
}