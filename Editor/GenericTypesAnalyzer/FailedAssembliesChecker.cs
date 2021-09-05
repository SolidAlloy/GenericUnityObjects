namespace GenericUnityObjects.Editor
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using Util;

    internal static class FailedAssembliesChecker
    {
        private const string PreviouslyFailedAssemblyGuidsKey = "PreviouslyFailedAssemblyGuids";
        private const string NewFailedAssemblyGuidsKey = "NewFailedAssemblyGuids";

        public static readonly List<string> FailedAssemblyPaths = new List<string>();

        public static void ReimportFailedAssemblies()
        {
            if (FailedAssemblyPaths.Count == 0)
            {
                return;
            }

            var previouslyFailedAssemblies = PersistentStorage.GetFromPlayerPrefs<List<string>>(PreviouslyFailedAssemblyGuidsKey);
            var newFailedAssemblyGuids = new List<string>();

            using (new DisabledAssetDatabase(true))
            {
                var failedAssemblyPathsAndGuids = FailedAssemblyPaths.Select(path => (path, AssetDatabase.AssetPathToGUID(path)))
                    .Where(pathAndGuid => !string.IsNullOrEmpty(pathAndGuid.Item2)).ToList();

                foreach ((string path, string guid) in failedAssemblyPathsAndGuids)
                {
                    // Reimport only the assemblies that did not fail previously
                    if (previouslyFailedAssemblies.Contains(guid))
                        continue;

                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    newFailedAssemblyGuids.Add(guid);
                }

                // Remove all the assemblies that failed but were fixed from previously failed ones.
                previouslyFailedAssemblies = previouslyFailedAssemblies.Intersect(failedAssemblyPathsAndGuids.Select(pathAndGuid => pathAndGuid.Item2)).ToList();
            }

            PersistentStorage.SaveToPlayerPrefs(PreviouslyFailedAssemblyGuidsKey, previouslyFailedAssemblies);
            PersistentStorage.SaveToPlayerPrefs(NewFailedAssemblyGuidsKey, newFailedAssemblyGuids);

            if (newFailedAssemblyGuids.Count != 0)
                PersistentStorage.ExecuteOnScriptsReload(ReimportCreatedAssets);

            AssetDatabase.Refresh();
        }

        private static void ReimportCreatedAssets()
        {
            var failedAssemblyGuids = PersistentStorage.GetFromPlayerPrefs<List<string>>(NewFailedAssemblyGuidsKey);

            using (new DisabledAssetDatabase(true))
            {
                var assetPaths = failedAssemblyGuids
                    .SelectMany(assemblyGuid => AssetDatabase.FindAssets($"t:ConcreteClass_{assemblyGuid}"))
                    .Select(AssetDatabase.GUIDToAssetPath);

                foreach (string assetPath in assetPaths)
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }

            AssetDatabase.Refresh();
        }
    }
}