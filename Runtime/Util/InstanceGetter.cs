namespace GenericUnityObjects.Util
{
    using System;
    using System.IO;
    using System.Linq;
    using JetBrains.Annotations;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    internal class InstanceGetter<T>
        where T : ScriptableObject, ICanBeInitialized
    {
        private readonly Type _actualType;
        private readonly string _folderPath;
        private readonly string _assetPath;

        private T _instance;

        public InstanceGetter(string folderPath)
        {
            _actualType = GetActualType();
            _folderPath = folderPath;
            _assetPath = $"{_folderPath}/{_actualType.Name}.asset";
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

        private static Type GetActualType()
        {
            Type type = typeof(T);

            if ( ! type.IsGenericType)
                return type;

            Type actualType = GetChildType(type);

            if (actualType == null)
                throw new ArgumentException($"Generic type {type} doesn't have an underlying concrete type. Please create one.");

            return actualType;
        }

        private static Type GetChildType(Type type)
        {
#if UNITY_EDITOR
            return TypeCache.GetTypesDerivedFrom(type)
#else
            return type.Assembly.GetTypes()
#endif
                .FirstOrDefault(childType => childType.BaseType == type && ! childType.IsGenericTypeDefinition);
        }

        private T CreateAsset()
        {
            var instance = (T) ScriptableObject.CreateInstance(_actualType);
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
                instance = (T) AssetDatabase.LoadAssetAtPath(_assetPath, _actualType);
#else
                instance = (T) Resources.Load(_actualType.Name, _actualType);
#endif
            }
            catch (UnityException)
            {
                Debug.LogError($"{nameof(GenericScriptableObject)}.{nameof(GenericScriptableObject.CreateInstance)}()" +
                               " cannot be called in the field initializer. Please initialize GenericScriptableObjects in Awake or Start.");
                throw;
            }

#if UNITY_EDITOR
            if (instance == null && File.Exists(_assetPath))
            {
                throw new ApplicationException($"AssetDatabase failed to load an asset at path {_assetPath} but the file was there.");
            }
#endif

            return instance;
        }
    }
}