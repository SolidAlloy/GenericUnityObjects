namespace GenericScriptableObjects.Editor
{
    using System;
    using System.IO;
    using System.Linq;
    using JetBrains.Annotations;
    using SolidUtilities.Editor.EditorWindows;
    using SolidUtilities.Editor.Helpers;
    using SolidUtilities.Extensions;
    using SolidUtilities.Helpers;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Assertions;

    internal class AssetCreatorHelper
    {
        private readonly Type _genericType;
        private readonly Type[] _paramTypes;
        private readonly string[] _paramTypeNamesWithoutAssembly;
        private readonly string _genericTypeClassSafeName;
        private readonly string _scriptsPath;
        private readonly string _namespaceName;

        public AssetCreatorHelper(Type genericType, Type[] paramTypes, string namespaceName, string scriptsPath)
        {
            _genericType = genericType;
            _paramTypes = paramTypes;
            _scriptsPath = scriptsPath;
            _namespaceName = namespaceName;

            _paramTypeNamesWithoutAssembly = _paramTypes
                .Select(type => GetTypeNameWithoutAssembly(type.FullName)).ToArray();

            _genericTypeClassSafeName = GetClassSafeTypeName(_genericType.Name);
        }

        private string DefaultAssetName => $"New {_genericTypeClassSafeName}.asset";

        public void CreateAsset()
        {
            if (GenericSODatabase.ContainsKey(_genericType, _paramTypes))
            {
                CreateAssetInteractively();
                return;
            }

            Type genericTypeWithArgs = _genericType.MakeGenericType(_paramTypes);

            Type existingAssetType = GetEmptyTypeDerivedFrom(genericTypeWithArgs);

            if (existingAssetType != null)
            {
                CreateAssetFromExistingType(existingAssetType);
                return;
            }

            AssetCreatorPersistentStorage.SaveForAssemblyReload(genericTypeWithArgs, _namespaceName, _scriptsPath);
            string className = GetUniqueClassName();
            CreateScript(className);
        }

        public void CreateAssetFromExistingType()
        {
            Type genericTypeWithArgs = _genericType.MakeGenericType(_paramTypes);
            Type existingAssetType = GetEmptyTypeDerivedFrom(genericTypeWithArgs);
            Assert.IsNotNull(existingAssetType);
            CreateAssetFromExistingType(existingAssetType);
        }

        private static string GetClassSafeTypeName(string rawTypeName)
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
        /// The method generates a SHA1 hash for the generic parameters and strips the length to 10 characters. If it
        /// finds a collision, it increases the string length by one char and continues to do so until no collisions
        /// are found.
        /// </summary>
        /// <returns></returns>
        private string GetUniqueClassName()
        {
            string input = string.Join("_", _paramTypeNamesWithoutAssembly);
            string hashString = Hash.SHA1(input);

            int suffixLength = 10;
            string suffix = hashString.Substring(0, suffixLength);

            string GetClassName() => $"{_genericTypeClassSafeName}_{suffix}";

            while (AssetDatabase.FindAssets(GetClassName()).Length != 0)
            {
                suffix = hashString.Substring(0, ++suffixLength);
            }

            return GetClassName();
        }

        private void CreateAssetFromExistingType(Type assetType)
        {
            GenericSODatabase.Add(_genericType, _paramTypes, assetType);
            CreateAssetInteractively();
        }

        private void CreateScript(string className)
        {
            string fullAssetPath = $@"\\?\{Application.dataPath}/{_scriptsPath}/{className}.cs";
            string scriptContent = GetScriptContent(className);

            AssetDatabaseHelper.MakeSureFolderExists(_scriptsPath);
            File.WriteAllText(fullAssetPath, scriptContent);
            AssetDatabase.Refresh();
        }

        private string GetScriptContent(string className)
        {
            string genericTypeNameWithoutParam = _genericType.Name.Split('`')[0];
            string paramTypeNames = string.Join(", ", _paramTypeNamesWithoutAssembly);

            return $"namespace {_namespaceName} {{ public class {className} : " +
                   $"{_genericType.Namespace}.{genericTypeNameWithoutParam}<{paramTypeNames}> {{ }} }}";
        }

        private void CreateAssetInteractively()
        {
            var asset = GenericScriptableObject.CreateInstance(_genericType, _paramTypes);
            Assert.IsNotNull(asset);
            AssetCreator.Create(asset, DefaultAssetName);
        }
    }
}