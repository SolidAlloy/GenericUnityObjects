namespace GenericUnityObjects.Editor
{
    using UnityEditor.SettingsManagement;

    internal static class ProjectSettings
    {
        private const string PackageName = "com.solidalloy.generic-unity-objects";

        private static Settings _instance;

        private static UserSetting<bool> _alwaysCreatable;

        public static bool AlwaysCreatable
        {
            get
            {
                InitializeIfNeeded();
                return _alwaysCreatable.value;
            }

            set => _alwaysCreatable.value = value;
        }

        private static void InitializeIfNeeded()
        {
            if (_instance != null)
                return;

            _instance = new Settings(PackageName);

            _alwaysCreatable = new UserSetting<bool>(_instance, nameof(_alwaysCreatable), false);
        }
    }
}