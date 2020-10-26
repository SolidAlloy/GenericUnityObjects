namespace GenericScriptableObjects.Usage_Example.Editor
{
    using SolidUtilities.Editor.Helpers;
    using UnityEditor.PackageManager;
    using UnityEngine.Assertions;

    internal static class GenericSOSampleHelper
    {
        public const string NamespaceName = "GenericScriptableObjects.Usage_Example.ConcreteImplementations";

        private const string PackageName = "com.solidalloy.generic-scriptable-objects";

        public static string ScriptsPath
        {
            get
            {
                PackageInfo packageInfo = PackageSearcher.FindPackageByName(PackageName);
                Assert.IsNotNull(packageInfo);
                return $"Samples/GenericScriptableObjects/{packageInfo.version}/Usage Example/ConcreteImplementations/";
            }
        }
    }
}