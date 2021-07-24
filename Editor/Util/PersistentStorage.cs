namespace GenericUnityObjects.Editor.Util
{
    using System;
    using GenericUnityObjects.Util;
    using TypeReferences;
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
        [SerializeField] private TypeReference _genericSOType;
        [SerializeField] private string _fileName;

        [SerializeField] private GameObject _gameObject;
        [SerializeField] private TypeReference _genericBehaviourType;

        [SerializeField] private int _instanceID;
        [SerializeField] private string _propertyPath;

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

        public static void SaveForScriptsReload(Type genericTypeToCreate, string fileName)
        {
            Instance._genericSOType = genericTypeToCreate;
            Instance._fileName = fileName;
        }

        public static void SaveForScriptsReload(SerializedProperty property)
        {
            Instance._instanceID = property.serializedObject.targetObject.GetInstanceID();
            Instance._propertyPath = property.propertyPath;
        }

        public static void SaveForScriptsReload(GameObject gameObject, Type genericType)
        {
            Instance._gameObject = gameObject;
            Instance._genericBehaviourType = genericType;
        }

        public static (Type genericSOType, string fileName) GetGenericSODetails()
        {
            return (Instance._genericSOType, Instance._fileName);
        }

        public static (GameObject gameObject, Type genericBehaviourType) GetGenericBehaviourDetails()
        {
            return (Instance._gameObject, Instance._genericBehaviourType);
        }

        public static SerializedProperty GetSavedProperty()
        {
            var targetObject = EditorUtility.InstanceIDToObject(Instance._instanceID);

            if (targetObject == null)
                return null;

            var serializedObject = new SerializedObject(targetObject);
            return serializedObject.FindProperty(Instance._propertyPath);
        }

        public static void DisableFirstCompilation()
        {
            Instance._firstCompilation = false;
            EditorUtility.SetDirty(Instance);
        }

        public static void Clear()
        {
            Instance._genericSOType = null;
            Instance._fileName = null;
            Instance._gameObject = null;
            Instance._genericBehaviourType = null;
            Instance._instanceID = 0;
            Instance._propertyPath = null;
        }

        public void Initialize() { }
    }
}