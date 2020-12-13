namespace GenericScriptableObjects.Editor.Util
{
    using System;
    using GenericScriptableObjects.Util;
    using MenuItemsGeneration;
    using TypeReferences;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// A class used to hold serialized values that need to survive assemblies reload. It is mainly used for asset
    /// creation, but also for MenuItem methods creation and Usage Example installation.
    /// </summary>
    public class PersistentStorage : SingletonScriptableObject<PersistentStorage>
    {
        [HideInInspector] [SerializeField] private TypeReference _genericSOType;
        [HideInInspector] [SerializeField] private string _fileName;

        [HideInInspector] [SerializeField] private GameObject _gameObject;
        [HideInInspector] [SerializeField] private TypeReference _genericBehaviourType;

        [HideInInspector] [SerializeField] private MenuItemMethod[] _menuItemMethods = { };
        [HideInInspector] [SerializeField] private bool _usageExampleTypesAreAdded;

        public static bool NeedsSOCreation => OnlyCreatedInstance != null && OnlyCreatedInstance._genericSOType?.Type != null;

        public static bool NeedsBehaviourCreation => OnlyCreatedInstance != null && OnlyCreatedInstance._genericBehaviourType?.Type != null;

        public static TypeReference GenericSOType => Instance._genericSOType;
        public static string FileName => Instance._fileName;

        public static MenuItemMethod[] MenuItemMethods
        {
            get => OnlyCreatedInstance == null ? null : OnlyCreatedInstance._menuItemMethods;
            set
            {
                OnlyCreatedInstance._menuItemMethods = value;
                EditorUtility.SetDirty(Instance);
            }
        }

        public static bool UsageExampleTypesAreAdded
        {
            get => Instance._usageExampleTypesAreAdded;
            set
            {
                Instance._usageExampleTypesAreAdded = value;
                EditorUtility.SetDirty(Instance);
            }
        }

        public static void SaveForAssemblyReload(Type genericTypeToCreate, string fileName)
        {
            Instance._genericSOType = genericTypeToCreate;
            Instance._fileName = fileName;
        }

        public static void Clear()
        {
            Instance._genericSOType = null;
            Instance._fileName = null;
            Instance._gameObject = null;
            Instance._genericBehaviourType = null;
        }

        public static void SaveForAssemblyReload(GameObject gameObject, Type genericType)
        {
            Instance._gameObject = gameObject;
            Instance._genericBehaviourType = genericType;
        }

        public static (GameObject, Type) GetGenericBehaviourDetails()
        {
            return (Instance._gameObject, Instance._genericBehaviourType);
        }
    }
}