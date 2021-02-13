namespace GenericUnityObjects.Editor.ScriptableObjects.SelectionWindow
{
    using System;
    using System.Linq;
    using GenericUnityObjects.Util;
    using SolidUtilities.Editor.Extensions;
    using SolidUtilities.Editor.Helpers;
    using TypeReferences;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// A window that has as many TypeReference fields as needed for the asset creation. The user has to choose all
    /// the generic argument types for the asset to be created.
    /// </summary>
    internal class MultipleTypeSelectionWindow : TypeSelectionWindow
    {
        private const float WindowWidth = 300f;

        [SerializeField] private TypeReferenceWithBaseTypes[] _typeRefs;

        private Action<Type[]> _onTypesSelected;
        private SerializedObject _serializedObject;
        private ContentCache _contentCache;
        private string[] _genericArgNames;

        protected override void OnCreate(Action<Type[]> onTypesSelected, string[] genericArgNames, Type[][] genericParamConstraints)
        {
            int typesCount = genericParamConstraints.Length;
            _onTypesSelected = onTypesSelected;
            _genericArgNames = genericArgNames;
            _typeRefs = new TypeReferenceWithBaseTypes[typesCount];

            for (int i = 0; i < typesCount; i++)
            {
                _typeRefs[i] = new TypeReferenceWithBaseTypes
                {
                    BaseTypeNames = genericParamConstraints[i]
                        .Select(TypeReference.GetTypeNameAndAssembly)
                        .ToArray()
                };
            }

            titleContent = new GUIContent("Choose Arguments");
            _serializedObject = new SerializedObject(this);
            _contentCache = new ContentCache();
            this.Resize(WindowWidth, GetWindowHeight(typesCount));
            this.CenterOnMainWin();
        }

        protected override void OnGUI()
        {
            SerializedProperty typesArray = _serializedObject.FindProperty(nameof(_typeRefs));

            for (int i = 0; i < typesArray.arraySize; i++)
            {
                EditorGUILayout.PropertyField(
                    typesArray.GetArrayElementAtIndex(i),
                    _contentCache.GetItem(_genericArgNames[i]));
            }

            if ( ! GUILayout.Button("Create Asset"))
                return;

            if (_typeRefs.Any(typeRef => typeRef.Type == null))
            {
                Debug.LogWarning("Choose all the type parameters first!");
            }
            else
            {
                Close();
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
}