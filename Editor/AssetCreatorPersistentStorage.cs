namespace GenericScriptableObjects.Editor
{
    using System;
    using TypeReferences;
    using UnityEngine;
    using Util;

    internal class AssetCreatorPersistentStorage : SingletonScriptableObject<AssetCreatorPersistentStorage>
    {
        [HideInInspector]
        [SerializeField] private TypeReference _genericType;

        [HideInInspector]
        [SerializeField] private string _namespaceName;

        [HideInInspector]
        [SerializeField] private string _scriptsPath;

        [HideInInspector]
        [SerializeField] private string _fileName;

        [HideInInspector]
        [SerializeField] private string[] _typesWithAttribute;

        public static bool IsEmpty => Instance._genericType.Type == null;

        public static TypeReference GenericType => Instance._genericType;
        public static string NamespaceName => Instance._namespaceName;
        public static string ScriptsPath => Instance._scriptsPath;
        public static string FileName => Instance._fileName;
        public static string[] TypesWithAttribute
        {
            get => Instance._typesWithAttribute;
            set => Instance._typesWithAttribute = value;
        }

        public static void SaveForAssemblyReload(Type genericTypeToCreate, string namespaceName, string scriptsPath, string fileName)
        {
            Instance._genericType = genericTypeToCreate;
            Instance._namespaceName = namespaceName;
            Instance._scriptsPath = scriptsPath;
            Instance._fileName = fileName;
        }

        public static void Clear()
        {
            Instance._genericType = null;
            Instance._namespaceName = null;
            Instance._scriptsPath = null;
            Instance._fileName = null;
        }
    }
}