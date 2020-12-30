namespace GenericUnityObjects.Editor.Util
{
    using System.IO;
    using GenericUnityObjects.Util;
    using UnityEditor;
    using UnityEngine;

    internal interface ICanBeInitialized
    {
        void Initialize();
    }

    internal abstract class EditorOnlySingletonSO<T> : ScriptableObject
        where T : EditorOnlySingletonSO<T>, ICanBeInitialized
    {
        private static readonly string AssetPath = Config.EditorResources + '/' + typeof(T).Name + ".asset";

        private static T _instance;

        /// <summary> Retrieves an instance to the asset. </summary>
        public static T Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                var instance = AssetDatabase.LoadAssetAtPath<T>(AssetPath);

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
            Directory.CreateDirectory(Config.EditorResources);
            AssetDatabase.CreateAsset(instance, AssetPath);
            DebugUtil.Log($"The asset of type {typeof(T)} was created.");
            instance.Initialize();
            EditorUtility.SetDirty(instance);
            return instance;
        }
    }
}