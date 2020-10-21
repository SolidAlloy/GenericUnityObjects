namespace GenericScriptableObjects.Editor
{
    using System;
    using System.Linq;
    using JetBrains.Annotations;
    using SolidUtilities.Extensions;
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
        protected const string AssetCreatePath = "Assets/Create/";

        /// <summary>
        /// If the concrete implementation of a <see cref="GenericScriptableObject"/>-derived type was not created yet
        /// and assemblies need to be reloaded, this field stores the type of <see cref="GenericScriptableObject"/> to
        /// create while the assemblies are reloaded.
        /// </summary>
        [HideInInspector]
        [SerializeField] private TypeReference GenericTypeToCreate;

        [HideInInspector]
        [SerializeField] private string NamespaceName;

        [HideInInspector]
        [SerializeField] private string ScriptsPath;

        internal static void SaveForAssemblyReload(Type genericTypeToCreate, string namespaceName, string scriptsPath)
        {
            Instance.GenericTypeToCreate = genericTypeToCreate;
            Instance.NamespaceName = namespaceName;
            Instance.ScriptsPath = scriptsPath;
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
        protected static void CreateAsset(
            Type genericType,
            string namespaceName = "GenericScriptableObjectsTypes",
            string scriptsPath = "Scripts/GenericScriptableObjectTypes")
        {
            ValidateNamespaceName(namespaceName);
            ValidateScriptsPath(scriptsPath);

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
            if (Instance.GenericTypeToCreate.Type == null)
                return;

            try
            {
                Type genericTypeWithoutArgs = Instance.GenericTypeToCreate.Type.GetGenericTypeDefinition();
                var paramTypes = Instance.GenericTypeToCreate.Type.GenericTypeArguments;
                var creator = new AssetCreatorHelper(genericTypeWithoutArgs, paramTypes, Instance.NamespaceName, Instance.ScriptsPath);
                creator.CreateAssetFromExistingType();
            }
            finally
            {
                Instance.GenericTypeToCreate = null;
                Instance.NamespaceName = null;
                Instance.ScriptsPath = null;
            }
        }

        private static void ValidateNamespaceName(string namespaceName)
        {
            if ( ! namespaceName.IsValidIdentifier())
                throw new ArgumentException(); // TODO: set message for argument exception
        }

        private static void ValidateScriptsPath(string scriptsPath)
        {

        }
    }
}