namespace GenericUnityObjects.Editor.GeneratedTypesDatabase
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    internal class GenericTypeCollection
    {
        [SerializeField] private GenericTypeInfo[] _array;

        public GenericTypeCollection(List<GenericTypeInfo> collection) => _array = collection.ToArray();

        public GenericTypeCollection(GenericTypeInfo[] collection) => _array = collection;

        public GenericTypeCollection() : this((GenericTypeInfo[]) null) { }

        public static implicit operator GenericTypeCollection(List<GenericTypeInfo> typeInfoArray) =>
            new GenericTypeCollection(typeInfoArray);

        public static implicit operator GenericTypeInfo[] (GenericTypeCollection typeCollection) =>
            typeCollection._array;
    }
}