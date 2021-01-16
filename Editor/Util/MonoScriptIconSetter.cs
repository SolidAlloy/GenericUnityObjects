namespace GenericUnityObjects.Editor.Util
{
    using System;
    using System.Collections.Generic;
    using GenericUnityObjects.Util;
    using SolidUtilities.UnityEditorInternals;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;
    using Object = UnityEngine.Object;

    internal class MonoScriptIconSetter : EditorOnlySingletonSO<MonoScriptIconSetter>, ICanBeInitialized
    {
        [SerializeField] private Texture2D _monoScriptLight;
        [SerializeField] private Texture2D _monoScriptDark;
        [SerializeField] private Texture2D _scriptableObjectLight;
        [SerializeField] private Texture2D _scriptableObjectDark;

        private static Texture2D MonoScriptIcon => EditorGUIUtility.isProSkin
            ? Instance._monoScriptDark
            : Instance._monoScriptLight;

        private static Texture2D ScriptableObjectIcon => EditorGUIUtility.isProSkin
            ? Instance._scriptableObjectDark
            : Instance._scriptableObjectLight;

        [SerializeField] private List<string> _behaviourIconGUIDs = new List<string>();
        [SerializeField] private List<string> _scriptableObjectIconGUIDs = new List<string>();

        private void OnEnable()
        {
            _monoScriptLight ??= (Texture2D) EditorGUIUtility.IconContent("cs Script Icon").image;
            _monoScriptDark ??= (Texture2D) EditorGUIUtility.IconContent("d_cs Script Icon").image;
            _scriptableObjectLight ??= (Texture2D) EditorGUIUtility.IconContent("ScriptableObject Icon").image;
            _scriptableObjectDark ??= (Texture2D) EditorGUIUtility.IconContent("d_ScriptableObject Icon").image;
        }

        public static void AddAssemblyForIconChange<TObject>(string guid) where TObject : Object
        {
            Type objectType = typeof(TObject);

            if (objectType == typeof(MonoBehaviour))
            {
                Instance._behaviourIconGUIDs.Add(guid);
            }
            else if (objectType == typeof(GenericScriptableObject))
            {
                Instance._scriptableObjectIconGUIDs.Add(guid);
            }
            else
            {
                throw new ArgumentException(
                    $"Expected type parameter {nameof(MonoBehaviour)} or {nameof(GenericScriptableObject)}. Got {objectType} instead.");
            }

            EditorUtility.SetDirty(Instance);
        }

        [DidReloadScripts((int)DidReloadScriptsOrder.BeforeAssemblyGeneration)]
        private static void SetIcons()
        {
            using (new DisabledAssetDatabase(null))
            {
                SetIcon(Instance._behaviourIconGUIDs, MonoScriptIcon);
                SetIcon(Instance._scriptableObjectIconGUIDs, ScriptableObjectIcon);
            }

            if (Instance._behaviourIconGUIDs.Count != 0 || Instance._behaviourIconGUIDs.Count != 0)
                ClearIconChangeGUIDs();
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

        private static void ClearIconChangeGUIDs()
        {
            Instance._behaviourIconGUIDs.Clear();
            Instance._scriptableObjectIconGUIDs.Clear();
            EditorUtility.SetDirty(Instance);
        }

        public void Initialize() { }
    }
}