namespace GenericScriptableObjects.Usage_Example
{
    using System;
    using Classes;

    [Serializable]
    public class WarriorStats<TClass> : GenericScriptableObject
        where TClass : Class
    {
        public TClass Warrior;

        public int Health;
        public int Damage;
    }
}
