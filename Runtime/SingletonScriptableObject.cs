namespace GenericScriptableObjects
{
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using SolidUtilities.Editor.Helpers;
    using UnityEngine;
    using UnityEngine.Assertions;


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

                const string assetsFolder = "Assets";
                const string resourcesFolder = "Resources";

                string assetPath = assetsFolder + '/' + resourcesFolder + '/' + typeof(T).Name + ".asset";
                T instance = AssetDatabase.LoadAssetAtPath<T>(assetPath);

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