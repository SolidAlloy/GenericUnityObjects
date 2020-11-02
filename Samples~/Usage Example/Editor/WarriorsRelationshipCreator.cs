namespace GenericScriptableObjects.Usage_Example.Editor
{
    using GenericScriptableObjects.Editor;
    using UnityEditor;

    public class WarriorsRelationshipCreator : GenericSOCreator
    {
        [MenuItem("Assets/Create/Warriors Relationship", priority = 0)]
        public static void CreateAsset()
        {
            CreateAsset(
                typeof(WarriorsRelationship<,>),
                GenericSOSampleHelper.NamespaceName,
                GenericSOSampleHelper.ScriptsPath,
                "Warriors Relationship");
        }
    }
}