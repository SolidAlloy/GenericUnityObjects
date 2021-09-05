namespace GenericUnityObjects.Editor
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using GenericUnityObjects.Util;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEditor.Compilation;
    using UnityEngine;
    using Util;

#if GENERIC_UNITY_OBJECTS_DEBUG
    using SolidUtilities.Helpers;
#endif

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
            try
            {
                if (CompilationFailedOnEditorStart())
                    return;

                EditorApplication.quitting += () => PersistentStorage.AssembliesCount = GetAssembliesCount();

                // If PlayOptions is disabled and the domain reload happens on entering Play Mode, no changes to scripts
                // can be detected but NullReferenceException is thrown from UnityEditor internals. Since it is useless
                // to check changes to scripts in this situation, we can safely ignore this domain reload.
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    UpdateGeneratedAssemblies();
                }
            }
            catch (ApplicationException)
            {
                Debug.LogWarning("Editor could not load some of the scriptable objects from plugin's resources. It will try on next assembly reload.");
                CompilationHelper.RecompileOnce();
                return;
            }

            DictInitializer<MonoBehaviour>.Initialize();
            DictInitializer<GenericScriptableObject>.Initialize();
            FailedAssembliesChecker.ReimportFailedAssemblies();
        }

        private static bool CompilationFailedOnEditorStart()
        {
            // There is an issue in Unity causing some generic unity objects to be re-generated, and references to them are lost:
            // If there is a compilation error when you open Unity, not all assemblies are compiled. However, DidReloadScripts is executed anyway.
            // This causes TypeCache to not include some classes. The plugin thinks those classes are removed, however they are just located in uncompiled assemblies.
            // To avoid this, we need to compare the assemblies count on last Unity compilation before it exited, and the assemblies count now.
            // If it is not equal, it means the project was loaded with errors, and the generic classes check should not run this time.
            if ( ! PersistentStorage.FirstCompilation)
                return false;

            PersistentStorage.DisableFirstCompilation();
            return PersistentStorage.AssembliesCount != GetAssembliesCount();
        }

        private static int GetAssembliesCount()
        {
            return Directory.GetFiles("Library/ScriptAssemblies", "*.dll").Length;
        }

        private static void UpdateGeneratedAssemblies()
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
        }
    }
}