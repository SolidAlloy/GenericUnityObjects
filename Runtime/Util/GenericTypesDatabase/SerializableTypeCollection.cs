namespace GenericUnityObjects.Util
{
    using System;
    using UnityEngine;

    [Serializable]
    internal struct SerializableTypeCollection
    {
        [SerializeField] private SerializableType[] _collection;

        public static implicit operator Type[](SerializableTypeCollection typeCollection)
            => Array.ConvertAll<SerializableType, Type>(typeCollection._collection, type => type);

        public static implicit operator SerializableTypeCollection(Type[] typeCollection)
            => new SerializableTypeCollection(typeCollection);

        public SerializableTypeCollection(Type[] typeCollection)
            => _collection = Array.ConvertAll<Type, SerializableType>(typeCollection, type => type);
    }
}