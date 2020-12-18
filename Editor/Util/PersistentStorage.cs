namespace GenericUnityObjects.Editor
{
    using System;
    using MonoBehaviour;
    using ScriptableObject;
    using TypeReferences;
    using UnityEditor;
    using UnityEngine;
    using Util;

    /// <summary>
    /// A class used to hold serialized values that need to survive assemblies reload. It is mainly used for asset
    /// creation, but also for MenuItem methods creation and Usage Example installation.
    /// </summary>
    internal class PersistentStorage : SingletonScriptableObject<PersistentStorage>
    {
        [HideInInspector] [SerializeField] private TypeReference _genericSOType;
        [HideInInspector] [SerializeField] private string _fileName;

        [HideInInspector] [SerializeField] private GameObject _gameObject;
        [HideInInspector] [SerializeField] private TypeReference _genericBehaviourType;

        [HideInInspector] [SerializeField] private MenuItemMethod[] _menuItemMethods = { };
        [HideInInspector] [SerializeField] private bool _usageExampleTypesAreAdded;

        [HideInInspector] [SerializeField] private BehaviourInfo[] _behaviourInfos = { };

        public static bool NeedsSOCreation => CreatedOnlyInstance != null && CreatedOnlyInstance._genericSOType?.Type != null;

        public static bool NeedsBehaviourCreation => CreatedOnlyInstance != null && CreatedOnlyInstance._genericBehaviourType?.Type != null;

        public static MenuItemMethod[] MenuItemMethods
        {
            get => CreatedOnlyInstance == null ? null : CreatedOnlyInstance._menuItemMethods;
            set
            {
                CreatedOnlyInstance._menuItemMethods = value;
                EditorUtility.SetDirty(CreatedOnlyInstance);
            }
        }

        public static BehaviourInfo[] BehaviourInfos
        {
            get => CreatedOnlyInstance == null ? null : CreatedOnlyInstance._behaviourInfos;
            set
            {
                CreatedOnlyInstance._behaviourInfos = value;
                EditorUtility.SetDirty(CreatedOnlyInstance);
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

        public static void SaveForAssemblyReload(GameObject gameObject, Type genericType)
        {
            Instance._gameObject = gameObject;
            Instance._genericBehaviourType = genericType;
        }

        public static (Type, string) GetGenericSODetails()
        {
            return (Instance._genericSOType, Instance._fileName);
        }

        public static (GameObject, Type) GetGenericBehaviourDetails()
        {
            return (Instance._gameObject, Instance._genericBehaviourType);
        }

        public static void Clear()
        {
            Instance._genericSOType = null;
            Instance._fileName = null;
            Instance._gameObject = null;
            Instance._genericBehaviourType = null;
        }
    }
}