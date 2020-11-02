namespace GenericScriptableObjects.Usage_Example.Editor
{
    using GenericScriptableObjects.Editor;
    using UnityEditor;

    public class WarriorStatsCreator : GenericSOCreator
    {
        [MenuItem("Assets/Create/Warrior Stats", priority = 0)]
        public static void CreateAsset()
        {
            GenericSOCreator.CreateAsset(
                typeof(WarriorStats<>),
                GenericSOSampleHelper.NamespaceName,
                GenericSOSampleHelper.ScriptsPath,
                "Warrior Stats");
        }
    }
}