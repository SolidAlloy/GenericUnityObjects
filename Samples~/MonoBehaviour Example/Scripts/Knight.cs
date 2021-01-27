namespace GenericUnityObjects.MonoBehaviour_Example
{
    using UnityEngine;

    public class Knight : Warrior
    {
        [SerializeField] private int _level;
        [SerializeField] private int _defense;

        public override int MaxHealth => _level * 200;

        public override void Attack(Warrior enemyWarrior)
        {
            if (Vector2.Distance(this.transform.position, enemyWarrior.transform.position) <= 1f)
                enemyWarrior.CurrentHealth -= _level * 10;
        }
    }
}