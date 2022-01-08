namespace GenericUnityObjects.Editor.GeneratedTypesDatabase
{
    using System;
    using SolidUtilities;
    using UnityEngine;

    [Serializable]
    internal class ConcreteClass : IEquatable<ConcreteClass>
    {
        private static readonly ArrayEqualityComparer<ArgumentInfo> _arrayComparer = new ArrayEqualityComparer<ArgumentInfo>();

        [SerializeField] private ArgumentInfo[] _arguments;
        [SerializeField] private string _assemblyGUID;

        public ArgumentInfo[] Arguments => _arguments;

        public string AssemblyGUID => _assemblyGUID;

        public ConcreteClass(ArgumentInfo[] arguments, string assemblyGUID)
        {
            _arguments = arguments;
            _assemblyGUID = assemblyGUID;
        }

        public bool Equals(ConcreteClass p)
        {
            if (ReferenceEquals(p, null))
            {
                return false;
            }

            if (ReferenceEquals(this, p))
            {
                return true;
            }

            if (this.GetType() != p.GetType())
            {
                return false;
            }

            return _arrayComparer.Equals(_arguments, p._arguments) && _assemblyGUID == p._assemblyGUID;
        }

        public static bool operator ==(ConcreteClass lhs, ConcreteClass rhs)
        {
            return lhs?.Equals(rhs) ?? ReferenceEquals(rhs, null);
        }

        public static bool operator !=(ConcreteClass lhs, ConcreteClass rhs)
        {
            return ! (lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ConcreteClass);
        }

        // Can't make the fields readonly because Unity will not serialize them. Should be careful with not changing them instead.
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + _arrayComparer.GetHashCode(_arguments);
                hash = hash * 23 + _assemblyGUID.GetHashCode();
                return hash;
            }
        }
    }
}