namespace GenericScriptableObjects.Usage_Example
{
    using System;
    using Classes;

    [Serializable]
    [CreateGenericAssetMenu]
    public class WarriorsRelationship<TFirst, TSecond> : GenericScriptableObject
        where TFirst : Class
        where TSecond : Class
    {
        public enum RelationshipType
        {
            Friendly,
            Aggressive,
            Neutral
        }

        public TFirst FirstWarrior;
        public TSecond SecondWarrior;
        public RelationshipType Relationship;
    }
}