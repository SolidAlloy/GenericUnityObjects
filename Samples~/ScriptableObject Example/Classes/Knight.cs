namespace GenericUnityObjects.ScriptableObject_Example
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Classes/Knight")]
    public class Knight : WarriorClass
    {
        public WarriorStats<Knight> Stats;

        public override bool HasMeleeAttack => true;

        public override bool HasRangedAttack => false;

        public override bool HasAOEAttack => false;
    }
}