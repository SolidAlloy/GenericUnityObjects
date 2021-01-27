namespace GenericUnityObjects.ScriptableObject_Example
{
    using System;
    using GenericUnityObjects;

    [Serializable]
    [CreateGenericAssetMenu]
    public class WarriorStats<TClass> : GenericScriptableObject
        where TClass : WarriorClass
    {
        public int Health;
        public int Damage;

        public TClass[] FindAllWarriorsWithTheseStats()
        {
            return FindObjectsOfType<TClass>();
        }
    }
}
