namespace GenericUnityObjects.Util
{
    using System.IO;
    using JetBrains.Annotations;
    using UnityEngine;
    using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    internal interface ICanBeInitialized
    {
        void Initialize();
    }

    /// <summary>
    /// Abstract class for making reload-proof singletons out of ScriptableObjects
    /// Returns the asset created in the Assets/Resources folder, or null if there is none.
    /// </summary>
    /// <typeparam name="T">Singleton type.</typeparam>
    internal abstract class SingletonScriptableObject<T> : ScriptableObject
        where T : SingletonScriptableObject<T>, ICanBeInitialized
    {
        private static readonly InstanceGetter<T> Getter = new InstanceGetter<T>(Config.ResourcesPath);

        /// <summary> Retrieves an instance to the asset. </summary>
        protected static T Instance => Getter.GetInstance();

        protected new void SetDirty()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }

    internal class InstanceGetter<T>
            where T : ScriptableObject, ICanBeInitialized
    {
        private readonly string _folderPath;
        private readonly string _assetPath;

        private T _instance;

        public InstanceGetter(string folderPath)
        {
            _folderPath = folderPath;
            _assetPath = $"{_folderPath}/{typeof(T).Name}.asset";
        }

        public T GetInstance()
        {
            if (_instance != null)
                return _instance;

            T instance = GetInstanceFromAsset();

            if (instance != null)
            {
                _instance = instance;
                return _instance;
            }

            _instance = CreateAsset();
            return _instance;
        }

        private T CreateAsset()
        {
            var instance = ScriptableObject.CreateInstance<T>();
#if UNITY_EDITOR
            DebugUtility.Log($"The asset of type {typeof(T)} was created.");
            Directory.CreateDirectory(_folderPath);
            AssetDatabase.CreateAsset(instance, _assetPath);
            instance.Initialize();
            EditorUtility.SetDirty(instance);
#else
            Debug.Log($"The asset of type {typeof(T)} was not created. Please go to editor and create it.");
#endif
            return instance;
        }

        [CanBeNull]
        private T GetInstanceFromAsset()
        {
            T instance;

            try
            {
#if UNITY_EDITOR
                instance = AssetDatabase.LoadAssetAtPath<T>(_assetPath);
#else
                instance = Resources.Load<T>(typeof(T).Name);
#endif
            }
            catch (UnityException)
            {
                Debug.LogError($"{nameof(GenericScriptableObject)}.{nameof(GenericScriptableObject.CreateInstance)}()" +
                               " cannot be called in the field initializer. Please initialize GenericScriptableObjects in Awake or Start.");
                throw;
            }

            return instance;
        }
    }
}