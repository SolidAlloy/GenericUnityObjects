namespace GenericUnityObjects.Util
{
    using System;
    using System.IO;
    using UnityEngine;

    /// <summary>
    /// Holder of constant values used by multiple classes in the plugin.
    /// </summary>
    internal static class Config
    {
        public const string ResourcesPath = MainFolderPath + "/Resources";
        public const string EditorResourcesPath = MainFolderPath + "/EditorResources";
        public const string ScriptableObjectsPath = AssembliesDirPath + "/ScriptableObjects";
        public const string MonoBehavioursPath = AssembliesDirPath + "/MonoBehaviours";
        public const string BehaviourSelectorsPath = AssembliesDirPath + "/BehaviourSelectors";
        public const string AssembliesDirPath = MainFolderPath + "/Assemblies";
        public const string ConcreteClassNamespace = "GenericUnityObjects.ConcreteClasses";
        private const string MainFolderPath = "Assets/Plugins/GenericUnityObjects";

        public static string GetAssemblyPathForType(Type parentType)
        {
            if (typeof(ScriptableObject).IsAssignableFrom(parentType))
            {
                return ScriptableObjectsPath;
            }

            if (typeof(BehaviourSelector).IsAssignableFrom(parentType))
            {
                return BehaviourSelectorsPath;
            }

            if (typeof(MonoBehaviour).IsAssignableFrom(parentType))
            {
                return MonoBehavioursPath;
            }

            return AssembliesDirPath;
        }
    }
}