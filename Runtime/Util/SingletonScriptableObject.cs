namespace GenericScriptableObjects.Util
{
    using UnityEngine;
    using UnityEngine.Assertions;
#if UNITY_EDITOR
    using SolidUtilities.Editor.Helpers;
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
        private static T _instance = null;

        public static T Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

#if UNITY_EDITOR
                const string assetsFolder = "Assets";
                const string resourcesFolder = "Resources";
                string assetPath = assetsFolder + '/' + resourcesFolder + '/' + typeof(T).Name + ".asset";
                T instance = AssetDatabase.LoadAssetAtPath<T>(assetPath);
#else
                T instance = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();
#endif

                if (instance != null)
                {
                    _instance = instance;
                    return _instance;
                }

                var allInstances = FindObjectsOfType<T>(true);
                _instance = allInstances.Length == 0 ? CreateInstance<T>() : allInstances[0];
                Assert.IsNotNull(_instance);

#if UNITY_EDITOR
                AssetDatabaseHelper.MakeSureFolderExists(resourcesFolder);

                AssetDatabase.CreateAsset(_instance, assetPath);
                EditorUtility.SetDirty(_instance);
#else
                Debug.Log($"The asset of type {typeof(T)} was not created. Please go to editor and create it.");
#endif

                return _instance;
            }
        }
    }
}