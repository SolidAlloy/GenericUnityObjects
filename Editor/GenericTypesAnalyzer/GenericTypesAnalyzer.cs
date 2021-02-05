namespace GenericUnityObjects.Editor
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using GenericUnityObjects.Util;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;
    using Util;

    /// <summary>
    /// A class that gathers all changes related to generic types and generates/updates/removes DLLs based on the changes.
    /// </summary>
    internal static class GenericTypesAnalyzer
    {
        [DidReloadScripts((int)DidReloadScriptsOrder.AssemblyGeneration)]
        [SuppressMessage("ReSharper", "RCS1233",
            Justification = "We need | instead of || so that all methods are executed before moving to the next statement.")]
        private static void AnalyzeGenericTypes()
        {
#if GENERIC_UNITY_OBJECTS_DEBUG
            using var timer = Timer.CheckInMilliseconds("AnalyzeGenericTypes");
#endif

            bool behavioursNeedDatabaseRefresh;
            bool scriptableObjectsNeedDatabaseRefresh;

            var behavioursChecker = new BehavioursChecker();
            var scriptableObjectsChecker = new ScriptableObjectsChecker();

            using (new DisabledAssetDatabase(true))
            {
                Directory.CreateDirectory(Config.AssembliesDirPath);

                behavioursNeedDatabaseRefresh =
                    ArgumentsChecker<MonoBehaviour>.Check(behavioursChecker)
                    | behavioursChecker.Check();

                scriptableObjectsNeedDatabaseRefresh =
                    ArgumentsChecker<GenericScriptableObject>.Check(scriptableObjectsChecker)
                    | scriptableObjectsChecker.Check()
                    | MenuItemsChecker.Check();
            }

            if (behavioursNeedDatabaseRefresh || scriptableObjectsNeedDatabaseRefresh)
                AssetDatabase.Refresh();

            DictInitializer<MonoBehaviour>.Initialize();
            DictInitializer<GenericScriptableObject>.Initialize();
        }
    }
}