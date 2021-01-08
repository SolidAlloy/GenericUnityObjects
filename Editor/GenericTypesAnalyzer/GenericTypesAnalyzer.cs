namespace GenericUnityObjects.Editor
{
    using System.IO;
    using GeneratedTypesDatabase;
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

            using (new DisabledAssetDatabase(null))
            {
                Directory.CreateDirectory(Config.AssembliesDirPath);

                needsAssetDatabaseRefresh =
                    GenericTypesAnalyzer<BehavioursGenerationDatabase>.CheckArguments()
                    || GenericTypesAnalyzer<BehavioursGenerationDatabase>.CheckBehaviours();
            }

            if (needsAssetDatabaseRefresh)
                AssetDatabase.Refresh();

            BehavioursDatabase.Initialize(GenericTypesAnalyzer<BehavioursGenerationDatabase>.GetDictForInitialization());
        }

        [DidReloadScripts(Config.AssemblyGenerationOrder)]
        private static void AnalyzeScriptableObjects()
        {
            bool needsAssetDatabaseRefresh;

            using (new DisabledAssetDatabase(null))
            {
                Directory.CreateDirectory(Config.AssembliesDirPath);

                needsAssetDatabaseRefresh =
                    GenericTypesAnalyzer<SOGenerationDatabase>.CheckArguments()
                    || GenericTypesAnalyzer<SOGenerationDatabase>.CheckScriptableObjects()
                    || GenericTypesAnalyzer<SOGenerationDatabase>.CheckMenuItems();
            }

            if (needsAssetDatabaseRefresh)
                AssetDatabase.Refresh();

            ScriptableObjectsDatabase.Initialize(GenericTypesAnalyzer<SOGenerationDatabase>.GetDictForInitialization());
        }
    }
}