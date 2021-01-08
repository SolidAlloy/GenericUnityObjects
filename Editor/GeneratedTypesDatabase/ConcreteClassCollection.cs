namespace GenericUnityObjects.Editor.GeneratedTypesDatabase
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    internal class ConcreteClassCollection
    {
        [SerializeField] private ConcreteClass[] _array;

        public ConcreteClassCollection(List<ConcreteClass> concreteClasses)
        {
            _array = concreteClasses.ToArray();
        }

        public ConcreteClassCollection() : this(new List<ConcreteClass>()) { }

        public static implicit operator List<ConcreteClass>(ConcreteClassCollection concreteClassCollection)
        {
            return new List<ConcreteClass>(concreteClassCollection._array);
        }

        public static implicit operator ConcreteClassCollection(List<ConcreteClass> concreteClasses) =>
            new ConcreteClassCollection(concreteClasses);
    }
}