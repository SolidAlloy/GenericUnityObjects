namespace GenericUnityObjects.Editor.Util
{
    using System;
    using GenericUnityObjects.Util;
    using SolidUtilities.SerializableCollections;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEditor.Events;
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// A class used to hold serialized values that need to survive assemblies reload. It is mainly used for asset
    /// creation, but also for the MenuItem methods creation and performing actions on next domain reload.
    /// </summary>
    internal class PersistentStorage : EditorOnlySingletonSO<PersistentStorage>, ICanBeInitialized
    {
        private const string AssembliesCountKey = "AssembliesCount";
        private const string FirstCompilationKey = "FirstCompilation";

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

        public static bool FirstCompilation => PlayerPrefs.GetInt(FirstCompilationKey, 1) == 1;

        public static void DisableFirstCompilation()
        {
            PlayerPrefs.SetInt(FirstCompilationKey, 0);
            PlayerPrefs.Save();
        }

        public void Initialize() { }

        #region InvokeEventsOnScriptsReload

        [SerializeField] private UnityEvent _afterReloadEvent = new UnityEvent();

        public static void ExecuteOnScriptsReload(UnityAction action)
        {
            UnityEventTools.AddVoidPersistentListener(Instance._afterReloadEvent, action);
            int lastListener = Instance._afterReloadEvent.GetPersistentEventCount() - 1;
            Instance._afterReloadEvent.SetPersistentListenerState(lastListener, UnityEventCallState.EditorAndRuntime);
            EditorUtility.SetDirty(Instance);
        }

        private static bool _skipEvent;

        public static void SkipAfterAssemblyGenerationEvent() => _skipEvent = true;

        [DidReloadScripts((int)DidReloadScriptsOrder.AfterAssemblyGeneration)]
        private static void OnScriptsReload()
        {
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
                if (Instance._afterReloadEvent.GetPersistentEventCount() != 0)
                    Instance._afterReloadEvent = new UnityEvent();
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