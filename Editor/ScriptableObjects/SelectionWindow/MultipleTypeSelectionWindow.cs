namespace GenericUnityObjects.Editor.ScriptableObjects.SelectionWindow
{
    using System;
    using System.Linq;
    using GenericUnityObjects.Util;
    using SolidUtilities.Editor;
    using TypeReferences;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// A window that has as many TypeReference fields as needed for the asset creation. The user has to choose all
    /// the generic argument types for the asset to be created.
    /// </summary>
    internal class MultipleTypeSelectionWindow : EditorWindow, ITypeSelectionWindow
    {
        private const float WindowWidth = 300f;

        [SerializeField] private TypeReferenceWithBaseTypes[] _typeRefs;

        private Action<Type[]> _onTypesSelected;
        private SerializedObject _serializedObject;
        private string[] _genericArgNames;

        public void OnCreate(Action<Type[]> onTypesSelected, string[] genericArgNames, Type[][] genericParamConstraints)
        {
            InitializeMembers(onTypesSelected, genericArgNames, genericParamConstraints);
            SubscribeToCloseWindow();

            this.Resize(WindowWidth, GetWindowHeight(_typeRefs.Length));
            this.CenterOnMainWin();
            Show();
        }

        private void InitializeMembers(Action<Type[]> onTypesSelected, string[] genericArgNames, Type[][] genericParamConstraints)
        {
            _onTypesSelected = onTypesSelected;
            _genericArgNames = genericArgNames;
            _typeRefs = GetTypeRefs(genericParamConstraints);
            titleContent = new GUIContent("Choose Arguments");
            _serializedObject = new SerializedObject(this);
        }

        private void OnGUI()
        {
            SerializedProperty typesArray = _serializedObject.FindProperty(nameof(_typeRefs));

            for (int i = 0; i < typesArray.arraySize; i++)
            {
                EditorGUILayout.PropertyField(
                    typesArray.GetArrayElementAtIndex(i),
                    GUIContentHelper.Temp(_genericArgNames[i]));
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

        private void SubscribeToCloseWindow()
        {
            EditorApplication.projectChanged += Close;
            EditorApplication.quitting += Close;
            AssemblyReloadEvents.beforeAssemblyReload += Close;
        }

        private void OnDestroy()
        {
            EditorApplication.projectChanged -= Close;
            EditorApplication.quitting -= Close;
            AssemblyReloadEvents.beforeAssemblyReload -= Close;
        }

        private static float GetWindowHeight(int typeFieldsCount)
        {
            float oneTypeFieldHeight = EditorStyles.popup.CalcHeight(GUIContent.none, 0f);
            const float buttonHeight = 24f;
            const float spacing = 2f;
            float windowHeight = (oneTypeFieldHeight + spacing) * typeFieldsCount + buttonHeight;
            return windowHeight;
        }

        private static TypeReferenceWithBaseTypes[] GetTypeRefs(Type[][] genericParamConstraints)
        {
            int typesCount = genericParamConstraints.Length;
            var typeRefs = new TypeReferenceWithBaseTypes[typesCount];

            for (int i = 0; i < typesCount; i++)
            {
                typeRefs[i] = new TypeReferenceWithBaseTypes
                {
                    BaseTypeNames = genericParamConstraints[i]
                        .Select(TypeReference.GetTypeNameAndAssembly)
                        .ToArray()
                };
            }

            return typeRefs;
        }
    }
}