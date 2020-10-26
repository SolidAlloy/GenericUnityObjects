namespace GenericScriptableObjects.Usage_Example.Classes
{
    using UnityEngine;

    public abstract class Class : MonoBehaviour
    {
        public abstract bool HasMeleeAttack { get; }
        public abstract bool HasRangedAttack { get; }
        public abstract bool HasAOEAttack { get; }

        public void TakeDamage() { }

        public void Attack() { }
    }
}