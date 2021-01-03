namespace GenericUnityObjects.Editor.MonoBehaviour
{
    using System.IO;
    using GenericUnityObjects.Util;
    using ScriptableObject;
    using UnityEditor.Callbacks;
    using Util;

    internal static class GenericTypesAnalyzer
    {
        [DidReloadScripts(Config.AssemblyGenerationOrder)]
        private static void AnalyzeBehaviours()
        {
            var analyzer = new GenericTypesAnalyzer<BehavioursGenerationDatabase>();

            analyzer.RefreshDatabaseIfNeeded(() =>
            {
                CreatorUtil.WithDisabledAssetDatabase(() =>
                {
                    Directory.CreateDirectory(Config.AssembliesDirPath);
                    analyzer.CheckArguments();
                    analyzer.CheckBehaviours();
                });

                BehavioursDatabase.Initialize(analyzer.GetDictForInitialization());
            });
        }

        // [DidReloadScripts(Config.AssemblyGenerationOrder)]
        private static void AnalyzeScriptableObjects()
        {
            var analyzer = new GenericTypesAnalyzer<SOGenerationDatabase>();

            analyzer.RefreshDatabaseIfNeeded(() =>
            {
                CreatorUtil.WithDisabledAssetDatabase(() =>
                {
                    Directory.CreateDirectory(Config.AssembliesDirPath);
                    analyzer.CheckArguments();
                    analyzer.CheckBehaviours();
                });

                ScriptableObjectsDatabase.Initialize(analyzer.GetDictForInitialization());
            });
        }
    }
}