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
    using TypeSelectionWindows;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;
    using UnityEngine.Assertions;

    /// <summary>
    /// Inherit from this class and use the <see cref="CreateAsset"/> method to create an AssetCreate menu.
    /// </summary>
    public class GenericSOCreator
    {
        public const string AssetCreatePath = "Assets/Create/";

        private const string DefaultNamespaceName = "GenericScriptableObjectsTypes";
        private const string DefaultScriptsPath = "Scripts/GenericScriptableObjectTypes";

        private readonly Type _genericType;
        private readonly Type[] _paramTypes;
        private readonly string[] _paramTypeNamesWithoutAssembly;
        private readonly string _className;
        private readonly string _defaultAssetName;
        private readonly string _scriptsPath;
        private readonly string _namespaceName;

        private GenericSOCreator(Type genericType, Type[] paramTypes, string namespaceName, string scriptsPath)
        {
            _genericType = genericType;
            _paramTypes = paramTypes;
            _scriptsPath = scriptsPath;
            _namespaceName = namespaceName;

            _paramTypeNamesWithoutAssembly = _paramTypes
                .Select(type => GetTypeNameWithoutAssembly(type.FullName)).ToArray();

            string genericTypeClassSafeName = GetClassSafeTypeName(_genericType.Name);
            string paramTypesClassSafeNames =
                string.Join("_", _paramTypeNamesWithoutAssembly.Select(GetClassSafeTypeName));

            _className = $"{genericTypeClassSafeName}_{paramTypesClassSafeNames}";
            _defaultAssetName = $"New {genericTypeClassSafeName}.asset";
        }

        /// <summary>
        /// Creates a <see cref="GenericScriptableObject"/> asset when used in a method with the
        /// <see cref="UnityEditor.MenuItem"/> attribute. Use it in classes that derive from <see cref="GenericSOCreator"/>.
        /// </summary>
        /// <param name="genericType">The type of <see cref="GenericScriptableObject"/> to create.</param>
        /// <param name="namespaceName">Custom namespace name to set for auto-generated non-generic types.
        /// Default is "GenericScriptableObjectsTypes".</param>
        /// <param name="scriptsPath">Custom path to a folder where auto-generated non-generic types must be kept.
        /// Default is "Scripts/GenericScriptableObjectTypes".</param>
        public static void CreateAsset(
            Type genericType,
            string namespaceName = DefaultNamespaceName,
            string scriptsPath = DefaultScriptsPath)
        {
            namespaceName = ValidateNamespaceName(namespaceName);
            scriptsPath = ValidateScriptsPath(scriptsPath);

            genericType = TypeHelper.MakeGenericTypeDefinition(genericType);
            var constraints = genericType.GetGenericArguments()
                .Select(type => type.GetGenericParameterConstraints())
                .ToArray();

            TypeSelectionWindow.Create(constraints, paramTypes =>
            {
                var creator = new GenericSOCreator(genericType, paramTypes, namespaceName, scriptsPath);
                creator.CreateAsset();
            });
        }

        [DidReloadScripts]
        private static void OnScriptsReload()
        {
            if (AssetCreatorPersistentStorage.IsEmpty)
                return;

            try
            {
                Type genericTypeWithoutArgs = AssetCreatorPersistentStorage.GenericType.Type.GetGenericTypeDefinition();
                var paramTypes = AssetCreatorPersistentStorage.GenericType.Type.GenericTypeArguments;

                var creator = new GenericSOCreator(
                    genericTypeWithoutArgs,
                    paramTypes,
                    AssetCreatorPersistentStorage.NamespaceName,
                    AssetCreatorPersistentStorage.ScriptsPath);

                creator.CreateAssetFromExistingType();
            }
            finally
            {
                AssetCreatorPersistentStorage.Clear();
            }
        }

        private static string ValidateNamespaceName(string namespaceName)
        {
            if (namespaceName.IsValidIdentifier())
                return namespaceName;

            Debug.LogError($"The provided namespace name '{namespaceName}' is not a valid identifier.");
            namespaceName = DefaultNamespaceName;
            return namespaceName;
        }

        private static string ValidateScriptsPath(string scriptsPath)
        {
            if (scriptsPath.IsValidPath())
                return scriptsPath;

            Debug.LogError($"The provided path '{scriptsPath}' is not a valid Unity path. Restricted characters are /?<>\\:*|\"");
            scriptsPath = DefaultScriptsPath;
            return scriptsPath;
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

        private void CreateAsset()
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
            CreateScript();
        }

        private void CreateAssetFromExistingType(Type assetType)
        {
            GenericSODatabase.Add(_genericType, _paramTypes, assetType);
            CreateAssetInteractively();
        }

        private void CreateAssetFromExistingType()
        {
            Type genericTypeWithArgs = _genericType.MakeGenericType(_paramTypes);
            Type existingAssetType = GetEmptyTypeDerivedFrom(genericTypeWithArgs);
            Assert.IsNotNull(existingAssetType);
            CreateAssetFromExistingType(existingAssetType);
        }

        private void CreateScript()
        {
            string fullAssetPath = $@"\\?\{Application.dataPath}/{_scriptsPath}/{_className}.cs";
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