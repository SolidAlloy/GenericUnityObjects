namespace GenericUnityObjects.Editor
{
    using UnityEditor.SettingsManagement;

    public static class ProjectSettings
    {
        private const string PackageName = "com.solidalloy.generic-unity-objects";

        private static Settings _instance;

        private static UserSetting<bool> _alwaysCreatableScriptableObject;
        private static UserSetting<bool> _alwaysCreatableMonoBehaviour;

        public static bool AlwaysCreatableScriptableObject
        {
            get
            {
                InitializeIfNeeded();
                return _alwaysCreatableScriptableObject.value;
            }

            set => _alwaysCreatableScriptableObject.value = value;
        }

        public static bool AlwaysCreatableMonoBehaviour
        {
            get
            {
                InitializeIfNeeded();
                return _alwaysCreatableMonoBehaviour.value;
            }

            set => _alwaysCreatableMonoBehaviour.value = value;
        }

        private static void InitializeIfNeeded()
        {
            if (_instance != null)
                return;

            _instance = new Settings(PackageName);

            // Previously, there was only one always-creatable parameter and it was used for scriptable objects.
            // For backwards compatibility, it stores the previous name.
            const string alwaysCreatableScriptableObjectKey = "_alwaysCreatable";
            _alwaysCreatableScriptableObject = new UserSetting<bool>(_instance, alwaysCreatableScriptableObjectKey, false);

            _alwaysCreatableMonoBehaviour = new UserSetting<bool>(_instance, nameof(_alwaysCreatableMonoBehaviour), false);
        }
    }
}