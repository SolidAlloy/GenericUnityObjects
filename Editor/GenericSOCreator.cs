namespace GenericScriptableObjects.Editor
{
    using System;
    using System.Linq;
    using SolidUtilities.Helpers;
    using TypeReferences;
    using TypeSelectionWindows;
    using UnityEditor.Callbacks;
    using UnityEngine;
    using Util;

    /// <summary>
    /// Inherit from this class and use the <see cref="CreateAsset"/> method to create an AssetCreate menu.
    /// </summary>
    public class GenericSOCreator : SingletonScriptableObject<GenericSOCreator>
    {
        /// <summary>
        /// If the concrete implementation of a <see cref="GenericScriptableObject"/>-derived type was not created yet
        /// and assemblies need to be reloaded, this field stores the type of <see cref="GenericScriptableObject"/> to
        /// create while the assemblies are reloaded.
        /// </summary>
        [HideInInspector] public TypeReference GenericTypeToCreate;

        protected const string AssetCreatePath = "Assets/Create/";

        /// <summary>
        /// Creates a <see cref="GenericScriptableObject"/> asset when used in a method with the
        /// <see cref="UnityEditor.MenuItem"/> attribute. Use it in classes that derive from <see cref="GenericSOCreator"/>.
        /// </summary>
        /// <param name="genericType">The type of <see cref="GenericScriptableObject"/> to create.</param>
        protected static void CreateAsset(Type genericType)
        {
            genericType = TypeHelper.MakeGenericTypeDefinition(genericType);
            var constraints = genericType.GetGenericArguments()
                .Select(type => type.GetGenericParameterConstraints())
                .ToArray();

            TypeSelectionWindow.Create(constraints, paramTypes =>
            {
                var creator = new AssetCreatorHelper(genericType, paramTypes);
                creator.CreateAsset();
            });
        }

        [DidReloadScripts]
        private static void OnScriptsReload()
        {
            if (Instance.GenericTypeToCreate.Type == null)
                return;

            try
            {
                Type genericTypeWithoutArgs = Instance.GenericTypeToCreate.Type.GetGenericTypeDefinition();
                var paramTypes = Instance.GenericTypeToCreate.Type.GenericTypeArguments;
                var creator = new AssetCreatorHelper(genericTypeWithoutArgs, paramTypes);
                creator.CreateAssetFromExistingType();
            }
            finally
            {
                Instance.GenericTypeToCreate = null;
            }
        }
    }
}