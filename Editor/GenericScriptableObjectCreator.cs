namespace GenericScriptableObjects.Editor
{
    using System;
    using System.IO;
    using GenericScriptableObjects;
    using SolidUtilities.Editor.EditorWindows;
    using SolidUtilities.Editor.Helpers;
    using TypeReferences;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;
    using UnityEngine.Assertions;
    using Assembly = System.Reflection.Assembly;

    public class GenericScriptableObjectCreator : SingletonScriptableObject<GenericScriptableObjectCreator>
    {
        private const string GenericSOTypesPath = "Scripts/GenericScriptableObjectTypes";
        private const string NamespaceName = "GenericScriptableObjectsTypes";
        private const string GenericTypeName = nameof(Generic);

        [SerializeField] private TypeReference _pendingCreationType; // TODO: hide in inspector

        [DidReloadScripts]
        private static void OnScriptsReload()
        {
            if (Instance._pendingCreationType.Type == null)
                return;

            try
            {
                string typeName = GetClassSafeTypeName(GetTypeNameWithoutAssembly(Instance._pendingCreationType.Type.FullName));
                CreateAssetFromExistingType(Instance._pendingCreationType, typeName);
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
                    return;
                }

                string fullAssetPath = $"{Application.dataPath}/{GenericSOTypesPath}/Generic_{classSafeTypeName}.cs";

                string scriptContent = GetScriptContent(NamespaceName, GenericTypeName,
                    classSafeTypeName, typeof(Generic<>).Namespace, fullTypeName);

                if (FileContentMatches(fullAssetPath, scriptContent))
                {
                    CreateAssetFromExistingType(selectedType, classSafeTypeName);
                    return;
                }

                Instance._pendingCreationType = selectedType;
                AssetDatabaseHelper.MakeSureFolderExists(GenericSOTypesPath);
                File.WriteAllText(fullAssetPath, scriptContent);
                AssetDatabase.Refresh();
            });
        }

        private static bool FileContentMatches(string filePath, string contentToCompareTo)
        {
            if (File.Exists(filePath))
            {
                string oldFileContent = File.ReadAllText(filePath);
                if (oldFileContent == contentToCompareTo)
                    return true;
            }

            return false;
        }

        private static string GetScriptContent(
            string namespaceName,
            string genericName,
            string typeName,
            string genericNamespace,
            string type)
        {
            return $"namespace {namespaceName} {{ public class {genericName}_{typeName} : {genericNamespace}.{genericName}<{type}> {{ }} }}";
        }

        private static void CreateAssetFromExistingType(Type selectedType, string classSafeTypeName)
        {
            var csharpAssembly = Assembly.Load("Assembly-CSharp");
            Type assetType = csharpAssembly.GetType($"GenericScriptableObjectsTypes.Generic_{classSafeTypeName}");
            Assert.IsNotNull(assetType);
            GenericDerivativesDatabase.Add(selectedType, assetType);
            CreateAssetInteractively(selectedType, classSafeTypeName);
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