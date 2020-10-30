namespace GenericScriptableObjects.Usage_Example.Classes
{
    using SolidUtilities.Attributes;
    using UnityEngine;

    [CreateAssetMenu(menuName = "Classes/Knight")]
    public class Knight : Class
    {
        [SerializeField] private WarriorStats<Knight> _stats;

        public override bool HasMeleeAttack => true;
        public override bool HasRangedAttack => false;
        public override bool HasAOEAttack => false;
    }
}