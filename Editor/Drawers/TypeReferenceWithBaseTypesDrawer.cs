namespace GenericUnityObjects.Editor
{
    using System;
    using GenericUnityObjects.Util;
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

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Assert.IsNotNull(label);
            Assert.IsFalse(label == GUIContent.none);
            position = EditorGUI.PrefixLabel(position, label);
            DrawTypeReferenceField(position, property);
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

            _attribute.BaseTypes = baseTypes;
            return _attribute;
        }

        private void DrawTypeReferenceField(Rect position, SerializedProperty property)
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

            var dropdownDrawer = new TypeDropdownDrawer(selectedType, typeOptionsAttribute, fieldInfo?.DeclaringType);

            var fieldDrawer = new TypeFieldDrawer(
                serializedTypeRef,
                position,
                dropdownDrawer,
                typeOptionsAttribute.ShortName,
                typeOptionsAttribute.UseBuiltInNames);

            fieldDrawer.Draw();
        }
    }
}