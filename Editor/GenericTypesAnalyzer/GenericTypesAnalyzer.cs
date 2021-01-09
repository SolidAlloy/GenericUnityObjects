namespace GenericUnityObjects.Editor
{
    using System.IO;
    using GenericUnityObjects.Util;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using Util;

    internal static class GenericTypesAnalyzer
    {
        [DidReloadScripts(Config.AssemblyGenerationOrder)]
        private static void AnalyzeBehaviours()
        {
            bool needsAssetDatabaseRefresh;

            var behavioursChecker = new BehavioursChecker();

            using (new DisabledAssetDatabase(null))
            {
                Directory.CreateDirectory(Config.AssembliesDirPath);

                needsAssetDatabaseRefresh =
                    ArgumentsChecker<UnityEngine.MonoBehaviour>.Check(behavioursChecker)
                    | behavioursChecker.Check();
            }

            if (needsAssetDatabaseRefresh)
                AssetDatabase.Refresh();

            DictInitializer<UnityEngine.MonoBehaviour>.Initialize();
        }

        [DidReloadScripts(Config.AssemblyGenerationOrder)]
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