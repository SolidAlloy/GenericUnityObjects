namespace GenericUnityObjects.Editor
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using Util;

    internal static class FailedAssembliesChecker
    {
        private const string FailedAssemblyGuids = "FailedAssemblyGuids";

        public static readonly List<string> FailedAssemblyPaths = new List<string>();

        public static void ReimportFailedAssemblies()
        {
            if (FailedAssemblyPaths.Count == 0)
            {
                return;
            }

            var failedAssemblyGuids = new List<string>();

            using (new DisabledAssetDatabase(true))
            {
                foreach (string assemblyPath in FailedAssemblyPaths)
                {
                    AssetDatabase.ImportAsset(assemblyPath, ImportAssetOptions.ForceUpdate);

                    string assemblyGuid = AssetDatabase.AssetPathToGUID(assemblyPath);

                    if (!string.IsNullOrEmpty(assemblyGuid))
                        failedAssemblyGuids.Add(assemblyGuid);


                }
            }

            if (failedAssemblyGuids.Count != 0)
            {
                PersistentStorage.SaveData(FailedAssemblyGuids, failedAssemblyGuids);
                PersistentStorage.ExecuteOnScriptsReload(ReimportCreatedAssets);
            }

            AssetDatabase.Refresh();
        }

        private static void ReimportCreatedAssets()
        {
            try
            {
                var failedAssemblyGuids = PersistentStorage.GetData<List<string>>(FailedAssemblyGuids);

                using (new DisabledAssetDatabase(true))
                {
                    var assetPaths = failedAssemblyGuids
                        .SelectMany(assemblyGuid => AssetDatabase.FindAssets($"t:ConcreteClass_{assemblyGuid}"))
                        .Select(AssetDatabase.GUIDToAssetPath);

                    foreach (string assetPath in assetPaths)
                    {
                        Debug.Log($"Reimported {assetPath}");
                        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                    }
                }
            }
            finally
            {
                PersistentStorage.DeleteData(FailedAssemblyGuids);
            }

            AssetDatabase.Refresh();
        }
    }
}