namespace GenericUnityObjects.Editor.MonoBehaviour
{
    using System;
    using System.IO;
    using System.Text;
    using UnityEditor;

    internal static class AssemblyGeneration
    {
        private static GUID GetGUID()
        {
            GUID newGUID;

            do
            {
                newGUID = GUID.Generate();
            }
            while ( ! string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(newGUID.ToString())));

            return newGUID;
        }

        public static void WithDisabledAssetDatabase(Action doAction)
        {
            try
            {
                AssetDatabase.DisallowAutoRefresh();
                AssetDatabase.StartAssetEditing();

                doAction();
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.AllowAutoRefresh();
            }
        }

        public static string ImportAssemblyAsset(string assemblyPath)
        {
            string assemblyGUID = GetGUID().ToString();
            string metaContent = GetMetaFileContent(assemblyGUID);

            using (var fs = new FileStream($"{assemblyPath}.meta", FileMode.Create))
            {
                using (TextWriter tw = new StreamWriter(fs, Encoding.UTF8, 1024, true))
                {
                    tw.Write(metaContent);
                }

                fs.SetLength(fs.Position);
            }

            AssetDatabase.ImportAsset(assemblyPath);
            AssetDatabase.ImportAsset($"{assemblyPath}.mdb");

            return assemblyGUID;
        }

        private static string GetMetaFileContent(string guid)
        {
            return $@"fileFormatVersion: 2
guid: {guid}
PluginImporter:
  externalObjects: {{}}
  serializedVersion: 2
  iconMap: {{}}
  executionOrder: {{}}
  defineConstraints: []
  isPreloaded: 0
  isOverridable: 0
  isExplicitlyReferenced: 1
  validateReferences: 1
  platformData:
  - first:
      : Any
    second:
      enabled: 0
      settings:
        Exclude Editor: 0
        Exclude Linux64: 0
        Exclude OSXUniversal: 0
        Exclude Win: 0
        Exclude Win64: 0
  - first:
      Any:
    second:
      enabled: 1
      settings: {{}}
  - first:
      Editor: Editor
    second:
      enabled: 1
      settings:
        CPU: AnyCPU
        DefaultValueInitialized: true
        OS: AnyOS
  - first:
      Standalone: Linux64
    second:
      enabled: 1
      settings:
        CPU: AnyCPU
  - first:
      Standalone: OSXUniversal
    second:
      enabled: 1
      settings:
        CPU: AnyCPU
  - first:
      Standalone: Win
    second:
      enabled: 1
      settings:
        CPU: x86
  - first:
      Standalone: Win64
    second:
      enabled: 1
      settings:
        CPU: x86_64
  - first:
      Windows Store Apps: WindowsStoreApps
    second:
      enabled: 0
      settings:
        CPU: AnyCPU
  userData:
  assetBundleName:
  assetBundleVariant:
";
        }
    }
}