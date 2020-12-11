namespace GenericScriptableObjects.Editor.AssetCreation
{
    using System;
    using System.Linq;
    using SolidUtilities.Extensions;
    using SolidUtilities.Helpers;
    using TypeSelectionWindows;
    using UnityEditor.Callbacks;
    using UnityEngine;
    using Util;

    /// <summary>
    /// Inherit from this class and use the <see cref="CreateAsset"/> method to create an AssetCreate menu.
    /// </summary>
    public class GenericSOCreator
    {
        /// <summary>
        /// Creates a <see cref="GenericScriptableObject"/> asset when used in a method with the
        /// <see cref="UnityEditor.MenuItem"/> attribute. Use it in classes that derive from <see cref="GenericSOCreator"/>.
        /// </summary>
        /// <param name="genericType">The type of <see cref="GenericScriptableObject"/> to create.</param>
        /// <param name="namespaceName">Custom namespace name to set for auto-generated non-generic types.</param>
        /// <param name="scriptsPath">Path to a folder where auto-generated non-generic types must be kept.</param>
        /// <param name="fileName">Name for an asset.</param>
        protected static void CreateAsset(
            Type genericType, string fileName)
        {
            genericType = TypeHelper.MakeGenericTypeDefinition(genericType);
            var constraints = genericType.GetGenericArguments()
                .Select(type => type.GetGenericParameterConstraints())
                .ToArray();

            TypeSelectionWindow.Create(constraints, paramTypes =>
            {
                var creator = new AssetCreatorHelper(genericType, paramTypes, fileName);
                creator.CreateAsset();
            });
        }

        [DidReloadScripts]
        private static void OnScriptsReload()
        {
            if ( ! GenericObjectsPersistentStorage.NeedsSOCreation)
                return;

            try
            {
                Type genericTypeWithoutArgs = GenericObjectsPersistentStorage.GenericSOType.Type.GetGenericTypeDefinition();
                var paramTypes = GenericObjectsPersistentStorage.GenericSOType.Type.GenericTypeArguments;

                var creator = new AssetCreatorHelper(
                    genericTypeWithoutArgs,
                    paramTypes,
                    GenericObjectsPersistentStorage.FileName);

                creator.CreateAssetFromExistingType();
            }
            finally
            {
                GenericObjectsPersistentStorage.Clear();
            }
        }
    }
}