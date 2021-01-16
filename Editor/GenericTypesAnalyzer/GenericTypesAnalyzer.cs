namespace GenericUnityObjects.Editor
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using GenericUnityObjects.Util;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;
    using Util;

    internal static class GenericTypesAnalyzer
    {
        [DidReloadScripts((int)DidReloadScriptsOrder.AssemblyGeneration)]
        [SuppressMessage("ReSharper", "RCS1233",
            Justification = "We need | instead of || so that all methods are executed before moving to the next statement.")]
        private static void AnalyzeGenericTypes()
        {
            bool behavioursNeedDatabaseRefresh;
            bool scriptableObjectsNeedDatabaseRefresh;

            var behavioursChecker = new BehavioursChecker();
            var scriptableObjectsChecker = new ScriptableObjectsChecker();

            using (new DisabledAssetDatabase(null))
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