namespace GenericUnityObjects.Util
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    [Serializable]
    internal struct SerializableTypeDictionary : ISerializationCallbackReceiver
    {
        private Dictionary<Type[], Type> _dict;

        [SerializeField] private SerializableTypeCollection[] _keys;
        [SerializeField] private SerializableType[] _values;

        public static implicit operator Dictionary<Type[], Type>(SerializableTypeDictionary serializableDictionary) => serializableDictionary._dict;

        public static implicit operator SerializableTypeDictionary(Dictionary<Type[], Type> dictionary) => new SerializableTypeDictionary(dictionary);

        public SerializableTypeDictionary(Dictionary<Type[], Type> dictionary)
        {
            _dict = dictionary;
            _keys = null;
            _values = null;
        }

        public void OnBeforeSerialize()
        {
            if (_dict == null)
                return;

            int dictLength = _dict.Count;

            _keys = new SerializableTypeCollection[dictLength];
            _values = new SerializableType[dictLength];

            int index = 0;

            foreach (var kvp in _dict)
            {
                _keys[index] = kvp.Key;
                _values[index] = kvp.Value;
                index++;
            }
        }

        public void OnAfterDeserialize()
        {
            if (_keys == null)
                return;

            int keysLength = _keys.Length;
            _dict = new Dictionary<Type[], Type>(keysLength, default(TypeArrayComparer));

            for (int i = 0; i < keysLength; i++)
            {
                Type[] key = _keys[i];
                Type value = _values[i];

                if (key.Any(type => type == null) || value == null)
                    continue;

                _dict.Add(key, value);
            }
        }
    }
}