namespace GenericUnityObjects.Editor
{
    using System;
    using System.IO;
    using GenericUnityObjects.Util;
    using JetBrains.Annotations;
    using UnityEditor;

    /// <summary>
    /// A class that contains methods related to t
    /// </summary>
    internal static class AssemblyAssetOperations
    {
        public static void RemoveAssemblyByGUID([CanBeNull] string assemblyGUID)
        {
            if (string.IsNullOrEmpty(assemblyGUID))
                return;

            string assemblyPath = AssetDatabase.GUIDToAssetPath(assemblyGUID);
            RemoveAssemblyByPath(assemblyPath);
        }

        public static void RemoveAssemblyByPath(string assemblyPath)
        {
            string mdbPath = $"{assemblyPath}.mdb";
            AssetDatabase.DeleteAsset(assemblyPath);
            AssetDatabase.DeleteAsset(mdbPath);
        }

        public readonly struct AssemblyReplacer : IDisposable
        {
            private readonly string _oldAssemblyPath;
            private readonly string _newAssemblyName;

            public static AssemblyReplacer UsingGUID(string assemblyGUID, string newAssemblyName)
            {
                string oldAssemblyPath = AssetDatabase.GUIDToAssetPath(assemblyGUID);
                return new AssemblyReplacer(oldAssemblyPath, newAssemblyName);
            }

            private AssemblyReplacer(string oldAssemblyPath, string newAssemblyName)
            {
                _newAssemblyName = newAssemblyName;
                _oldAssemblyPath = oldAssemblyPath;
                File.Delete(_oldAssemblyPath);
                File.Delete($"{_oldAssemblyPath}.mdb");
            }

            public void Dispose()
            {
                string newAssemblyPathWithoutExtension = $"{Config.AssembliesDirPath}/{_newAssemblyName}";
                File.Move($"{_oldAssemblyPath}.meta", $"{newAssemblyPathWithoutExtension}.dll.meta");
                File.Move($"{_oldAssemblyPath}.mdb.meta", $"{newAssemblyPathWithoutExtension}.dll.mdb.meta");
            }
        }
    }
}