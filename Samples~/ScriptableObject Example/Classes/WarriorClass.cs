namespace GenericUnityObjects.ScriptableObject_Example
{
    using SolidUtilities.Attributes;
    using UnityEngine;

    public abstract class WarriorClass : ScriptableObject
    {
        [SerializeField, ReadOnly] private bool _hasMeleeAttack;
        [SerializeField, ReadOnly] private bool _hasRangedAttack;
        [SerializeField, ReadOnly] private bool _hasAOEAttack;

        public abstract bool HasMeleeAttack { get; }

        public abstract bool HasRangedAttack { get; }

        public abstract bool HasAOEAttack { get; }

        public void TakeDamage() { }

        public void Attack() { }

        private void Reset()
        {
            _hasMeleeAttack = HasMeleeAttack;
            _hasRangedAttack = HasRangedAttack;
            _hasAOEAttack = HasAOEAttack;
        }
    }
}