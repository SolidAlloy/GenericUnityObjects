namespace GenericScriptableObjects.Editor.AssetCreation
{
    using System;
    using System.IO;
    using System.Linq;
    using SolidUtilities.Editor.EditorWindows;
    using SolidUtilities.Editor.Helpers;
    using SolidUtilities.Helpers;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Assertions;
    using Util;

    internal class AssetCreatorHelper
    {
        private readonly Type _genericType;
        private readonly Type[] _argumentTypes;
        private readonly string _fileName;

        public AssetCreatorHelper(Type genericType, Type[] argumentTypes, string fileName)
        {
            _genericType = genericType;
            _argumentTypes = argumentTypes;
            _fileName = fileName;
        }

        public void CreateAsset()
        {
            if (GenericObjectDatabase.ContainsKey(_genericType, _argumentTypes))
            {
                CreateAssetInteractively();
                return;
            }

            Type genericTypeWithArgs = _genericType.MakeGenericType(_argumentTypes);

            Type existingAssetType = CreatorUtil.GetEmptyTypeDerivedFrom(genericTypeWithArgs);

            if (existingAssetType != null)
            {
                CreateAssetFromExistingType(existingAssetType);
                return;
            }

            PersistentStorage.SaveForAssemblyReload(genericTypeWithArgs, _fileName);
            string className = GetUniqueClassName();
            CreateScript(className);
        }

        public void CreateAssetFromExistingType()
        {
            Type genericTypeWithArgs = _genericType.MakeGenericType(_argumentTypes);
            Type existingAssetType = CreatorUtil.GetEmptyTypeDerivedFrom(genericTypeWithArgs);
            Assert.IsNotNull(existingAssetType);
            CreateAssetFromExistingType(existingAssetType);
        }

        /// <summary>
        /// The method first creates a class name that consists of the generic type name and names of the arguments.
        /// If such a name already exists, it adds an index to the name (1, 2, and so on.)
        /// </summary>
        /// <returns>Unique class name.</returns>
        private string GetUniqueClassName()
        {
            string argumentNames = string.Join("_", _argumentTypes.Select(type => type.Name.MakeClassFriendly()));
            string className = $"{_genericType.Name.MakeClassFriendly().StripGenericSuffix()}_{argumentNames}";
            int identicalNamesNum = AssetDatabase.FindAssets(className, new[] { Config.GeneratedTypesPath }).Length;
            return identicalNamesNum == 0 ? className : $"{className}_{identicalNamesNum}";
        }

        private void CreateAssetFromExistingType(Type assetType)
        {
            GenericObjectDatabase.Add(_genericType, _argumentTypes, assetType);
            CreateAssetInteractively();
        }

        private void CreateScript(string className)
        {
#if UNITY_EDITOR_WIN
            const string longPathPrefix = @"\\?\";
            string fullAssetPath = $"{longPathPrefix}{Directory.GetCurrentDirectory()}/{Config.GeneratedTypesPath}/{className}.cs";

            if (fullAssetPath.Length > 260 + longPathPrefix.Length)
            {
                Debug.LogWarning("The generated script has a path over 260 characters. It may cause issues " +
                                 $"on Windows (e.g. with git pull/push). Path: {fullAssetPath}");
            }
#else
            string fullAssetPath = $"{Config.GeneratedTypesPath}/{className}.cs";
#endif

            string scriptContent = GetScriptContent(className);

            CreatorUtil.WriteAllText(fullAssetPath, scriptContent, Config.GeneratedTypesPath);
            AssetDatabase.Refresh();
        }

        private string GetScriptContent(string className)
        {
            string genericTypeWithBrackets = CreatorUtil.GetFullNameWithBrackets(_genericType, _argumentTypes);
            return $"namespace {Config.GeneratedTypesNamespace} {{ internal class {className} : {genericTypeWithBrackets} {{ }} }}";
        }

        private void CreateAssetInteractively()
        {
            Type typeToCreate = _genericType.MakeGenericType(_argumentTypes);
            var asset = GenericScriptableObject.CreateInstance(typeToCreate);
            Assert.IsNotNull(asset);
            AssetCreator.Create(asset, $"{_fileName}.asset");
        }
    }
}