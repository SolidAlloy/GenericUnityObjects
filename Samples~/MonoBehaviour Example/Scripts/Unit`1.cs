namespace GenericUnityObjects.MonoBehaviour_Example
{
    using System.Linq;
    using UnityEngine;

    public class Unit<TWarrior> : MonoBehaviour
        where TWarrior : Warrior
    {
        public TWarrior[] Warriors;

        public int TotalCurrentHealth => Warriors.Sum(warrior => warrior.CurrentHealth);

        public int TotalMaxHealth => Warriors.Sum(warrior => warrior.MaxHealth);

        public void Move(Vector2 position)
        {
            foreach (TWarrior warrior in Warriors)
                warrior.Move(position);
        }
    }
}
