namespace GenericUnityObjects.Editor.GeneratedTypesDatabase
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    internal abstract partial class GenerationDatabase<TDatabase>
    {
        [Serializable]
        private class BehaviourCollection
        {
            [SerializeField] private GenericTypeInfo[] _array;

            public BehaviourCollection(List<GenericTypeInfo> collection) => _array = collection.ToArray();

            public BehaviourCollection(GenericTypeInfo[] collection) => _array = collection;

            public BehaviourCollection() : this((GenericTypeInfo[]) null) { }

            public static implicit operator BehaviourCollection(List<GenericTypeInfo> typeInfoArray) =>
                new BehaviourCollection(typeInfoArray);

            public static implicit operator GenericTypeInfo[] (BehaviourCollection typeCollection) =>
                typeCollection._array;
        }
    }
}