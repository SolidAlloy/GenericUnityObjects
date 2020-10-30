namespace GenericScriptableObjects.Usage_Example.Classes
{
    using System;
    using SolidUtilities.Attributes;
    using SolidUtilities.Helpers;
    using UnityEditor;
    using UnityEngine;

    [CreateAssetMenu(menuName = "Classes/Archer")]
    public class Archer : Class
    {
        [SerializeField] private WarriorStats<Archer> _stats;

        public override bool HasMeleeAttack => false;
        public override bool HasRangedAttack => true;
        public override bool HasAOEAttack => false;
    }

}