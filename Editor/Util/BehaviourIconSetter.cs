namespace GenericUnityObjects.Editor.Util
{
    using System.Collections.Generic;
    using GenericUnityObjects.Util;
    using SolidUtilities.UnityEditorInternals;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;

    internal class BehaviourIconSetter : EditorOnlySingletonSO<BehaviourIconSetter>, ICanBeInitialized
    {
        [SerializeField] private Texture2D _behaviourIconLight;
        [SerializeField] private Texture2D _behaviourIconDark;

        private static Texture2D BehaviourIcon => EditorGUIUtility.isProSkin
            ? Instance._behaviourIconDark
            : Instance._behaviourIconLight;

        [SerializeField] private List<string> _behaviourIconGUIDs = new List<string>();

        private void OnEnable()
        {
            _behaviourIconLight ??= (Texture2D) EditorGUIUtility.IconContent("cs Script Icon").image;
            _behaviourIconDark ??= (Texture2D) EditorGUIUtility.IconContent("d_cs Script Icon").image;
        }

        public static void AddAssemblyForIconChange(string guid)
        {
            Instance._behaviourIconGUIDs.Add(guid);
            EditorUtility.SetDirty(Instance);
        }

        [DidReloadScripts((int)DidReloadScriptsOrder.BeforeAssemblyGeneration)]
        private static void SetIcons()
        {
            using (new DisabledAssetDatabase(null))
            {
                foreach (string iconChangeGUID in Instance._behaviourIconGUIDs)
                {
                    string path = AssetDatabase.GUIDToAssetPath(iconChangeGUID);
                    var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                    EditorGUIUtilityProxy.SetIconForObject(script, BehaviourIcon);
                    MonoImporterProxy.CopyMonoScriptIconToImporters(script);
                }
            }

            if (Instance._behaviourIconGUIDs.Count != 0 || Instance._behaviourIconGUIDs.Count != 0)
                ClearIconChangeGUIDs();
        }

        private static void ClearIconChangeGUIDs()
        {
            Instance._behaviourIconGUIDs.Clear();
            EditorUtility.SetDirty(Instance);
        }

        public void Initialize() { }
    }
}