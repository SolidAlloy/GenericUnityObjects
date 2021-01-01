namespace GenericUnityObjects.Editor.MonoBehaviour
{
    using System;
    using SolidUtilities.Helpers;
    using UnityEngine;

    [Serializable]
    internal class ConcreteClass : IEquatable<ConcreteClass>
    {
        private static readonly ArrayEqualityComparer<ArgumentInfo> ArrayComparer = new ArrayEqualityComparer<ArgumentInfo>();

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
            // If parameter is null, return false.
            if (ReferenceEquals(p, null))
            {
                return false;
            }

            // Optimization for a common success case.
            if (ReferenceEquals(this, p))
            {
                return true;
            }

            // If run-time types are not exactly the same, return false.
            if (this.GetType() != p.GetType())
            {
                return false;
            }

            // Return true if the fields match.
            // Note that the base class is not invoked because it is
            // System.Object, which defines Equals as reference equality.
            return ArrayComparer.Equals(_arguments, p._arguments) && _assemblyGUID == p._assemblyGUID;
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
            return this.Equals(obj as ConcreteClass);
        }

        // Can't make the fields readonly because Unity will not serialize them. Should be careful with not changing them instead.
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + ArrayComparer.GetHashCode(_arguments);
                hash = hash * 23 + _assemblyGUID.GetHashCode();
                return hash;
            }
        }
    }
}