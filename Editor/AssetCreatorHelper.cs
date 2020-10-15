namespace GenericScriptableObjects.Editor
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

    internal class AssetCreatorHelper
    {
        private const string GenericSOTypesPath = "Scripts/GenericScriptableObjectTypes";
        private const string NamespaceName = "GenericScriptableObjectsTypes";

        private readonly Type _genericType;
        private readonly Type[] _paramTypes;
        private readonly string[] _paramTypeNamesWithoutAssembly;
        private readonly string _className;
        private readonly string _defaultAssetName;

        public AssetCreatorHelper(Type genericType, Type[] paramTypes)
        {
            _genericType = genericType;
            _paramTypes = paramTypes;

            _paramTypeNamesWithoutAssembly = _paramTypes
                .Select(type => GetTypeNameWithoutAssembly(type.FullName)).ToArray();

            var genericTypeClassSafeName = GetClassSafeTypeName(_genericType.Name);
            string paramTypesClassSafeNames =
                string.Join("_", _paramTypeNamesWithoutAssembly.Select(GetClassSafeTypeName));

            _className = $"{genericTypeClassSafeName}_{paramTypesClassSafeNames}";
            _defaultAssetName = $"New {genericTypeClassSafeName}.asset";
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

            Type genericTypeWithArgs = _genericType.MakeGenericType(_paramTypes);
            GenericSOCreator.Instance.GenericTypeToCreate = genericTypeWithArgs;

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
            AssetCreator.Create(asset, _defaultAssetName);
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