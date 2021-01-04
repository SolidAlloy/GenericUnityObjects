namespace GenericUnityObjects.Editor.MonoBehaviour
{
    using System;
    using System.IO;
    using GenericUnityObjects.Util;
    using ScriptableObject;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using Util;

    internal static class GenericTypesAnalyzer
    {
        [DidReloadScripts(Config.AssemblyGenerationOrder)]
        private static void AnalyzeBehaviours()
        {
            RefreshDatabaseIfNeeded(() =>
            {
                bool needsAssetDatabaseRefresh = false;

                CreatorUtil.WithDisabledAssetDatabase(() =>
                {
                    Directory.CreateDirectory(Config.AssembliesDirPath);
                    needsAssetDatabaseRefresh = GenericTypesAnalyzer<BehavioursGenerationDatabase>.CheckArguments();
                    needsAssetDatabaseRefresh = GenericTypesAnalyzer<BehavioursGenerationDatabase>.CheckBehaviours();
                });

                return needsAssetDatabaseRefresh;
            });

            BehavioursDatabase.Initialize(GenericTypesAnalyzer<BehavioursGenerationDatabase>.GetDictForInitialization());
        }

        [DidReloadScripts(Config.AssemblyGenerationOrder)]
        private static void AnalyzeScriptableObjects()
        {
            RefreshDatabaseIfNeeded(() =>
            {
                bool needsAssetDatabaseRefresh = false;

                CreatorUtil.WithDisabledAssetDatabase(() =>
                {
                    Directory.CreateDirectory(Config.AssembliesDirPath);
                    needsAssetDatabaseRefresh = GenericTypesAnalyzer<SOGenerationDatabase>.CheckArguments();
                    needsAssetDatabaseRefresh = GenericTypesAnalyzer<SOGenerationDatabase>.CheckScriptableObjects();
                });

                return needsAssetDatabaseRefresh;
            });

            ScriptableObjectsDatabase.Initialize(GenericTypesAnalyzer<BehavioursGenerationDatabase>.GetDictForInitialization());
        }

        private static void RefreshDatabaseIfNeeded(Func<bool> doStuff)
        {
            if (doStuff())
                AssetDatabase.Refresh();
        }
    }
}