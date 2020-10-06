namespace GenericScriptableObjects.Editor
{
    using System;
    using System.IO;
    using GenericScriptableObjects;
    using TypeReferences;
    using UnityEditor;
    using UnityEditor.Compilation;
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
                ProjectWindowUtil.CreateAsset(asset, $"New Generic_{typeName}.asset");
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

            File.WriteAllText($"{Application.dataPath}/Scripts/GenericScriptableObjects/Generic_{classSafeTypeName}.cs", template);
            Instance._pendingCreationType = type;
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
}