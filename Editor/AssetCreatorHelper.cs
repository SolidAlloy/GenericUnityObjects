namespace GenericScriptableObjects.Editor
{
    using System;
    using System.IO;
    using System.Linq;
    using JetBrains.Annotations;
    using SolidUtilities.Editor.EditorWindows;
    using SolidUtilities.Editor.Helpers;
    using SolidUtilities.Extensions;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Assertions;

    internal class AssetCreatorHelper
    {
        private readonly Type _genericType;
        private readonly Type[] _argumentTypes;
        private readonly string _scriptsPath;
        private readonly string _namespaceName;
        private readonly string _fileName;

        public AssetCreatorHelper(Type genericType, Type[] argumentTypes, string namespaceName, string scriptsPath, string fileName)
        {
            _genericType = genericType;
            _argumentTypes = argumentTypes;
            _namespaceName = namespaceName;
            _scriptsPath = scriptsPath;
            _fileName = fileName;
        }

        public void CreateAsset()
        {
            if (GenericSODatabase.ContainsKey(_genericType, _argumentTypes))
            {
                CreateAssetInteractively();
                return;
            }

            Type genericTypeWithArgs = _genericType.MakeGenericType(_argumentTypes);

            Type existingAssetType = GetEmptyTypeDerivedFrom(genericTypeWithArgs);

            if (existingAssetType != null)
            {
                CreateAssetFromExistingType(existingAssetType);
                return;
            }

            AssetCreatorPersistentStorage.SaveForAssemblyReload(genericTypeWithArgs, _namespaceName, _scriptsPath, _fileName);
            string className = GetUniqueClassName();
            CreateScript(className);
        }

        public void CreateAssetFromExistingType()
        {
            Type genericTypeWithArgs = _genericType.MakeGenericType(_argumentTypes);
            Type existingAssetType = GetEmptyTypeDerivedFrom(genericTypeWithArgs);
            Assert.IsNotNull(existingAssetType);
            CreateAssetFromExistingType(existingAssetType);
        }

        public static string GetClassSafeTypeName(string rawTypeName)
        {
            return rawTypeName
                .Replace('.', '_')
                .Replace('`', '_');
        }

        private static string GetTypeNameWithoutAssembly(string fullTypeName)
            => fullTypeName.Split('[')[0];

        [CanBeNull]
        private static Type GetEmptyTypeDerivedFrom(Type parentType)
        {
            TypeCache.TypeCollection foundTypes = TypeCache.GetTypesDerivedFrom(parentType);

            if (foundTypes.Count == 0)
                return null;

            // Why would there be another empty type derived from GenericScriptableObject?
            Assert.IsTrue(foundTypes.Count == 1);

            Type matchingType = foundTypes.FirstOrDefault(type => type.IsEmpty());
            return matchingType;
        }

        /// <summary>
        /// The method first creates a class name that consists of the generic type name and names of the arguments.
        /// If such a name already exists, it adds an index to the name (1, 2, and so on.)
        /// </summary>
        /// <returns>Unique class name.</returns>
        private string GetUniqueClassName()
        {
            string argumentNames = string.Join("_", _argumentTypes.Select(type => GetClassSafeTypeName(type.Name)));

            int duplicationSuffix = 0;
            string defaultClassName = $"{GetClassSafeTypeName(_genericType.Name)}_{argumentNames}";

            string GetClassName()
            {
                return duplicationSuffix == 0 ? defaultClassName : $"{defaultClassName}_{duplicationSuffix}";
            }

            while (AssetDatabase.FindAssets(GetClassName()).Length != 0)
                duplicationSuffix++;

            return GetClassName();
        }

        private void CreateAssetFromExistingType(Type assetType)
        {
            GenericSODatabase.Add(_genericType, _argumentTypes, assetType);
            CreateAssetInteractively();
        }

        private void CreateScript(string className)
        {
#if UNITY_EDITOR_WIN
            const string longPathPrefix = @"\\?\";
            string fullAssetPath = $"{longPathPrefix}{Application.dataPath}/{_scriptsPath}/{className}.cs";

            if (fullAssetPath.Length > 260 + longPathPrefix.Length)
            {
                Debug.LogWarning("The generated script has a path over 260 characters. It may cause issues " +
                                 $"on Windows (e.g. with git pull/push). Path: {fullAssetPath}");
            }
#else
            string fullAssetPath = $"{Application.dataPath}/{_scriptsPath}/{className}.cs";
#endif

            string scriptContent = GetScriptContent(className);

            AssetDatabaseHelper.MakeSureFolderExists(_scriptsPath);
            File.WriteAllText(fullAssetPath, scriptContent);
            AssetDatabase.Refresh();
        }

        private string GetScriptContent(string className)
        {
            string genericTypeNameWithoutParam = _genericType.Name.Split('`')[0];
            string paramTypeNames = string.Join(", ", _argumentTypes
                .Select(type => GetTypeNameWithoutAssembly(type.FullName)));

            return $"namespace {_namespaceName} {{ public class {className} : " +
                   $"{_genericType.Namespace}.{genericTypeNameWithoutParam}<{paramTypeNames}> {{ }} }}";
        }

        private void CreateAssetInteractively()
        {
            var asset = GenericScriptableObject.CreateInstance(_genericType, _argumentTypes);
            Assert.IsNotNull(asset);
            AssetCreator.Create(asset, $"{_fileName}.asset");
        }
    }
}