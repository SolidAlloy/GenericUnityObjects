namespace GenericUnityObjects.Editor
{
    using System;
    using System.IO;
    using GenericUnityObjects.Util;
    using JetBrains.Annotations;
    using UnityEditor;

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

        private static string RemoveDLLExtension(string assemblyPath)
        {
            return assemblyPath.Substring(0, assemblyPath.Length - 4);
        }

        public readonly struct AssemblyReplacer : IDisposable
        {
            private readonly string _oldAssemblyPathWithoutExtension;
            private readonly string _newAssemblyName;

            private AssemblyReplacer(string oldAssemblyPath, string newAssemblyName)
            {
                _newAssemblyName = newAssemblyName;
                _oldAssemblyPathWithoutExtension = RemoveDLLExtension(oldAssemblyPath);
                File.Delete(oldAssemblyPath);
                File.Delete($"{_oldAssemblyPathWithoutExtension}.dll.mdb");
            }

            public static AssemblyReplacer UsingGUID(string assemblyGUID, string newAssemblyName)
            {
                string oldAssemblyPath = AssetDatabase.GUIDToAssetPath(assemblyGUID);
                return new AssemblyReplacer(oldAssemblyPath, newAssemblyName);
            }

            public static AssemblyReplacer UsingPath(string oldAssemblyPath, string newAssemblyName)
            {
                return new AssemblyReplacer(oldAssemblyPath, newAssemblyName);
            }

            public void Dispose()
            {
                string newAssemblyPathWithoutExtension = $"{Config.AssembliesDirPath}/{_newAssemblyName}";
                File.Move($"{_oldAssemblyPathWithoutExtension}.dll.meta", $"{newAssemblyPathWithoutExtension}.dll.meta");
                File.Move($"{_oldAssemblyPathWithoutExtension}.dll.mdb.meta", $"{newAssemblyPathWithoutExtension}.dll.mdb.meta");
            }
        }
    }
}