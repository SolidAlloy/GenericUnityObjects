namespace GenericUnityObjects.Util
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    internal class GenericTypesDatabase<TDatabase> : SingletonScriptableObject<TDatabase>, ISerializationCallbackReceiver, ICanBeInitialized
    where TDatabase : GenericTypesDatabase<TDatabase>
    {
        private Dictionary<Type, Dictionary<Type[], Type>> _dict;

        [SerializeField] private SerializableType[] _keys;
        [SerializeField] private SerializableTypeDictionary[] _values;

        public static void Initialize(Dictionary<Type, Dictionary<Type[], Type>> dict)
        {
            Instance.InitializeImpl(dict);
        }

        public void InitializeImpl(Dictionary<Type, Dictionary<Type[], Type>> dict)
        {
            _dict = dict;
            SetDirty();
        }

        public void OnBeforeSerialize()
        {
            if (_dict == null)
                return;

            int dictLength = _dict.Count;

            _keys = new SerializableType[dictLength];
            _values = new SerializableTypeDictionary[dictLength];

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
#if ! UNITY_EDITOR
            if (_keys == null)
                return;

            int keysLength = _keys.Length;
            _dict = new Dictionary<Type, Dictionary<Type[], Type>>(keysLength);

            for (int i = 0; i < keysLength; i++)
            {
                Type key = _keys[i];
                Dictionary<Type[], Type> value = _values[i];

                if (key == null)
                    continue;

                _dict.Add(key, value);
            }
#endif
        }

        public static bool TryGetConcreteType(Type genericType, out Type concreteType)
        {
            return Instance.TryGetConcreteTypeImpl(genericType, out concreteType);
        }

        public bool TryGetConcreteTypeImpl(Type genericType, out Type concreteType)
        {
            Type genericTypeWithoutArgs = genericType.GetGenericTypeDefinition();
            Type[] genericArgs = genericType.GetGenericArguments();

            if (_dict.TryGetValue(genericTypeWithoutArgs, out var concreteClassesDict)
                && concreteClassesDict.TryGetValue(genericArgs, out concreteType))
            {
                return true;
            }

            return TryGetEmptyDerivedType(genericType, out concreteType);
        }

        public bool TryGetEmptyDerivedType(Type genericType, out Type derivedType)
        {
            derivedType = TypeUtility.GetEmptyTypeDerivedFrom(genericType);

            if (derivedType == null)
                return false;

            AddImpl(genericType, derivedType);
            return true;
        }

        public void AddImpl(Type genericType, Type concreteType)
        {
            Type genericTypeWithoutArgs = genericType.GetGenericTypeDefinition();
            Type[] genericArgs = genericType.GetGenericArguments();

            if (_dict.TryGetValue(genericTypeWithoutArgs, out var concreteClassesDict))
            {
                concreteClassesDict.Add(genericArgs, concreteType);
            }
            else
            {
                _dict.Add(genericTypeWithoutArgs, new Dictionary<Type[], Type>(default(TypeArrayComparer)) { { genericArgs, concreteType } });
            }
        }

        public void Initialize() { }
    }

    internal readonly struct TypeArrayComparer : IEqualityComparer<Type[]>
    {
        public bool Equals(Type[] firstArray, Type[] secondArray)
        {
            if (firstArray == null && secondArray == null)
                return true;

            if (firstArray == null || secondArray == null)
                return false;

            return firstArray.SequenceEqual(secondArray);
        }

        public int GetHashCode(Type[] array)
        {
            int hashCode = 0;

            foreach (Type item in array)
                hashCode ^= item.GetHashCode();

            return hashCode;
        }
    }

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

    [Serializable]
    internal struct SerializableType : IEquatable<SerializableType>
    {
        [SerializeField] private string _typeNameAndAssembly;
        private Type _value;
        private bool _triedSettingTypeOnce;

        public Type Value
        {
            get
            {
                if (_value != null || _triedSettingTypeOnce)
                    return _value;

                _value = Type.GetType(_typeNameAndAssembly);
                _triedSettingTypeOnce = true;
                return _value;
            }
        }

        public SerializableType(Type type)
        {
            _value = type;
            _typeNameAndAssembly = TypeUtility.GetTypeNameAndAssembly(type);
            _triedSettingTypeOnce = false;
        }

        public override bool Equals(object obj)
        {
            return obj is SerializableType type && this.Equals(type);
        }

        public bool Equals(SerializableType p)
        {
            return _typeNameAndAssembly == p._typeNameAndAssembly;
        }

        // Serialized field cannot be readonly, so just need to have caution when working with it.
        public override int GetHashCode() => _typeNameAndAssembly.GetHashCode();

        public static bool operator ==(SerializableType lhs, SerializableType rhs) => lhs.Equals(rhs);

        public static bool operator !=(SerializableType lhs, SerializableType rhs) => ! lhs.Equals(rhs);

        public static implicit operator Type(SerializableType typeReference) => typeReference.Value;

        public static implicit operator SerializableType(Type type) => new SerializableType(type);
    }
}