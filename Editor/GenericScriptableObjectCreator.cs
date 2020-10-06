namespace GenericScriptableObjects.Editor
{
    using System;
    using System.IO;
    using GenericScriptableObjects;
    using SolidUtilities.Editor.Extensions;
    using TypeReferences;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Assertions;
    using Assembly = System.Reflection.Assembly;

    [InitializeOnLoad]
    public class GenericScriptableObjectCreator : SingletonScriptableObject<GenericScriptableObjectCreator>
    {
        [SerializeField] private TypeReference _pendingCreationType; // TODO: hide in inspector

        private void OnEnable()
        {
            if (Instance._pendingCreationType.Type == null)
                return;

            try
            {
                var csharpAssembly = Assembly.Load("Assembly-CSharp");
                string typeName = GetClassSafeTypeName(GetTypeNameWithoutAssembly(Instance._pendingCreationType.Type.FullName));
                Type type = csharpAssembly.GetType($"GenericScriptableObjectsTypes.Generic_{typeName}");
                Assert.IsNotNull(type);
                GenericDerivativesDatabase.Add(Instance._pendingCreationType, type);
                var asset = Generic.Create(Instance._pendingCreationType);
                Assert.IsNotNull(asset);
                // ProjectWindowUtil.CreateAsset(asset, $"New Generic_{typeName}.asset");
                DummyWindow.Create(asset, $"New Generic_{typeName}.asset");
            }
            finally
            {
                Instance._pendingCreationType.Type = null;
            }
        }

        // [MenuItem("Assets/Create/Generic ScriptableObject", false, 100)]
        [MenuItem("Assets/Create/Generic ScriptableObject")]
        public static void CreateAsset()
        {
            TypeSelectionWindow.Create(selectedType =>
            {
                if (GenericDerivativesDatabase.ContainsKey(selectedType))
                {
                    var asset = Generic.Create(selectedType);
                    string typeName = GetClassSafeTypeName(GetTypeNameWithoutAssembly(selectedType.FullName));
                    ProjectWindowUtil.CreateAsset(asset, $"New Generic_{typeName}.asset");
                }
                else
                {
                    CreateNewType(selectedType);
                }
            });
        }

        private static void CreateNewType(Type type)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scripts"))
                AssetDatabase.CreateFolder("Assets", "Scripts");

            if (!AssetDatabase.IsValidFolder("Assets/Scripts/GenericScriptableObjects"))
                AssetDatabase.CreateFolder("Assets/Scripts", "GenericScriptableObjects");

            string template = GenericDerivativesDatabase.Template;
            string fullTypeName = GetTypeNameWithoutAssembly(type.FullName);
            string classSafeTypeName = GetClassSafeTypeName(fullTypeName);
            template = template.Replace("#TYPE_NAME", classSafeTypeName);
            template = template.Replace("#TYPE", fullTypeName);

            Instance._pendingCreationType = type;
            File.WriteAllText($"{Application.dataPath}/Scripts/GenericScriptableObjects/Generic_{classSafeTypeName}.cs", template);
            AssetDatabase.Refresh();
            // CompilationPipeline.RequestScriptCompilation(); // TODO: enable after testing
        }

        private static string GetClassSafeTypeName(string rawTypeName)
        {
            return rawTypeName
                .Replace('.', '_')
                .Replace('`', '_');
        }

        private static string GetTypeNameWithoutAssembly(string fullTypeName)
        {
            return fullTypeName.Split('[')[0];
        }
    }

    public class DummyWindow : EditorWindow
    {
        private ScriptableObject _asset;
        private string _name;
        private bool _calledOnGuiOnce;

        public static void Create(ScriptableObject asset, string name)
        {
            var window = CreateInstance<DummyWindow>();
            window.OnCreate(asset, name);
        }

        private void OnCreate(ScriptableObject asset, string name)
        {
            _asset = asset;
            _name = name;
            this.Resize(1f, 1f);
            EditorApplication.projectChanged += Close;

            Show();
            Focus();
        }

        private void OnGUI()
        {
            if (_calledOnGuiOnce)
                return;

            _calledOnGuiOnce = true;
            ProjectWindowUtil.CreateAsset(_asset, _name);
            position = new Rect(Screen.currentResolution.width + 10f, Screen.currentResolution.height + 10f, 0f, 0f);
        }

        private void OnDestroy()
        {
            EditorApplication.projectChanged -= Close;
        }
    }
}