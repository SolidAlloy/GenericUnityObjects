namespace GenericUnityObjects.Editor
{
    using System.IO;
    using GenericUnityObjects.Util;
    using UnityEditor;
    using UnityEngine;

    [InitializeOnLoad]
    public static class PackageUpgrader
    {
        private const string PackageUpgradedKey = "GenericUnityObjects_PackageUpgraded";

        static PackageUpgrader()
        {
            // Run the upgrade process only once per project.
            if (PlayerPrefs.GetInt(PackageUpgradedKey, 0) == 1)
                return;

            Directory.CreateDirectory(Config.AssembliesDirPath);

            foreach (string filePath in Directory.EnumerateFiles(Config.AssembliesDirPath))
            {
                if (filePath.EndsWith(".mdb"))
                {
                    AssetDatabase.DeleteAsset(filePath);
                    continue;
                }

                if (filePath.EndsWith(".meta") || filePath.Contains("MenuItems")) // skip the GeneratedMenuItems assembly
                    continue;

                MoveAssemblyToRequiredPath(filePath);
            }

            if (FailedAssembliesChecker.FailedAssemblyPaths.Count == 0)
                PlayerPrefs.SetInt(PackageUpgradedKey, 1);
        }

        private static void MoveAssemblyToRequiredPath(string filePath)
        {
            var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(filePath);

            if (monoScript == null)
            {
                FailedAssembliesChecker.FailedAssemblyPaths.Add(filePath);
                return;
            }

            var concreteType = monoScript.GetClass();

            if (concreteType == null) // Shouldn't happen but let's be safe
                return;

            string requiredPath = Config.GetAssemblyPathForType(concreteType);

            if (!Directory.Exists(requiredPath))
                Directory.CreateDirectory(requiredPath);

            string assemblyName = Path.GetFileName(filePath);

            File.Move(filePath, $"{requiredPath}/{assemblyName}");
            File.Move($"{filePath}.meta", $"{requiredPath}/{assemblyName}.meta");
        }
    }
}