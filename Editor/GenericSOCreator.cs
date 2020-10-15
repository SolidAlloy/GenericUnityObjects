namespace GenericScriptableObjects.Editor
{
    using System;
    using JetBrains.Annotations;
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

        [SerializeField] [HideInInspector] private TypeReference _genericType;
        [SerializeField] [HideInInspector] private TypeReference[] _paramTypes;
        
        public void SetAssetToCreate([CanBeNull] Type genericType, [CanBeNull] Type[] paramTypes)
        {
            _genericType = genericType;
            _paramTypes = paramTypes?.CastToTypeReference();
        }

        protected static void CreateAsset(Type genericType)
        {
            genericType = TypeHelper.MakeGenericTypeDefinition(genericType);
            int typeParamCount = genericType.GetGenericArguments().Length;

            TypeSelectionWindow.Create(typeParamCount, paramTypes =>
            {
                var creator = new AssetCreatorHelper(genericType, paramTypes);
                creator.CreateAsset();
            });
        }

        [DidReloadScripts]
        private static void OnScriptsReload()
        {
            if (Instance._genericType.Type == null)
                return;

            try
            {
                var paramTypes = Instance._paramTypes.CastToType();
                var creator = new AssetCreatorHelper(Instance._genericType, paramTypes);
                creator.CreateAssetFromExistingType();
            }
            finally
            {
                Instance.SetAssetToCreate(null, null);
            }
        }
    }
}