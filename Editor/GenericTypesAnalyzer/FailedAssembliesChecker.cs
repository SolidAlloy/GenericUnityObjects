namespace GenericUnityObjects.Editor
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using Util;

    internal static class FailedAssembliesChecker
    {
        private const string FailedAssemblyGuidsKey = "FailedAssemblyGuids";

        public static readonly List<string> FailedAssemblyPaths = new List<string>();

        public static void ReimportFailedAssemblies()
        {
            if (FailedAssemblyPaths.Count == 0)
            {
                return;
            }

            using (new DisabledAssetDatabase(true))
            {
                var newFailedAssemblyGuids = new List<string>();

                var failedAssemblyPathsAndGuids = FailedAssemblyPaths
                    .Select(path => (path, AssetDatabase.AssetPathToGUID(path)))
                    .Where(pathAndGuid => !string.IsNullOrEmpty(pathAndGuid.Item2)).ToList();

                foreach ((string path, string guid) in failedAssemblyPathsAndGuids)
                {
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    newFailedAssemblyGuids.Add(guid);
                }

                if (newFailedAssemblyGuids.Count != 0)
                {
                    PersistentStorage.SaveData(FailedAssemblyGuidsKey, newFailedAssemblyGuids);
                    PersistentStorage.ExecuteOnScriptsReload(ReimportCreatedAssets);
                }
            }

            AssetDatabase.Refresh();
        }

        private static void ReimportCreatedAssets()
        {
            try
            {
                var failedAssemblyGuids = PersistentStorage.GetData<List<string>>(FailedAssemblyGuidsKey);

                if (failedAssemblyGuids == null)
                    return;

                using (new DisabledAssetDatabase(true))
                {
                    var assetPaths = failedAssemblyGuids
                        .SelectMany(assemblyGuid => AssetDatabase.FindAssets($"t:ConcreteClass_{assemblyGuid}"))
                        .Select(AssetDatabase.GUIDToAssetPath);

                    foreach (string assetPath in assetPaths)
                        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                }
            }
            finally
            {
                PersistentStorage.DeleteData(FailedAssemblyGuidsKey);
            }

            AssetDatabase.Refresh();
        }
    }
}