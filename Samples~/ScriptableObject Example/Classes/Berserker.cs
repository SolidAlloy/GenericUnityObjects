namespace GenericUnityObjects.ScriptableObject_Example
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Classes/Berserker")]
    public class Berserker : WarriorClass
    {
        public WarriorStats<Berserker> Stats;

        public override bool HasMeleeAttack => true;

        public override bool HasRangedAttack => false;

        public override bool HasAOEAttack => true;
    }
}