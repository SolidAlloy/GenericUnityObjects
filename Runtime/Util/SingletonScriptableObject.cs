namespace GenericUnityObjects.Util
{
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    /// <summary>
    /// Abstract class for making reload-proof singletons out of ScriptableObjects
    /// </summary>
    /// <typeparam name="T">Singleton type.</typeparam>
    internal abstract class SingletonScriptableObject<T> : SingletonScriptableObject
        where T : SingletonScriptableObject<T>, ICanBeInitialized
    {
        private static readonly InstanceGetter<T> _getter = new InstanceGetter<T>(Config.ResourcesPath);

        /// <summary> Retrieves an instance to the asset. </summary>
        public static T Instance => _getter.GetInstance();

        protected new void SetDirty()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }

    internal abstract class SingletonScriptableObject : ScriptableObject { }

    internal interface ICanBeInitialized
    {
        void Initialize();
    }
}