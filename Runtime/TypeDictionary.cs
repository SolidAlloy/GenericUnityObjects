namespace GenericScriptableObjects
{
    using System;
    using System.Collections.Generic;
    using TypeReferences;
    using UnityEngine;
    using Util;

    [Serializable]
    internal class TypeDictionary : ISerializationCallbackReceiver
    {
        private readonly Dictionary<TypeReference[], TypeReference> _dict =
            new Dictionary<TypeReference[], TypeReference>(new TypeReferenceArrayComparer());

        [SerializeField] private TypeReferenceCollection[] _keys;

        [SerializeField] private TypeReference[] _values;

        public void Add(Type[] key, Type value) => _dict.Add(key.CastToTypeReference(), value);

        public bool ContainsKey(Type[] key) => _dict.ContainsKey(key.CastToTypeReference());

        public bool TryGetValue(TypeReference[] key, out TypeReference value) => _dict.TryGetValue(key, out value);

        public bool TryGetValue(Type[] key, out Type value)
        {
            bool result = TryGetValue(key.CastToTypeReference(), out TypeReference typeRef);
            value = typeRef;
            return result;
        }

        public void OnAfterDeserialize()
        {
            if (_keys == null || _values == null || _keys.Length != _values.Length)
                return;

            _dict.Clear();
            int keysLength = _keys.Length;

            for (int i = 0; i < keysLength; ++i)
            {
                if (_values[i].Type != null)
                    _dict[_keys[i]] = _values[i];
            }

            _keys = null;
            _values = null;
        }

        public void OnBeforeSerialize()
        {
            int dictLength = _dict.Count;
            _keys = new TypeReferenceCollection[dictLength];
            _values = new TypeReference[dictLength];

            int keysIndex = 0;
            foreach (var pair in _dict)
            {
                _keys[keysIndex] = pair.Key;
                _values[keysIndex] = pair.Value;
                ++keysIndex;
            }
        }

        [Serializable]
        private class TypeReferenceCollection // TODO: check if struct will give more performance
        {
            [SerializeField] private TypeReference[] _array;

            public TypeReferenceCollection(TypeReference[] collection) => _array = collection;

            public TypeReferenceCollection() : this((TypeReference[]) null) { }

            public TypeReferenceCollection(Type[] collection) : this(collection.CastToTypeReference()) { }

            public static implicit operator TypeReferenceCollection(Type[] typeCollection) =>
                new TypeReferenceCollection(typeCollection);

            public static implicit operator TypeReferenceCollection(TypeReference[] typeRefCollection) => new TypeReferenceCollection(typeRefCollection);

            public static implicit operator TypeReference[](TypeReferenceCollection typeRefCollection) =>
                typeRefCollection._array;
        }
    }
}