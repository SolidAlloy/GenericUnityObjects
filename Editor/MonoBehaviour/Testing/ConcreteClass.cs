namespace GenericUnityObjects.Editor.MonoBehaviour
{
    using System;
    using SolidUtilities.Helpers;

    [Serializable]
    internal class ConcreteClass : IEquatable<ConcreteClass>
    {
        public readonly ArgumentInfo[] Arguments;
        public readonly string AssemblyGUID;

        private static readonly ArrayEqualityComparer<ArgumentInfo> ArrayComparer = new ArrayEqualityComparer<ArgumentInfo>();

        public ConcreteClass(ArgumentInfo[] arguments, string assemblyGUID)
        {
            Arguments = arguments;
            AssemblyGUID = assemblyGUID;
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
            return ArrayComparer.Equals(Arguments, p.Arguments) && AssemblyGUID == p.AssemblyGUID;
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

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + ArrayComparer.GetHashCode(Arguments);
                hash = hash * 23 + AssemblyGUID.GetHashCode();
                return hash;
            }
        }
    }
}