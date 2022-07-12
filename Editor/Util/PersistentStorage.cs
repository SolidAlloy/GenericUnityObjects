namespace GenericUnityObjects.Editor.Util
{
    using System;
    using System.Reflection;
    using ExtEvents;
    using GenericUnityObjects.Util;
    using NUnit.Framework.Internal;
    using SolidUtilities.Collections;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEditor.Compilation;
    using UnityEditor.Events;
    using UnityEngine;
    using UnityEngine.Events;
    using Object = UnityEngine.Object;

    /// <summary>
    /// A class used to hold serialized values that need to survive assemblies reload. It is mainly used for asset
    /// creation, but also for the MenuItem methods creation and performing actions on next domain reload.
    /// </summary>
    internal class PersistentStorage : EditorOnlySingletonSO<PersistentStorage>, ICanBeInitialized
    {
        private const string AssembliesCountKey = "AssembliesCount";
        private const string FirstCompilationKey = "FirstCompilation";
        private const string MenuItemsAssemblyKey = "MenuItemsAssemblyPath";
        private const string DelayActionsOnScriptsReloadKey = "DelayActionsOnScriptsReload";

        [SerializeField] private MenuItemMethod[] _menuItemMethods = { };

        public static MenuItemMethod[] MenuItemMethods
        {
            get => Instance._menuItemMethods;
            set
            {
                Instance._menuItemMethods = value;
                EditorUtility.SetDirty(Instance);
            }
        }

        public static string MenuItemsAssemblyPath
        {
            get => PlayerPrefs.GetString(MenuItemsAssemblyKey);
            set
            {
                PlayerPrefs.SetString(MenuItemsAssemblyKey, value);
                PlayerPrefs.Save();
            }
        }

        public static int AssembliesCount
        {
            get => PlayerPrefs.GetInt(AssembliesCountKey);
            set
            {
                PlayerPrefs.SetInt(AssembliesCountKey, value);
                PlayerPrefs.SetInt(FirstCompilationKey, 1);
                PlayerPrefs.Save();
            }
        }

        public static bool DelayActionsOnScriptsReload
        {
            get => PlayerPrefs.GetInt(DelayActionsOnScriptsReloadKey, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(DelayActionsOnScriptsReloadKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool FirstCompilation => PlayerPrefs.GetInt(FirstCompilationKey, 1) == 1;

        public static void DisableFirstCompilation()
        {
            PlayerPrefs.SetInt(FirstCompilationKey, 0);
            PlayerPrefs.Save();
        }

        public void Initialize() { }

        #region InvokeEventsOnScriptsReload

        [SerializeField] private ExtEvent _afterReloadEvent;

        public static void ExecuteOnScriptsReload(Action action)
        {
            MethodInfo method = action.Method;

            PersistentListener listener = method.IsStatic
                ? PersistentListener.FromStatic(method, UnityEventCallState.EditorAndRuntime)
                : PersistentListener.FromInstance(method, (Object) action.Target, UnityEventCallState.EditorAndRuntime);

            Instance._afterReloadEvent.AddPersistentListener(listener);

            EditorUtility.SetDirty(Instance);
        }

        private static bool _skipEvent;

        public static void SkipAfterAssemblyGenerationEvent() => _skipEvent = true;

        public static void OnScriptsReload()
        {
            // If an exception happened in GenericTypesAnalyzer and the recompilation is requested,
            // no need to run the actions because they might throw errors too.
            // Try running the action after the compilation then.
            if (CompilationHelper.RecompilationRequested)
            {
                return;
            }

            // Skip event is set by IconSetter when some DLL icons are set. Domain reload cannot be forced but it
            // happens on the second frame after the recompilation, before an asset is created, so it cancels the asset
            // creation. When asset is created on next domain reload, everything is OK.
            if (_skipEvent)
            {
                _skipEvent = false;
                return;
            }

            EditorApplication.update += InvokeEventsWhenEditorIsReady;
        }

        private static bool _editorInitialized;
        private static bool _toolbarInitialized;
        private static bool _lastFrameSkipped;

        // We cannot use EditorCoroutines or UniTask here and have to rely on checking bool flags each update because
        // only this way we can force editor to update.
        private static void InvokeEventsWhenEditorIsReady()
        {
            if (_editorInitialized)
            {
                return;
            }

            if (!_toolbarInitialized)
            {
                try
                {
                    var _ = EditorStyles.toolbar;
                    _toolbarInitialized = true;
                }
                catch (NullReferenceException)
                {
                    EditorApplication.QueuePlayerLoopUpdate();
                    SceneView.RepaintAll();
                    return;
                }
            }

            if (!_lastFrameSkipped)
            {
                EditorApplication.QueuePlayerLoopUpdate();
                SceneView.RepaintAll();
                _lastFrameSkipped = true;
                return;
            }

            _editorInitialized = true;
            EditorApplication.update -= InvokeEventsWhenEditorIsReady;

            try
            {
                Instance._afterReloadEvent.Invoke();
            }
            finally
            {
                if (Instance._afterReloadEvent.PersistentListeners != null)
                {
                    for (int i = Instance._afterReloadEvent.PersistentListeners.Count - 1; i >= 0; i--)
                    {
                        Instance._afterReloadEvent.RemovePersistentListenerAt(i);
                    }
                }
            }
        }

        #endregion

        #region Generic Data Save-Load

        [SerializeField]
        private SerializableDictionary<string, string> _savedData = new SerializableDictionary<string, string>();

        public static void SaveData<T>(string key, T data)
        {
            Instance._savedData[key] = JsonUtility.ToJson(new DataWrapper<T>(data));
        }

        public static T GetData<T>(string key)
        {
            if (!Instance._savedData.TryGetValue(key, out string serializedWrapper))
                return default;

            var wrapper = new DataWrapper<T>();
            JsonUtility.FromJsonOverwrite(serializedWrapper, wrapper);
            return wrapper.Data;
        }

        public static void DeleteData(string key) => Instance._savedData.Remove(key);

        public static void SaveToPlayerPrefs<T>(string key, T data)
        {
            PlayerPrefs.SetString(key, JsonUtility.ToJson(new DataWrapper<T>(data)));
            PlayerPrefs.Save();
        }

        public static T GetFromPlayerPrefs<T>(string key)
        {
            if ( ! PlayerPrefs.HasKey(key))
                return default;

            var wrapper = new DataWrapper<T>();
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(key), wrapper);
            return wrapper.Data;
        }

        public static void DeleteFromPlayerPrefs(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }

        [Serializable]
        private class DataWrapper<T>
        {
            public T Data;

            public DataWrapper() { }

            public DataWrapper(T data) => Data = data;
        }

        #endregion
    }
}