namespace GenericUnityObjects.Util
{
    using System.IO;
    using JetBrains.Annotations;
    using UnityEngine;
    using UnityEngine.Assertions;

#if UNITY_EDITOR
    using UnityEditor;
#else
    using System.Linq;
#endif

    /// <summary>
    /// Abstract class for making reload-proof singletons out of ScriptableObjects
    /// Returns the asset created in the Assets/Resources folder, or null if there is none.
    /// </summary>
    /// <typeparam name="T">Singleton type.</typeparam>
    internal abstract class SingletonScriptableObject<T> : ScriptableObject
        where T : SingletonScriptableObject<T>, ISerializationCallbackReceiver
    {
        private static readonly string AssetPath = Config.ResourcesPath + '/' + typeof(T).Name + ".asset";

        private static T _instance;

        /// <summary> Retrieves an instance to the asset. </summary>
        public static T Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                T instance = GetInstanceFromAsset();

                if (instance != null)
                {
                    _instance = instance;
                    return _instance;
                }

                _instance = CreateInstance<T>();
                Assert.IsNotNull(_instance);

#if UNITY_EDITOR
                Debug.Log($"The asset of type {typeof(T)} was created.");
                Directory.CreateDirectory(Config.ResourcesPath);
                AssetDatabase.CreateAsset(_instance, AssetPath);
                _instance.OnAfterDeserialize();
                EditorUtility.SetDirty(_instance);
#else
                Debug.Log($"The asset of type {typeof(T)} was not created. Please go to editor and create it.");
#endif
                return _instance;
            }
        }

        [CanBeNull]
        private static T GetInstanceFromAsset()
        {
            T instance;

            try
            {
#if UNITY_EDITOR
                instance = AssetDatabase.LoadAssetAtPath<T>(AssetPath);
#else
                instance = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();
#endif
            }
            catch (UnityException)
            {
                Debug.LogError("GenericScriptableObject.CreateInstance() cannot be called in the field " +
                               "initializer. Please initialize GenericScriptableObjects in Awake or Start.");
                throw;
            }

            return instance;
        }
    }
}