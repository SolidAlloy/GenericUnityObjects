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
    internal abstract class SingletonScriptableObject<T> : ScriptableObject
        where T : ScriptableObject
    {
        private static readonly string AssetPath = Config.ResourcesPath + '/' + typeof(T).Name + ".asset";

        private static T _instance = null;

        /// <summary>
        /// Retrieves an instance to the asset. DON'T call in DidReloadScripts and InitializeOnLoad!
        /// Use <see cref="CreatedOnlyInstance"/> instead.
        /// </summary>
        protected static T Instance
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
                Directory.CreateDirectory(Config.ResourcesPath);
                AssetDatabase.CreateAsset(_instance, AssetPath);
                EditorUtility.SetDirty(_instance);
#else
                Debug.Log($"The asset of type {typeof(T)} was not created. Please go to editor and create it.");
#endif
                return _instance;
            }
        }

        /// <summary>
        /// Sometimes, FindObjectsOfType and FindObjectsOfTypeAll return null even though the asset exists (this
        /// happens in a DidReloadScripts method on editor launch, for example). In such a case, if
        /// <see cref="Instance"/> is called, a new asset is created and replaces the old one which is harmful.
        /// So, only <see cref="CreatedOnlyInstance"/> must be used in DidReloadScripts, InitializeOnLoad and similar methods.
        /// </summary>
        protected static T CreatedOnlyInstance
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
                               "initializer. Please initialize GenericScriptableObjects in Awake or Start.");
                throw;
            }

            return instance;
        }
    }
}