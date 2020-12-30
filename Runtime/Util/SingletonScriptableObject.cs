namespace GenericUnityObjects.Util
{
    using System.IO;
    using JetBrains.Annotations;
    using UnityEngine;
    using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    /// <summary>
    /// Abstract class for making reload-proof singletons out of ScriptableObjects
    /// Returns the asset created in the Assets/Resources folder, or null if there is none.
    /// </summary>
    /// <typeparam name="T">Singleton type.</typeparam>
    internal abstract class SingletonScriptableObject<T> : ScriptableObject
        where T : SingletonScriptableObject<T>
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

                _instance = CreateAsset();
                return _instance;
            }
        }

        private static T CreateAsset()
        {
            var instance = CreateInstance<T>();
#if UNITY_EDITOR
            DebugUtil.Log($"The asset of type {typeof(T)} was created.");
            Directory.CreateDirectory(Config.ResourcesPath);
            AssetDatabase.CreateAsset(instance, AssetPath);
            EditorUtility.SetDirty(instance);
#else
            Debug.Log($"The asset of type {typeof(T)} was not created. Please go to editor and create it.");
#endif
            return instance;
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

        protected new void SetDirty()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }
}