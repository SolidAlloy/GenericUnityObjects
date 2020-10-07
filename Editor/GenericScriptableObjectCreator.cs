namespace GenericScriptableObjects.Editor
{
    using System;
    using System.IO;
    using GenericScriptableObjects;
    using SolidUtilities.Editor.EditorWindows;
    using TypeReferences;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;
    using UnityEngine.Assertions;
    using Assembly = System.Reflection.Assembly;

    public class GenericScriptableObjectCreator : SingletonScriptableObject<GenericScriptableObjectCreator>
    {
        [SerializeField] private TypeReference _pendingCreationType; // TODO: hide in inspector

        [DidReloadScripts]
        private static void OnScriptsReload()
        {
            if (Instance._pendingCreationType.Type == null)
                return;

            try
            {
                string typeName = GetClassSafeTypeName(GetTypeNameWithoutAssembly(Instance._pendingCreationType.Type.FullName));
                var csharpAssembly = Assembly.Load("Assembly-CSharp");
                Type type = csharpAssembly.GetType($"GenericScriptableObjectsTypes.Generic_{typeName}");
                Assert.IsNotNull(type);
                GenericDerivativesDatabase.Add(Instance._pendingCreationType, type);
                CreateAssetInteractively(Instance._pendingCreationType, typeName);
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
                string fullTypeName = GetTypeNameWithoutAssembly(selectedType.FullName);
                string classSafeTypeName = GetClassSafeTypeName(fullTypeName);

                if (GenericDerivativesDatabase.ContainsKey(selectedType))
                {
                    CreateAssetInteractively(selectedType, classSafeTypeName);
                }
                else
                {
                    string template = GenericDerivativesDatabase.Template;
                    template = template.Replace("#TYPE_NAME", classSafeTypeName);
                    template = template.Replace("#TYPE", fullTypeName);

                    string fullAssetPath =
                        $"{Application.dataPath}/Scripts/GenericScriptableObjects/Generic_{classSafeTypeName}.cs";

                    if (File.Exists(fullAssetPath))
                    {
                        string oldFileContent = File.ReadAllText(fullAssetPath);
                        if (oldFileContent == template)
                        {
                            var csharpAssembly = Assembly.Load("Assembly-CSharp");
                            Type assetType = csharpAssembly.GetType($"GenericScriptableObjectsTypes.Generic_{classSafeTypeName}");
                            Assert.IsNotNull(assetType);
                            GenericDerivativesDatabase.Add(selectedType, assetType);
                            CreateAssetInteractively(selectedType, classSafeTypeName);
                            return;
                        }
                    }

                    if (!AssetDatabase.IsValidFolder("Assets/Scripts"))
                        AssetDatabase.CreateFolder("Assets", "Scripts");

                    if (!AssetDatabase.IsValidFolder("Assets/Scripts/GenericScriptableObjects"))
                        AssetDatabase.CreateFolder("Assets/Scripts", "GenericScriptableObjects");

                    Instance._pendingCreationType = selectedType;
                    File.WriteAllText($"{Application.dataPath}/Scripts/GenericScriptableObjects/Generic_{classSafeTypeName}.cs", template);
                    AssetDatabase.Refresh();
                }
            });
        }

        private static void CreateAssetInteractively(Type selectedType, string classSafeTypeName)
        {
            var asset = Generic.Create(selectedType);
            Assert.IsNotNull(asset);
            AssetCreator.Create(asset, $"New Generic_{classSafeTypeName}.asset");
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