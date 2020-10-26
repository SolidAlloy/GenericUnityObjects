namespace GenericScriptableObjects.Usage_Example.Classes
{
    using SolidUtilities.Attributes;
    using UnityEngine;

    public class Knight : Class
    {
        [SerializeField] private WarriorStats<Knight> _stats;

        [field: SerializeField, ReadOnly] public override bool HasMeleeAttack { get; } = true;
        [field: SerializeField, ReadOnly] public override bool HasRangedAttack { get; } = false;
        [field: SerializeField, ReadOnly] public override bool HasAOEAttack { get; } = false;
    }
}