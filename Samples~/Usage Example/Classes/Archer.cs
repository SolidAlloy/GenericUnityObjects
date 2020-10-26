namespace GenericScriptableObjects.Usage_Example.Classes
{
    using SolidUtilities.Attributes;
    using UnityEngine;

    public class Archer : Class
    {
        [SerializeField] private WarriorStats<Archer> _stats;

        [field: SerializeField, ReadOnly] public override bool HasMeleeAttack { get; } = false;
        [field: SerializeField, ReadOnly] public override bool HasRangedAttack { get; } = true;
        [field: SerializeField, ReadOnly] public override bool HasAOEAttack { get; } = false;
    }
}