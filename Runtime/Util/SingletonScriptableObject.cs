﻿namespace GenericUnityObjects.Util
{
    using UnityEngine;
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
        private static readonly InstanceGetter<T> _getter = new InstanceGetter<T>(Config.ResourcesPath);

        /// <summary> Retrieves an instance to the asset. </summary>
        protected static T Instance => _getter.GetInstance();

        protected new void SetDirty()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }
}