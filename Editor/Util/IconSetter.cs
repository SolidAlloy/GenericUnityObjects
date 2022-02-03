namespace GenericUnityObjects.Editor.Util
{
    using System;
    using System.Collections.Generic;
    using GenericUnityObjects.Util;
    using SolidUtilities;
    using SolidUtilities.Editor;
    using SolidUtilities.UnityEditorInternals;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// By default, an icon of a MonoScript asset generated from an assembly is different from an icon of a C# script.
    /// This makes generic components look different. This class sets generic MonoBehaviour icons to be the same as the usual ones.
    /// </summary>
    internal class IconSetter : EditorOnlySingletonSO<IconSetter>, ICanBeInitialized, ISerializationCallbackReceiver
    {
        [SerializeField] private Texture2D[] _keys;
        [SerializeField] private Collection<string>[] _values;

        private FastIterationDictionary<Texture2D, List<string>> _dict;

        public static void AddAssemblyForIconChange(string genericScriptGUID, string assemblyGUID, bool isScriptableObject)
        {
            if (string.IsNullOrEmpty(genericScriptGUID))
                return;

            if (IconFinder.TryGetCustomIcon(genericScriptGUID, out Texture2D texture, isScriptableObject))
                Instance.AddAssembly(assemblyGUID, texture);
        }

        private void AddAssembly(string guid, Texture2D texture)
        {
            if ( ! _dict.TryGetValue(texture, out var guids))
            {
                guids = new List<string>();
                _dict.Add(texture, guids);
            }

            guids.Add(guid);

            EditorUtility.SetDirty(Instance);
        }

        public static void SetIcons()
        {
            try
            {
                if (Instance._dict.Count == 0)
                    return;
            }
            catch (ApplicationException)
            {
                // Recompilation doesn't work when it is requested instantly, for some reason
                EditorCoroutineHelper.Delay(CompilationHelper.RecompileOnce, 1f);
                return;
            }

            using (AssetDatabaseHelper.DisabledScope())
            {
                foreach (var kvp in Instance._dict)
                {
                    foreach (string guid in kvp.Value)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);

                        var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

                        // Something unexpected happened. The newly generated assembly was removed while the scripts were reloading.
                        // It shouldn't stop us from setting other assemblies' icons and clearing them up in the end.
                        if (script == null)
                            continue;

#if UNITY_2021_2_OR_NEWER
                        var pluginImporter = AssetImporter.GetAtPath(path) as PluginImporter;

                        if (pluginImporter == null)
                            continue;

                        var type = script.GetClass();

                        if (type == null)
                            continue;

                        pluginImporter.SetIcon(type.FullName, kvp.Key);
                        pluginImporter.SaveAndReimport();
#else
                        EditorGUIUtilityProxy.SetIconForObject(script, kvp.Key);
                        MonoImporterProxy.CopyMonoScriptIconToImporters(script);
#endif
                    }
                }
            }

            ClearIconChangeGUIDs();

            // If some icons are changed and an asset needs to be created, its creation process is triggered but does
            // not finish because of the domain reload associated with the icons change. That's why the event must be
            // skipped so that the asset is created properly after the domain reload.
            PersistentStorage.SkipAfterAssemblyGenerationEvent();
        }

        private static void ClearIconChangeGUIDs()
        {
            Instance._dict.Clear();
            EditorUtility.SetDirty(Instance);
        }

        public void Initialize()
        {
            if (_keys != null)
                throw new InvalidOperationException("The asset is already initialized.");

            _dict = new FastIterationDictionary<Texture2D, List<string>>();
        }

        public void OnBeforeSerialize()
        {
            if (_dict == null)
                return;

            int dictLength = _dict.Count;

            _keys = new Texture2D[dictLength];
            _values = new Collection<string>[dictLength];

            int index = 0;

            foreach (var kvp in _dict)
            {
                _keys[index] = kvp.Key;
                _values[index] = kvp.Value;
                index++;
            }
        }

        public void OnAfterDeserialize()
        {
            if (_keys == null)
                return;

            int keysLength = _keys.Length;
            _dict = new FastIterationDictionary<Texture2D, List<string>>(keysLength);

            for (int i = 0; i < keysLength; i++)
            {
                var key = _keys[i];
                var value = _values[i];

                if (key == null)
                    continue;

                _dict.Add(key, value);
            }
        }
    }
}