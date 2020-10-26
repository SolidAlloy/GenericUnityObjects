namespace GenericScriptableObjects.Usage_Example.Editor
{
    using GenericScriptableObjects.Editor;
    using UnityEditor;

    public static class WarriorsRelationshipCreator
    {
        [MenuItem(GenericSOCreator.AssetCreatePath + "Warriors Relationship", priority = 0)]
        public static void CreateAsset()
        {
            GenericSOCreator.CreateAsset(
                typeof(WarriorsRelationship<,>),
                GenericSOSampleHelper.NamespaceName,
                GenericSOSampleHelper.ScriptsPath);
        }
    }
}