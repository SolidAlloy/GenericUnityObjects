namespace GenericUnityObjects.Editor
{
    using System.IO;
    using GenericUnityObjects.Util;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;
    using Util;

    internal static class GenericTypesAnalyzer
    {
        [DidReloadScripts((int)DidReloadScriptsOrder.AssemblyGeneration)]
        private static void AnalyzeBehaviours()
        {
            bool needsAssetDatabaseRefresh;

            var behavioursChecker = new BehavioursChecker();

            using (new DisabledAssetDatabase(null))
            {
                Directory.CreateDirectory(Config.AssembliesDirPath);

                needsAssetDatabaseRefresh =
                    ArgumentsChecker<MonoBehaviour>.Check(behavioursChecker)
                    | behavioursChecker.Check();
            }

            if (needsAssetDatabaseRefresh)
                AssetDatabase.Refresh();

            DictInitializer<MonoBehaviour>.Initialize();
        }

        [DidReloadScripts((int)DidReloadScriptsOrder.AssemblyGeneration)]
        private static void AnalyzeScriptableObjects()
        {
            bool needsAssetDatabaseRefresh;

            var scriptableObjectsChecker = new ScriptableObjectsChecker();

            using (new DisabledAssetDatabase(null))
            {
                Directory.CreateDirectory(Config.AssembliesDirPath);

                needsAssetDatabaseRefresh =
                    ArgumentsChecker<GenericScriptableObject>.Check(scriptableObjectsChecker)
                    | scriptableObjectsChecker.Check()
                    | MenuItemsChecker.Check();
            }

            if (needsAssetDatabaseRefresh)
                AssetDatabase.Refresh();

            DictInitializer<GenericScriptableObject>.Initialize();
        }
    }
}