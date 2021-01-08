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

            using (new DisabledAssetDatabase(null))
            {
                Directory.CreateDirectory(Config.AssembliesDirPath);

                needsAssetDatabaseRefresh =
                    GenericTypesAnalyzer<UnityEngine.MonoBehaviour>.ArgumentsChecker.Check()
                    || GenericTypesAnalyzer<UnityEngine.MonoBehaviour>.BehavioursChecker.Check();
            }

            if (needsAssetDatabaseRefresh)
                AssetDatabase.Refresh();

            BehavioursDatabase.Initialize(GenericTypesAnalyzer<UnityEngine.MonoBehaviour>.GetDictForInitialization());
        }

        [DidReloadScripts(Config.AssemblyGenerationOrder)]
        private static void AnalyzeScriptableObjects()
        {
            bool needsAssetDatabaseRefresh;

            using (new DisabledAssetDatabase(null))
            {
                Directory.CreateDirectory(Config.AssembliesDirPath);

                needsAssetDatabaseRefresh =
                    GenericTypesAnalyzer<UnityEngine.ScriptableObject>.ArgumentsChecker.Check()
                    || GenericTypesAnalyzer<UnityEngine.ScriptableObject>.ScriptableObjectsChecker.Check()
                    || GenericTypesAnalyzer<UnityEngine.ScriptableObject>.MenuItemsChecker.Check();
            }

            if (needsAssetDatabaseRefresh)
                AssetDatabase.Refresh();

            ScriptableObjectsDatabase.Initialize(GenericTypesAnalyzer<UnityEngine.ScriptableObject>.GetDictForInitialization());
        }
    }
}