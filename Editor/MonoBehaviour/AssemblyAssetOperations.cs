namespace GenericUnityObjects.Editor.MonoBehaviour
{
    using System;
    using System.IO;
    using GenericUnityObjects.Util;
    using UnityEditor;

    internal static class AssemblyAssetOperations
    {
        public static void RemoveAssemblyByGUID(string assemblyGUID)
        {
            string assemblyPath = AssetDatabase.GUIDToAssetPath(assemblyGUID);
            RemoveAssemblyByPath(assemblyPath);
        }

        public static void RemoveAssemblyByPath(string assemblyPath)
        {
            string mdbPath = $"{assemblyPath}.mdb";
            AssetDatabase.DeleteAsset(assemblyPath);
            AssetDatabase.DeleteAsset(mdbPath);
        }

        public static void ReplaceAssemblyByGUID(string assemblyGUID, string newAssemblyName, Action createAssembly)
        {
            string oldAssemblyPath = AssetDatabase.GUIDToAssetPath(assemblyGUID);
            ReplaceAssemblyByPath(oldAssemblyPath, newAssemblyName, createAssembly);
        }

        public static void ReplaceAssemblyByPath(string oldAssemblyPath, string newAssemblyName, Action createAssembly)
        {
            string oldAssemblyPathWithoutExtension = RemoveDLLExtension(oldAssemblyPath);
            File.Delete(oldAssemblyPath);
            File.Delete($"{oldAssemblyPathWithoutExtension}.dll.mdb");

            createAssembly();

            string newAssemblyPathWithoutExtension = $"{Config.AssembliesDirPath}/{newAssemblyName}";
            File.Move($"{oldAssemblyPathWithoutExtension}.dll.meta", $"{newAssemblyPathWithoutExtension}.dll.meta");
            File.Move($"{oldAssemblyPathWithoutExtension}.dll.mdb.meta", $"{newAssemblyPathWithoutExtension}.dll.mdb.meta");
        }

        private static string RemoveDLLExtension(string assemblyPath)
        {
            return assemblyPath.Substring(0, assemblyPath.Length - 4);
        }
    }
}