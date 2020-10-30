namespace GenericScriptableObjects.Usage_Example.Classes
{
    using SolidUtilities.Attributes;
    using UnityEngine;

    [CreateAssetMenu(menuName = "Classes/Berserker")]
    public class Berserker : Class
    {
        [SerializeField] private WarriorStats<Berserker> _stats;

        public override bool HasMeleeAttack => true;
        public override bool HasRangedAttack => false;
        public override bool HasAOEAttack => true;
    }
}