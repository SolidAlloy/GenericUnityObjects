namespace GenericUnityObjects.Editor
{
    using GenericUnityObjects.Util;
    using SolidUtilities.UnityEditorInternals;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;
    using Util;

    internal static class MonoScriptIconSetter
    {
        [DidReloadScripts((int)DidReloadScriptsOrder.BeforeAssemblyGeneration)]
        private static void SetIcons()
        {
            var monoScriptIcon = (Texture2D) EditorGUIUtility.IconContent("d_cs Script Icon").image;

            foreach (string iconChangeGUID in PersistentStorage.IconChangeGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(iconChangeGUID);
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                EditorGUIUtilityProxy.SetIconForObject(script, monoScriptIcon);
                MonoImporterProxy.CopyMonoScriptIconToImporters(script);
            }

            PersistentStorage.ClearIconChangeGUIDs();
        }
    }
}