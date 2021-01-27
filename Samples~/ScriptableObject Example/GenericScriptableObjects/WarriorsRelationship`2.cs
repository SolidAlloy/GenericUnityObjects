namespace GenericUnityObjects.ScriptableObject_Example
{
    using System;
    using GenericUnityObjects;

    public enum RelationshipType { Friendly, Aggressive, Neutral }

    [Serializable]
    [CreateGenericAssetMenu]
    public class WarriorsRelationship<TFirst, TSecond> : GenericScriptableObject
        where TFirst : WarriorClass
        where TSecond : WarriorClass
    {
        public TFirst FirstWarrior;
        public TSecond SecondWarrior;
        public RelationshipType Relationship;
    }
}