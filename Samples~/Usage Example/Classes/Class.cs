namespace GenericScriptableObjects.Usage_Example.Classes
{
    using JetBrains.Annotations;
    using SolidUtilities.Attributes;
    using UnityEngine;

    public abstract class Class : ScriptableObject
    {
        [SerializeField, ReadOnly] private bool _hasMeleeAttack;
        [SerializeField, ReadOnly] private bool _hasRangedAttack;
        [SerializeField, ReadOnly] private bool _hasAOEAttack;

        [PublicAPI] public abstract bool HasMeleeAttack { get; }
        [PublicAPI] public abstract bool HasRangedAttack { get; }
        [PublicAPI] public abstract bool HasAOEAttack { get; }

        [PublicAPI] public void TakeDamage() { }

        [PublicAPI] public void Attack() { }

        private void Reset()
        {
            _hasMeleeAttack = HasMeleeAttack;
            _hasRangedAttack = HasRangedAttack;
            _hasAOEAttack = HasAOEAttack;
        }
    }
}