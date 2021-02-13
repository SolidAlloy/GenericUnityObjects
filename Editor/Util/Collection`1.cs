namespace GenericUnityObjects.Editor.Util
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    internal class Collection<T>
    {
        [SerializeField] private T[] _array;

        public Collection(List<T> collection) => _array = collection.ToArray();

        public Collection(T[] collection) => _array = collection;

        public Collection() : this((T[]) null) { }

        public static implicit operator Collection<T>(List<T> list)
        {
            return new Collection<T>(list);
        }

        public static implicit operator Collection<T>(T[] array)
        {
            return new Collection<T>(array);
        }

        public static implicit operator T[] (Collection<T> collection) =>
            collection._array;

        public static implicit operator List<T>(Collection<T> collection) => new List<T>(collection._array);

        public void ResetToLength(int length)
        {
            _array = new T[length];
        }
    }
}