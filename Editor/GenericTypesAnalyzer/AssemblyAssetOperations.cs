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
            AssetDatabase.DeleteAsset(assemblyPath);
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
            }

            public void Dispose()
            {
                string newAssemblyPathWithoutExtension = $"{Config.AssembliesDirPath}/{_newAssemblyName}";

                if (_oldAssemblyPath != $"{newAssemblyPathWithoutExtension}.dll")
                    File.Move($"{_oldAssemblyPath}.meta", $"{newAssemblyPathWithoutExtension}.dll.meta");
            }
        }
    }
}