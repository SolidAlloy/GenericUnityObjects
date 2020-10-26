namespace GenericScriptableObjects.Usage_Example.Editor
{
    using GenericScriptableObjects.Editor;
    using UnityEditor;

    public static class WarriorStatsCreator
    {
        [MenuItem(GenericSOCreator.AssetCreatePath + "Warrior Stats", priority = 0)]
        public static void CreateAsset()
        {
            GenericSOCreator.CreateAsset(
                typeof(WarriorStats<>),
                GenericSOSampleHelper.NamespaceName,
                GenericSOSampleHelper.ScriptsPath);
        }
    }
}