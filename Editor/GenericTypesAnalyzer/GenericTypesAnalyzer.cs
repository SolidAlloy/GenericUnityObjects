namespace GenericUnityObjects.Editor
{
    using System.IO;
    using GeneratedTypesDatabase;
    using GenericUnityObjects.Util;
    using UnityEditor.Callbacks;
    using Util;

    internal static class GenericTypesAnalyzer
    {
        [DidReloadScripts(Config.AssemblyGenerationOrder)]
        private static void AnalyzeBehaviours()
        {
            AssetDatabaseHelper.RefreshDatabaseIfNeeded(() =>
            {
                bool needsAssetDatabaseRefresh = false;

                AssetDatabaseHelper.WithDisabledAssetDatabase(() =>
                {
                    Directory.CreateDirectory(Config.AssembliesDirPath);

                    needsAssetDatabaseRefresh =
                        GenericTypesAnalyzer<BehavioursGenerationDatabase>.CheckArguments()
                        || GenericTypesAnalyzer<BehavioursGenerationDatabase>.CheckBehaviours();
                });

                return needsAssetDatabaseRefresh;
            });

            BehavioursDatabase.Initialize(GenericTypesAnalyzer<BehavioursGenerationDatabase>.GetDictForInitialization());
        }

        [DidReloadScripts(Config.AssemblyGenerationOrder)]
        private static void AnalyzeScriptableObjects()
        {
            AssetDatabaseHelper.RefreshDatabaseIfNeeded(() =>
            {
                bool needsAssetDatabaseRefresh = false;

                AssetDatabaseHelper.WithDisabledAssetDatabase(() =>
                {
                    Directory.CreateDirectory(Config.AssembliesDirPath);

                    needsAssetDatabaseRefresh =
                        GenericTypesAnalyzer<SOGenerationDatabase>.CheckArguments()
                        || GenericTypesAnalyzer<SOGenerationDatabase>.CheckScriptableObjects()
                        || GenericTypesAnalyzer<SOGenerationDatabase>.CheckMenuItems();
                });

                return needsAssetDatabaseRefresh;
            });

            ScriptableObjectsDatabase.Initialize(GenericTypesAnalyzer<SOGenerationDatabase>.GetDictForInitialization());
        }
    }
}