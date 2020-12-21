namespace GenericUnityObjects.Editor.ScriptableObject
{
    using System;
    using System.IO;
    using System.Linq;
    using GenericUnityObjects;
    using SolidUtilities.Editor.EditorWindows;
    using SolidUtilities.Helpers;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;
    using UnityEngine.Assertions;
    using Util;
    using TypeHelper = Util.TypeHelper;

    /// <summary>
    /// Inherit from this class and use the <see cref="CreateAsset"/> method to create an AssetCreate menu.
    /// </summary>
    public class GenericSOCreator
    {
        /// <summary>
        /// Creates a <see cref="GenericScriptableObject"/> asset when used in a method with the
        /// <see cref="UnityEditor.MenuItem"/> attribute. Use it in classes that derive from <see cref="GenericSOCreator"/>.
        /// </summary>
        /// <param name="genericTypeWithoutArgs">The type of <see cref="GenericScriptableObject"/> to create.</param>
        /// <param name="fileName">Name for an asset.</param>
        protected static void CreateAsset(Type genericTypeWithoutArgs, string fileName)
        {
            Assert.IsTrue(genericTypeWithoutArgs.IsGenericTypeDefinition);

            var constraints = genericTypeWithoutArgs.GetGenericArguments()
                .Select(type => type.GetGenericParameterConstraints())
                .ToArray();

            TypeSelectionWindow.Create(constraints, genericArgs =>
            {
                var creator = new GenericSOCreator(genericTypeWithoutArgs, genericArgs, fileName);
                creator.CreateAsset();
            });
        }

        [DidReloadScripts]
        private static void OnScriptsReload()
        {
            if ( ! PersistentStorage.NeedsSOCreation)
                return;

            try
            {
                (Type genericType, string fileName) = PersistentStorage.GetGenericSODetails();
                Type genericTypeWithoutArgs = genericType.GetGenericTypeDefinition();
                var genericArgs = genericType.GetGenericArguments();

                var creator = new GenericSOCreator(genericTypeWithoutArgs, genericArgs, fileName);
                creator.CreateAssetFromExistingType();
            }
            finally
            {
                PersistentStorage.Clear();
            }
        }

        private readonly Type _genericTypeWithoutArgs;
        private readonly Type[] _genericArgs;
        private readonly string _fileName;

        protected GenericSOCreator() { }

        private GenericSOCreator(Type genericTypeWithoutArgs, Type[] genericArgs, string fileName)
        {
            _genericTypeWithoutArgs = genericTypeWithoutArgs;
            _genericArgs = genericArgs;
            _fileName = fileName;
        }

        private void CreateAsset()
        {
            if (GenericObjectDatabase.ContainsKey(_genericTypeWithoutArgs, _genericArgs))
            {
                CreateAssetInteractively();
                return;
            }

            Type genericTypeWithArgs = _genericTypeWithoutArgs.MakeGenericType(_genericArgs);

            Type existingAssetType = TypeHelper.GetEmptyTypeDerivedFrom(genericTypeWithArgs);

            if (existingAssetType != null)
            {
                CreateAssetFromExistingType(existingAssetType);
                return;
            }

            PersistentStorage.SaveForAssemblyReload(genericTypeWithArgs, _fileName);
            string className = GetUniqueClassName();
            CreateScript(className);
        }

        private void CreateAssetFromExistingType()
        {
            Type genericTypeWithArgs = _genericTypeWithoutArgs.MakeGenericType(_genericArgs);
            Type existingAssetType = TypeHelper.GetEmptyTypeDerivedFrom(genericTypeWithArgs);
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
            string argumentNames = string.Join("_", _genericArgs.Select(type => type.Name.MakeClassFriendly()));
            string className = $"{_genericTypeWithoutArgs.Name.MakeClassFriendly().StripGenericSuffix()}_{argumentNames}";
            int identicalNamesNum = AssetDatabase.FindAssets(className, new[] { Config.GeneratedTypesPath }).Length;
            return identicalNamesNum == 0 ? className : $"{className}_{identicalNamesNum}";
        }

        private void CreateAssetFromExistingType(Type assetType)
        {
            GenericObjectDatabase.Add(_genericTypeWithoutArgs, _genericArgs, assetType);
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
            string genericTypeWithBrackets = CreatorUtil.GetFullNameWithBrackets(_genericTypeWithoutArgs, _genericArgs);
            return $"namespace {Config.GeneratedTypesNamespace} {{ internal class {className} : {genericTypeWithBrackets} {{ }} }}";
        }

        private void CreateAssetInteractively()
        {
            Type typeToCreate = _genericTypeWithoutArgs.MakeGenericType(_genericArgs);
            var asset = GenericScriptableObject.CreateInstance(typeToCreate);
            Assert.IsNotNull(asset);
            AssetCreator.Create(asset, $"{_fileName}.asset");
        }
    }
}