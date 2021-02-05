namespace GenericUnityObjects.Editor.Util
{
    using GenericUnityObjects.Util;

    /// <summary>
    /// Completely identical to <see cref="SingletonScriptableObject{T}"/> except that assets are saved to
    /// an Editor/Resources folder instead of Resources.
    /// </summary>
    /// <typeparam name="T">Type of singleton ScriptableObject to create.</typeparam>
    internal abstract class EditorOnlySingletonSO<T> : SingletonScriptableObject
        where T : EditorOnlySingletonSO<T>, ICanBeInitialized
    {
        private static readonly InstanceGetter<T> _getter = new InstanceGetter<T>(Config.EditorResourcesPath);

        /// <summary> Retrieves an instance to the asset. </summary>
        public static T Instance => _getter.GetInstance();
    }
}