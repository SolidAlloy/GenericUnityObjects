namespace GenericUnityObjects.MonoBehaviour_Example
{
    using UnityEngine;

    public abstract class Warrior : MonoBehaviour
    {
        private int _currentHealth;

        public abstract int MaxHealth { get; }

        public int CurrentHealth
        {
            get => _currentHealth;
            set => Mathf.Min(value, MaxHealth);
        }

        public void Move(Vector2 position)
        {
            transform.position = position;
        }

        public abstract void Attack(Warrior enemyWarrior);

        protected virtual void OnEnable()
        {
            _currentHealth = MaxHealth;
        }
    }
}