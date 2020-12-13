namespace GenericScriptableObjects.Util
{
    using JetBrains.Annotations;
    using UnityEngine;
    using UnityEngine.Assertions;
#if UNITY_EDITOR
    using System.IO;
    using UnityEditor;
#else
    using System.Linq;
#endif

    /// <summary>
    /// Abstract class for making reload-proof singletons out of ScriptableObjects
    /// Returns the asset created in the Assets/Resources folder, or null if there is none.
    /// </summary>
    /// <typeparam name="T">Singleton type.</typeparam>
    public abstract class SingletonScriptableObject<T> : ScriptableObject
        where T : ScriptableObject
    {
        private static readonly string AssetPath = Config.SettingsPath + '/' + typeof(T).Name + ".asset";

        private static T _instance = null;

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

#if UNITY_2020_1_OR_NEWER
                // FindObjectsOfType(true) returns less objects than FindObjectsOfTypeAll, thus speeding up the search
                // of the necessary object.
                var allInstances = FindObjectsOfType<T>(true);
#else
                var allInstances = Resources.FindObjectsOfTypeAll<T>();
#endif

                _instance = allInstances.Length == 0 ? CreateInstance<T>() : allInstances[0];
                Assert.IsNotNull(_instance);

#if UNITY_EDITOR
                Directory.CreateDirectory(Config.SettingsPath);

                AssetDatabase.CreateAsset(_instance, AssetPath);
                EditorUtility.SetDirty(_instance);
#else
                Debug.Log($"The asset of type {typeof(T)} was not created. Please go to editor and create it.");
#endif
                return _instance;
            }
        }

        public static T OnlyCreatedInstance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                _instance = GetInstanceFromAsset();
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
                               "initializer. Please initialize Generic ScriptableObjects in Awake or Start.");
                throw;
            }

            return instance;
        }
    }
}