namespace GenericScriptableObjects.Editor
{
    using System;
    using System.Linq;
    using SolidUtilities.Extensions;
    using SolidUtilities.Helpers;
    using TypeSelectionWindows;
    using UnityEditor.Callbacks;
    using UnityEngine;

    /// <summary>
    /// Inherit from this class and use the <see cref="CreateAsset"/> method to create an AssetCreate menu.
    /// </summary>
    public static class GenericSOCreator
    {
        public const string AssetCreatePath = "Assets/Create/";

        private const string DefaultNamespaceName = "GenericSOTypes";
        private const string DefaultScriptsPath = "Scripts/GenericSOTypes";

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
                var creator = new AssetCreatorHelper(genericType, paramTypes, namespaceName, scriptsPath);
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

                var creator = new AssetCreatorHelper(
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
    }
}