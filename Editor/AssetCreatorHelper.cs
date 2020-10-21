namespace GenericScriptableObjects.Editor
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using JetBrains.Annotations;
    using SolidUtilities.Editor.EditorWindows;
    using SolidUtilities.Editor.Helpers;
    using SolidUtilities.Extensions;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Assertions;

    /// <summary>
    /// The class, responsible for creating a <see cref="GenericScriptableObject"/> asset when the context menu
    /// button is pressed.
    /// </summary>
    internal class AssetCreatorHelper
    {
        private readonly Type _genericType;
        private readonly Type[] _paramTypes;
        private readonly string[] _paramTypeNamesWithoutAssembly;
        private readonly string _className;
        private readonly string _defaultAssetName;
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

            var genericTypeClassSafeName = GetClassSafeTypeName(_genericType.Name);
            string paramTypesClassSafeNames =
                string.Join("_", _paramTypeNamesWithoutAssembly.Select(GetClassSafeTypeName));

            _className = $"{genericTypeClassSafeName}_{paramTypesClassSafeNames}";
            _defaultAssetName = $"New {genericTypeClassSafeName}.asset";
        }

        /// <summary>
        /// Starts the process of creating an asset. The assembly reload may be needed to finish the process if a
        /// concrete class implementation is not created yet.
        /// </summary>
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

            GenericSOCreator.SaveForAssemblyReload(genericTypeWithArgs, _namespaceName, _scriptsPath);
            CreateScript();
        }

        /// <summary>
        /// Creates a <see cref="GenericScriptableObject"/> asset, but only if its concrete class implementation
        /// already exists.
        /// </summary>
        public void CreateAssetFromExistingType()
        {
            Type genericTypeWithArgs = _genericType.MakeGenericType(_paramTypes);
            Type existingAssetType = GetEmptyTypeDerivedFrom(genericTypeWithArgs);
            Assert.IsNotNull(existingAssetType);
            CreateAssetFromExistingType(existingAssetType);
        }

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

        private static string GetClassSafeTypeName(string rawTypeName)
        {
            return rawTypeName
                .Replace('.', '_')
                .Replace('`', '_');
        }

        private static string GetTypeNameWithoutAssembly(string fullTypeName)
            => fullTypeName.Split('[')[0];

        private void CreateAssetFromExistingType(Type assetType)
        {
            GenericSODatabase.Add(_genericType, _paramTypes, assetType);
            CreateAssetInteractively();
        }

        private void CreateScript()
        {
            string fullAssetPath = $"{Application.dataPath}/{_scriptsPath}/{_className}.cs";
            string scriptContent = GetScriptContent();

            AssetDatabaseHelper.MakeSureFolderExists(_scriptsPath);
            File.WriteAllText(fullAssetPath, scriptContent);
            AssetDatabase.Refresh();
        }

        private string GetScriptContent()
        {
            string genericTypeNameWithoutParam = _genericType.Name.Split('`')[0];
            string paramTypeNames = string.Join(", ", _paramTypeNamesWithoutAssembly);

            return $"namespace {_namespaceName} {{ public class {_className} : " +
                   $"{_genericType.Namespace}.{genericTypeNameWithoutParam}<{paramTypeNames}> {{ }} }}";
        }

        private void CreateAssetInteractively()
        {
            var asset = GenericScriptableObject.CreateInstance(_genericType, _paramTypes);
            Assert.IsNotNull(asset);
            AssetCreator.Create(asset, _defaultAssetName);
        }
    }
}