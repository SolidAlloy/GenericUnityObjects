namespace GenericUnityObjects.Editor.Util
{
    using System;
    using UnityEditor;

    /// <summary>
    /// Completely prevents AssetDatabase from importing assets or refreshing.
    /// </summary>
    internal readonly struct DisabledAssetDatabase : IDisposable
    {
        private readonly bool _disable;

        public DisabledAssetDatabase(bool disable)
        {
            _disable = disable;

            if (_disable)
            {
                AssetDatabase.DisallowAutoRefresh();
                AssetDatabase.StartAssetEditing();
            }
        }

        public void Dispose()
        {
            if (_disable)
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.AllowAutoRefresh();
            }
        }
    }
}