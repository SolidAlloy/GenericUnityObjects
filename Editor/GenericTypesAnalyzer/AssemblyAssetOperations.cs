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

        public static AssemblyReplacer StartAssemblyReplacement(string oldAssemblyPath)
        {
            return new AssemblyReplacer(oldAssemblyPath);
        }

        public readonly struct AssemblyReplacer
        {
            private readonly string _oldAssemblyPath;

            public AssemblyReplacer(string oldAssemblyPath)
            {
                _oldAssemblyPath = oldAssemblyPath;
                File.Delete(oldAssemblyPath);
            }

            public void FinishReplacement(string newAssemblyPath)
            {
                if (_oldAssemblyPath == newAssemblyPath)
                    return;

                string oldAssemblyMeta = $"{_oldAssemblyPath}.meta";

                if (!File.Exists(oldAssemblyMeta))
                    return;

                File.Move(oldAssemblyMeta, $"{newAssemblyPath}.meta");
            }
        }
    }
}