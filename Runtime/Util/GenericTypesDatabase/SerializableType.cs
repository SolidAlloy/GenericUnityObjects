namespace GenericUnityObjects.Util
{
    using System;
    using UnityEngine;

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