namespace GenericUnityObjects.MonoBehaviour_Example
{
    using UnityEngine;

    public class Archer : Warrior
    {
        [SerializeField] private int _level;
        [SerializeField] private int _attackDistance;

        public override int MaxHealth => _level * 100;

        public override void Attack(Warrior enemyWarrior)
        {
            if (Vector2.Distance(this.transform.position, enemyWarrior.transform.position) <= _attackDistance)
                enemyWarrior.CurrentHealth -= _level * 5;
        }
    }
}