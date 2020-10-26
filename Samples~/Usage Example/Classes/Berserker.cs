namespace GenericScriptableObjects.Usage_Example.Classes
{
    using SolidUtilities.Attributes;
    using UnityEngine;

    public class Berserker : Class
    {
        [SerializeField] private WarriorStats<Berserker> _stats;

        [field: SerializeField, ReadOnly] public override bool HasMeleeAttack { get; } = true;
        [field: SerializeField, ReadOnly] public override bool HasRangedAttack { get; } = false;
        [field: SerializeField, ReadOnly] public override bool HasAOEAttack { get; } = true;
    }
}