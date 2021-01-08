namespace GenericUnityObjects.Editor.Util
{
    using System;
    using UnityEditor;

    internal struct DisabledAssetDatabase : IDisposable
    {
        private bool _disposed;

        public DisabledAssetDatabase(object _)
        {
            AssetDatabase.DisallowAutoRefresh();
            AssetDatabase.StartAssetEditing();
            _disposed = false;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            AssetDatabase.StopAssetEditing();
            AssetDatabase.AllowAutoRefresh();
        }
    }
}