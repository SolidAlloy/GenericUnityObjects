namespace GenericScriptableObjects.Editor
{
    using System;
    using System.ComponentModel;
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

    [TypeDescriptionProvider(typeof(GenericSODescriptionProvider))]
    public abstract class GenericSOCreator : SingletonScriptableObject<GenericSOCreator>
    {
        private const string GenericSOTypesPath = "Scripts/GenericScriptableObjectTypes";
        private const string NamespaceName = "GenericScriptableObjectsTypes";

        [SerializeField] private TypeReference _pendingCreationType; // TODO: hide in inspector
        [SerializeField] private string _genericTypeName;

        [DidReloadScripts]
        private static void OnScriptsReload()
        {
            if (Instance._pendingCreationType.Type == null)
                return;

            try
            {
                string typeName = GetClassSafeTypeName(GetTypeNameWithoutAssembly(Instance._pendingCreationType.Type.FullName));
                CreateAssetFromExistingType(Instance._pendingCreationType, Instance._genericTypeName, typeName);
            }
            finally
            {
                Instance._pendingCreationType.Type = null;
                Instance._genericTypeName = null;
            }
        }

        protected static void CreateAsset(Type genericSOType)
        {
            TypeSelectionWindow.Create(selectedParamType =>
            {
                string classSafeGenericTypeName = GetClassSafeTypeName(genericSOType.Name);
                string genericTypeNameWithoutParam = genericSOType.Name.Split('`')[0];
                string fullParamTypeName = GetTypeNameWithoutAssembly(selectedParamType.FullName);
                string classSafeParamTypeName = GetClassSafeTypeName(fullParamTypeName);

                if (GenericSODatabase.ContainsKey(selectedParamType)) // TODO: replace with a dedicated database
                {
                    CreateAssetInteractively(selectedParamType, classSafeGenericTypeName, classSafeParamTypeName);
                    return;
                }

                string fullAssetPath = $"{Application.dataPath}/{GenericSOTypesPath}/Generic_{classSafeParamTypeName}.cs";

                string scriptContent = GetScriptContent(NamespaceName, classSafeGenericTypeName, classSafeParamTypeName,
                    genericSOType.Namespace, genericTypeNameWithoutParam, fullParamTypeName);

                if (FileContentMatches(fullAssetPath, scriptContent))
                {
                    CreateAssetFromExistingType(selectedParamType, classSafeGenericTypeName, classSafeParamTypeName);
                    return;
                }

                Instance._pendingCreationType = selectedParamType;
                Instance._genericTypeName = classSafeGenericTypeName;
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
            string classSafeGenericTypeName,
            string classSafeTypeName,
            string genericNamespace,
            string genericTypeNameWithoutParam,
            string type)
        {
            return $"namespace {namespaceName} {{ " +
                   $"public class {classSafeGenericTypeName}_{classSafeTypeName} : " +
                   $"{genericNamespace}.{genericTypeNameWithoutParam}<{type}> {{ }} }}";
        }

        private static void CreateAssetFromExistingType(Type selectedType, string classSafeGenericTypeName, string classSafeTypeName)
        {
            var csharpAssembly = Assembly.Load("Assembly-CSharp");
            Type assetType = csharpAssembly.GetType($"GenericScriptableObjectsTypes.Generic_{classSafeTypeName}");
            Assert.IsNotNull(assetType);
            GenericSODatabase.Add(selectedType, assetType);
            CreateAssetInteractively(selectedType, classSafeGenericTypeName, classSafeTypeName);
        }

        private static void CreateAssetInteractively(Type selectedType, string classSafeGenericTypeName, string classSafeTypeName)
        {
            var asset = GenericScriptableObject.Create(selectedType);
            Assert.IsNotNull(asset);
            AssetCreator.Create(asset, $"New {classSafeGenericTypeName}_{classSafeTypeName}.asset");
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

    public class CustomSOCreator : GenericSOCreator
    {
        [CreateCustomAssetMenu("Custom Generic SO")]
        private static void CreateAsset()
        {
            CreateAsset(typeof(CustomGeneric<>));
        }
    }
}