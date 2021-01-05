namespace GenericUnityObjects.Editor.Util
{
    using System;
    using UnityEditor;

    internal static class AssetDatabaseHelper
    {
        public static void WithDisabledAssetDatabase(Action doAction)
        {
            try
            {
                AssetDatabase.DisallowAutoRefresh();
                AssetDatabase.StartAssetEditing();

                doAction();
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.AllowAutoRefresh();
            }
        }

        public static void RefreshDatabaseIfNeeded(Func<bool> doStuff)
        {
            if (doStuff())
                AssetDatabase.Refresh();
        }
    }
}