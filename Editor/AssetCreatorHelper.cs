﻿namespace GenericScriptableObjects.Editor
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using SolidUtilities.Editor.EditorWindows;
    using SolidUtilities.Editor.Helpers;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Assertions;

    internal class AssetCreatorHelper // TODO: rename to a more meaningful name
    {
        private const string GenericSOTypesPath = "Scripts/GenericScriptableObjectTypes";
        private const string NamespaceName = "GenericScriptableObjectsTypes";

        private readonly Type _genericType;
        private readonly Type[] _paramTypes;
        private readonly string[] _paramTypeNamesWithoutAssembly;
        private readonly string _className;

        public AssetCreatorHelper(Type genericType, Type[] paramTypes)
        {
            _genericType = genericType;
            _paramTypes = paramTypes;

            _paramTypeNamesWithoutAssembly = _paramTypes
                .Select(type => GetTypeNameWithoutAssembly(type.FullName)).ToArray();

            string genericTypeClassSafeName = GetClassSafeTypeName(_genericType.Name);
            string paramTypesClassSafeNames =
                string.Join("_", _paramTypeNamesWithoutAssembly.Select(GetClassSafeTypeName));

            _className = $"{genericTypeClassSafeName}_{paramTypesClassSafeNames}";
        }

        public void CreateAsset()
        {
            if (GenericSODatabase.ContainsKey(_genericType, _paramTypes))
            {
                CreateAssetInteractively();
                return;
            }

            string fullAssetPath = $"{Application.dataPath}/{GenericSOTypesPath}/{_className}.cs";

            string scriptContent = GetScriptContent();

            if (FileContentMatches(fullAssetPath, scriptContent))
            {
                CreateAssetFromExistingType();
                return;
            }

            GenericSOCreator.Instance.SetAssetToCreate(_genericType, _paramTypes);

            AssetDatabaseHelper.MakeSureFolderExists(GenericSOTypesPath);
            File.WriteAllText(fullAssetPath, scriptContent);
            AssetDatabase.Refresh();
        }

        private string GetScriptContent()
        {
            string genericTypeNameWithoutParam = _genericType.Name.Split('`')[0];
            string paramTypeNames = string.Join(", ", _paramTypeNamesWithoutAssembly);

            return $"namespace {NamespaceName} {{ public class {_className} : " +
                   $"{_genericType.Namespace}.{genericTypeNameWithoutParam}<{paramTypeNames}> {{ }} }}";
        }

        private void CreateAssetInteractively()
        {
            var asset = GenericScriptableObject.CreateInstance(_genericType, _paramTypes);
            Assert.IsNotNull(asset);
            AssetCreator.Create(asset, $"New {_className}.asset");
        }

        public void CreateAssetFromExistingType()
        {
            var csharpAssembly = Assembly.Load("Assembly-CSharp");
            Type assetType = csharpAssembly.GetType($"{NamespaceName}.{_className}");
            Assert.IsNotNull(assetType);
            GenericSODatabase.Add(_genericType, _paramTypes, assetType);
            CreateAssetInteractively();
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