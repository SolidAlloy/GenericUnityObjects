namespace GenericUnityObjects.Editor
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using GeneratedTypesDatabase;
    using GenericUnityObjects.Util;
    using SolidUtilities.Extensions;
    using UnityEditor;
    using UnityEngine;
    using Util;
    using Object = UnityEngine.Object;
#if GENERIC_UNITY_OBJECTS_DEBUG
    using SolidUtilities.Helpers;
#endif

    /// <summary>
    /// A class that gathers all changes related to generic types and generates/updates/removes DLLs based on the changes.
    /// </summary>
    internal static class GenericTypesAnalyzer
    {
        [SuppressMessage("ReSharper", "RCS1233",
            Justification = "We need | instead of || so that all methods are executed before moving to the next statement.")]
        public static void AnalyzeGenericTypes()
        {
#if GENERIC_UNITY_OBJECTS_DEBUG
            using var timer = Timer.CheckInMilliseconds("AnalyzeGenericTypes");
#endif

            if (CompilationFailedOnEditorStart())
                return;

            EditorApplication.quitting += () => PersistentStorage.AssembliesCount = GetAssembliesCount();

            // If PlayOptions is disabled and the domain reload happens on entering Play Mode, no changes to scripts
            // can be detected but NullReferenceException is thrown from UnityEditor internals. Since it is useless
            // to check changes to scripts in this situation, we can safely ignore this domain reload.
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                UpdateGeneratedAssemblies();
                // We don't check missing selector assemblies because a new one would be created in
                // UpdateGeneratedAssemblies anyway if a generic type exists in the project and is missing from the database.
                AddMissingConcreteClassesToDatabase<GenericScriptableObject>();
                AddMissingConcreteClassesToDatabase<MonoBehaviour>();
            }

            if (FailedAssembliesChecker.FailedAssemblyPaths.Count == 0)
            {
                DictInitializer<MonoBehaviour>.Initialize();
                DictInitializer<GenericScriptableObject>.Initialize();
            }

            CompilationHelper.CompilationNotNeeded();
            FailedAssembliesChecker.ReimportFailedAssemblies();
            FlushConfigChangesToDisk();
        }

        private static void AddMissingConcreteClassesToDatabase<TObject>()
            where TObject : Object
        {
            string folderPath = Config.GetAssemblyPathForType(typeof(TObject));

            // When Config.CreateNecessaryDirectories is run the first time, AssetDatabase does not import the folders,
            // so it throws warning thinking they don't exist.
            if (!AssetDatabase.IsValidFolder(folderPath))
                return;

            var guids = AssetDatabase.FindAssets(string.Empty, new[] { folderPath });

            int concreteClassesCount = GenerationDatabase<TObject>.GenericTypeArguments.Values
                .Select(concreteClasses => concreteClasses.Count).Sum();

            Debug.Log($"guids count {guids.Length}, concreteClasses count {concreteClassesCount}");

            // The guids count may be lower than the concrete class count because some assemblies are located in another folder
            if (guids.Length <= concreteClassesCount)
            {
                return;
            }

            // Have to use the extension method explicitly to avoid ambiguous reference error between SolidUtilities.ToHashSet and NetStandard 2.1 Enumerable.ToHashSet
            var registeredGuids = EnumerableExtensions.ToHashSet(GenerationDatabase<TObject>.GenericTypeArguments.Values
                    .Aggregate((allConcreteClasses, thisClasses) =>
                    {
                        allConcreteClasses.AddRange(thisClasses);
                        return allConcreteClasses;
                    })
                    .Select(concreteClass => concreteClass.AssemblyGUID));

            // If there are assemblies that exist in the folder but are missing from the database, add them to the database.
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

                if (monoScript == null)
                {
                    FailedAssembliesChecker.FailedAssemblyPaths.Add(path);
                    continue;
                }

                if (registeredGuids.Contains(guid))
                {
                    continue;
                }

                var concreteType = monoScript.GetClass();
                var genericType = concreteType?.BaseType;

                Type genericTypeWithoutArgs = genericType.GetGenericTypeDefinition();
                Type[] genericArgs = genericType.GenericTypeArguments;

                var genericTypeInfo = GenericTypeInfo.Instantiate<TObject>(genericTypeWithoutArgs);
                GenerationDatabase<TObject>.AddConcreteClass(genericTypeInfo, genericArgs, guid);
            }
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
            using var timer = Timer.CheckInMilliseconds("UpdateGeneratedAssemblies");
#endif

            bool behavioursNeedDatabaseRefresh;
            bool scriptableObjectsNeedDatabaseRefresh;

            var behavioursChecker = new BehavioursChecker();
            var scriptableObjectsChecker = new ScriptableObjectsChecker();

            using (new DisabledAssetDatabase(true))
            {
                Config.CreateNecessaryDirectories();

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

        private static void FlushConfigChangesToDisk()
        {
            // AssetDatabase.SaveAssetIfDirty was added in Unity 2020.3.16
#if UNITY_2020_3_OR_NEWER && ! (UNITY_2020_3_0 || UNITY_2020_3_1 || UNITY_2020_3_2 || UNITY_2020_3_3 || UNITY_2020_3_4 || UNITY_2020_3_5 || UNITY_2020_3_6 || UNITY_2020_3_7 || UNITY_2020_3_8 || UNITY_2020_3_9 || UNITY_2020_3_10 || UNITY_2020_3_11 || UNITY_2020_3_12 || UNITY_2020_3_13 || UNITY_2020_3_14 || UNITY_2020_3_15)
            AssetDatabase.SaveAssetIfDirty(GenerationDatabase<MonoBehaviour>.Instance);
            AssetDatabase.SaveAssetIfDirty(GenerationDatabase<GenericScriptableObject>.Instance);
            AssetDatabase.SaveAssetIfDirty(GenericTypesDatabase<MonoBehaviour>.Instance);
            AssetDatabase.SaveAssetIfDirty(GenericTypesDatabase<GenericScriptableObject>.Instance);
#else
            AssetDatabase.SaveAssets();
#endif
        }
    }
}