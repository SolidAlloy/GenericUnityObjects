namespace GenericUnityObjects.Editor.Util
{
    using GenericUnityObjects.Util;
    using UnityEngine;

    internal abstract class EditorOnlySingletonSO<T> : ScriptableObject
        where T : EditorOnlySingletonSO<T>, ICanBeInitialized
    {
        private static readonly InstanceGetter<T> Getter = new InstanceGetter<T>(Config.EditorResourcesPath);

        /// <summary> Retrieves an instance to the asset. </summary>
        public static T Instance => Getter.GetInstance();
    }
}