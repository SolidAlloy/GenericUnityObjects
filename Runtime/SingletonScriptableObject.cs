namespace GenericScriptableObjects
{
    using UnityEditor;
    using UnityEngine;
#if UNITY_EDITOR
#endif

    /// <summary>
    /// Abstract class for making reload-proof singletons out of ScriptableObjects
    /// Returns the asset created on the editor, or null if there is none.
    /// </summary>
    /// <typeparam name="T">Singleton type.</typeparam>

    public abstract class SingletonScriptableObject<T> : ScriptableObject
        where T : ScriptableObject
    {
        private static T _instance = null;

        public static T Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                var assets = Resources.LoadAll<T>(string.Empty);

                if (assets.Length > 1)
                    Debug.Log($"Multiple assets of type {typeof(T)} found. Please leave only one.");

                if (assets.Length == 0)
                {
                    const string assetsFolder = "Assets";
                    const string resourcesFolder = "Resources";
                    _instance = CreateInstance<T>();
#if UNITY_EDITOR
                    if (!AssetDatabase.IsValidFolder(resourcesFolder))
                        AssetDatabase.CreateFolder(assetsFolder, resourcesFolder);

                    AssetDatabase.CreateAsset(_instance, assetsFolder + '/' + resourcesFolder + '/' + typeof(T).Name + ".asset");
#else
                    Debug.Log($"The asset of type {typeof(T)} was not created. Please go to editor and create it.");
#endif
                }
                else
                {
                    _instance = assets[0];
                }

                return _instance;
            }
        }
    }
}