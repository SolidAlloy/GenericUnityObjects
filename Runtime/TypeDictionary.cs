namespace GenericScriptableObjects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TypeReferences;
    using UnityEngine;

    [Serializable]
    public class TypeDictionary : ISerializationCallbackReceiver
    {
        private readonly Dictionary<TypeReference[], TypeReference> _dict =
            new Dictionary<TypeReference[], TypeReference>(new TypeReferenceArrayComparer());

        [SerializeField]
        // [HideInInspector] // TODO: hide fields
        private TypeReference[][] _keys;

        [SerializeField]
        // [HideInInspector]
        private TypeReference[] _values;

        public void Add(Type[] key, Type value)
        {
            _dict.Add(key.Cast<TypeReference>().ToArray(), value);
        }

        public bool ContainsKey(Type[] key)
        {
            return _dict.ContainsKey(key.Cast<TypeReference>().ToArray()); // TODO: shorten the cast
        }

        public bool TryGetValue(TypeReference[] key, out TypeReference value) => _dict.TryGetValue(key, out value);

        public bool TryGetValue(Type[] key, out Type value)
        {
            bool result = TryGetValue(key.Cast<TypeReference>().ToArray(), out TypeReference typeRef);
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
            _keys = new TypeReference[dictLength][];
            _values = new TypeReference[dictLength];

            int keysIndex = 0;
            foreach (var pair in _dict)
            {
                _keys[keysIndex] = pair.Key;
                _values[keysIndex] = pair.Value;
                ++keysIndex;
            }
        }
    }
}