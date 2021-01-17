namespace GenericUnityObjects.Editor.Util
{
    using System;
    using System.Collections.Generic;
    using GenericUnityObjects.Util;
    using TypeReferences;
    using TypeReferences.Editor.Drawers;
    using TypeReferences.Editor.Util;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Assertions;
    using TypeCache = TypeReferences.Editor.Util.TypeCache;

    [CustomPropertyDrawer(typeof(TypeReferenceWithBaseTypes))]
    internal class TypeReferenceWithBaseTypesDrawer : TypeReferencePropertyDrawer
    {
        private static readonly string[] _additionalAssemblies = { "Assembly-CSharp" };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Assert.IsNotNull(label);
            Assert.IsFalse(label == GUIContent.none);
            position = EditorGUI.PrefixLabel(position, label);
            DrawTypeReferenceField(position, property);
        }

        private static TypeOptionsAttribute GetAttribute(SerializedProperty property)
        {
            SerializedProperty baseTypesProperty =
                property.FindPropertyRelative(nameof(TypeReferenceWithBaseTypes.BaseTypeNames));

            if (baseTypesProperty.arraySize == 0)
                return TypeOptionsAttribute.Default;

            var baseTypes = new Type[baseTypesProperty.arraySize];

            for (int i = 0; i < baseTypesProperty.arraySize; i++)
            {
                SerializedProperty typeRefProperty = baseTypesProperty.GetArrayElementAtIndex(i);
                Type baseType = TypeCache.GetType(typeRefProperty.stringValue);
                baseTypes[i] = baseType;
            }

            return new InheritsAttribute(baseTypes) { ExpandAllFolders = true };
        }

        private void DrawTypeReferenceField(Rect position, SerializedProperty property)
        {
            TypeOptionsAttribute typeOptionsAttribute = GetAttribute(property);
            typeOptionsAttribute.ExcludeNone = true;
            typeOptionsAttribute.IncludeAdditionalAssemblies = _additionalAssemblies;
            typeOptionsAttribute.ShortName = true;

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