namespace GenericScriptableObjects.Editor
{
    using System;
    using System.ComponentModel;
    using GenericScriptableObjects;
    using JetBrains.Annotations;
    using SolidUtilities.Helpers;
    using TypeReferences;
    using UnityEditor.Callbacks;
    using UnityEngine;
    using Util;

    [TypeDescriptionProvider(typeof(GenericSODescriptionProvider))]
    public class GenericSOCreator : SingletonScriptableObject<GenericSOCreator>
    {
        protected const string AssetCreatePath = "Assets/Create/";

        [SerializeField] private TypeReference _genericType; // TODO: hide in inspector
        [SerializeField] private TypeReference[] _paramTypes;

        [DidReloadScripts]
        private static void OnScriptsReload()
        {
            if (Instance._genericType == null || Instance._genericType.Type == null) // TODO: check if TypeReference can be null at all.
                return;

            try
            {
                var paramTypes = Instance._paramTypes.CastToType();
                var creator = new Creator(Instance._genericType, paramTypes);
                creator.CreateAssetFromExistingType();
            }
            finally
            {
                Instance.SetAssetToCreate(null, null);
            }
        }

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
                var creator = new Creator(genericType, paramTypes);
                creator.CreateAsset();
            });
        }
    }
}