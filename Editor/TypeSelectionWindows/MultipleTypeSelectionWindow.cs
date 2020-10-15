namespace GenericScriptableObjects.Editor.TypeSelectionWindows
{
    using System;
    using System.Linq;
    using SolidUtilities.Editor.Extensions;
    using SolidUtilities.Editor.Helpers;
    using TypeReferences;
    using UnityEditor;
    using UnityEngine;
    using Util;

    internal class MultipleTypeSelectionWindow : TypeSelectionWindow
    {
        [SerializeField, UseDefaultAssembly(ExcludeNone = true, SerializableOnly = true)]
        private TypeReference[] _typeRefs;

        private Action<Type[]> _onTypesSelected;
        private SerializedObject _serializedObject;
        private ContentCache _contentCache;

        protected override void OnCreate(Action<Type[]> onTypesSelected, int typesCount)
        {
            _onTypesSelected = onTypesSelected;
            _typeRefs = new TypeReference[typesCount];
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
}