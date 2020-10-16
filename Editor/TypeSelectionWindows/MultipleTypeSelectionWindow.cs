namespace GenericScriptableObjects.Editor.TypeSelectionWindows
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SolidUtilities.Editor.Extensions;
    using SolidUtilities.Editor.Helpers;
    using TypeReferences;
    using TypeReferences.Editor.Drawers;
    using TypeReferences.Editor.Util;
    using UnityEditor;
    using UnityEngine;
    using Util;
    using TypeCache = TypeReferences.Editor.Util.TypeCache;

    /// <summary>
    /// A window that has as many TypeReference fields as needed for the asset creation. The user has to choose all
    /// the generic argument types for the asset to be created.
    /// </summary>
    internal class MultipleTypeSelectionWindow : TypeSelectionWindow
    {
        [SerializeField]
        private ExtendedTypeReference[] _typeRefs;

        private Action<Type[]> _onTypesSelected;
        private SerializedObject _serializedObject;
        private ContentCache _contentCache;

        protected override void OnCreate(Action<Type[]> onTypesSelected, Type[][] genericParamConstraints)
        {
            int typesCount = genericParamConstraints.Length;
            _onTypesSelected = onTypesSelected;
            _typeRefs = new ExtendedTypeReference[typesCount];
            _serializedObject = new SerializedObject(this);
            _contentCache = new ContentCache();
            this.Resize(300f, GetWindowHeight(typesCount));
            this.CenterOnMainWin();
        }

        protected override void OnGUI()
        {
            SerializedProperty typesArray = _serializedObject.FindProperty(nameof(_typeRefs));
            for (int i = 0; i < typesArray.arraySize; i++)
            {
                EditorGUILayout.PropertyField(
                    typesArray.GetArrayElementAtIndex(i),
                    _contentCache.GetItem($"Type Parameter #{i+1}"));
            }

            if ( ! GUILayout.Button("Create Asset"))
                return;

            if (_typeRefs.Any(typeRef => typeRef.Type == null))
            {
                Debug.LogWarning("Choose all the type parameters first!");
            }
            else
            {
                _onTypesSelected(_typeRefs.CastToType());
            }
        }

        private static float GetWindowHeight(int typeFieldsCount)
        {
            float oneTypeFieldHeight = EditorStyles.popup.CalcHeight(GUIContent.none, 0f);
            const float buttonHeight = 24f;
            const float spacing = 2f;
            float windowHeight = (oneTypeFieldHeight + spacing) * typeFieldsCount + buttonHeight;
            return windowHeight;
        }
    }

    [Serializable]
    public class ExtendedTypeReference : TypeReference // TODO: think of a better name
    {
        // TODO: Maybe replace TypeReferences with type names?
        public TypeReference[] BaseTypes;
    }

    [CustomPropertyDrawer(typeof(ExtendedTypeReference))]
    public sealed class TypeReferencePropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorStyles.popup.CalcHeight(GUIContent.none, 0f);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = ExcludeLabelFromPositionIfNecessary(position, label);
            DrawTypeReferenceField(position, property);
        }

        private static Rect ExcludeLabelFromPositionIfNecessary(Rect position, GUIContent label)
        {
            if (label == null || label == GUIContent.none)
                return position;

            var fieldRectWithoutLabel = EditorGUI.PrefixLabel(position, label);
            return fieldRectWithoutLabel;
        }

        private void DrawTypeReferenceField(Rect position, SerializedProperty property)
        {
            // TODO: simplify the baseTypes creation process. Perhaps get rid of SerializedTypeReference creation.
            SerializedProperty baseTypesProperty = property.FindPropertyRelative(nameof(ExtendedTypeReference.BaseTypes));
            List<Type> baseTypes = new List<Type>(baseTypesProperty.arraySize);
            for (int i = 0; i < baseTypesProperty.arraySize; i++)
            {
                SerializedProperty typeRefProperty = baseTypesProperty.GetArrayElementAtIndex(i);
                var tempSerializedTypeRef = new SerializedTypeReference(typeRefProperty);
                var baseType = TypeCache.GetType(tempSerializedTypeRef.TypeNameAndAssembly);
                baseTypes.Add(baseType);
            }

            var typeOptionsAttribute = new InheritsAttribute(baseTypes.ToArray()) { ExcludeNone = true, SerializableOnly = true };
            var serializedTypeRef = new SerializedTypeReference(property);

            var selectedType = TypeCache.GetType(serializedTypeRef.TypeNameAndAssembly);

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