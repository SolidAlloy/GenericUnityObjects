namespace GenericScriptableObjects.Usage_Example
{
    using System;
    using Classes;

    [Serializable]
    public class WarriorStats<TClass> : GenericScriptableObject
        where TClass : Class
    {
        public int Health;
        public int Damage;

        public TClass[] FindAllWarriorsWithTheseStats()
        {
            return FindObjectsOfType<TClass>();
        }
    }
}
