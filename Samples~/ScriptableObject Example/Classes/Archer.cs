namespace GenericUnityObjects.ScriptableObject_Example
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Classes/Archer")]
    public class Archer : WarriorClass
    {
        public WarriorStats<Archer> Stats;

        public override bool HasMeleeAttack => false;

        public override bool HasRangedAttack => true;

        public override bool HasAOEAttack => false;
    }

}