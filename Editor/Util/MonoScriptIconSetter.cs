namespace GenericUnityObjects.Editor.Util
{
    using System.Collections.Generic;
    using GenericUnityObjects.Util;
    using SolidUtilities.UnityEditorInternals;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;

    internal static class MonoScriptIconSetter
    {
        private static readonly Texture2D _monoScriptLight = (Texture2D) EditorGUIUtility.IconContent("cs Script Icon").image;
        private static readonly Texture2D _monoScriptDark = (Texture2D) EditorGUIUtility.IconContent("d_cs Script Icon").image;

        private static readonly Texture2D _scriptableObjectLight = (Texture2D) EditorGUIUtility.IconContent("ScriptableObject Icon").image;
        private static readonly Texture2D _scriptableObjectDark = (Texture2D) EditorGUIUtility.IconContent("d_ScriptableObject Icon").image;

        private static Texture2D MonoScriptIcon => EditorGUIUtility.isProSkin ? _monoScriptDark : _monoScriptLight;

        private static Texture2D ScriptableObjectIcon => EditorGUIUtility.isProSkin ? _scriptableObjectDark : _scriptableObjectLight;

        [DidReloadScripts((int)DidReloadScriptsOrder.BeforeAssemblyGeneration)]
        private static void SetIcons()
        {
            using (new DisabledAssetDatabase(null))
            {
                SetIcon(PersistentStorage.BehaviourIconGUIDs, MonoScriptIcon);
                SetIcon(PersistentStorage.ScriptableObjectIconGUIDs, ScriptableObjectIcon);
            }

            PersistentStorage.ClearIconChangeGUIDs();
        }

        private static void SetIcon(IEnumerable<string> guids, Texture2D icon)
        {
            foreach (string iconChangeGUID in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(iconChangeGUID);
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                EditorGUIUtilityProxy.SetIconForObject(script, icon);
                MonoImporterProxy.CopyMonoScriptIconToImporters(script);
            }
        }
    }
}