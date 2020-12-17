namespace GenericUnityObjects.Editor.MonoBehaviour
{
    using System;
    using SolidUtilities.Editor.Helpers.AssetSearch;
    using UnityEngine;

    [Serializable]
    internal class TypeInfo : IEquatable<TypeInfo>
    {
        public string GUID;
        public string AssemblyName;

        [SerializeField] private string _typeFullName;

        public string TypeFullName => _typeFullName;

        public TypeInfo(string typeFullName, string guid)
        {
            _typeFullName = typeFullName;
            GUID = guid;
        }

        public TypeInfo(Type type)
        {
            _typeFullName = type.FullName;
            GUID = AssetSearcher.GetClassGUID(type);
        }

        public bool Equals(TypeInfo p)
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
            return TypeFullName == p.TypeFullName && GUID == p.GUID;
        }

        public static bool operator ==(TypeInfo lhs, TypeInfo rhs)
        {
            return lhs?.Equals(rhs) ?? ReferenceEquals(rhs, null);
        }

        public static bool operator !=(TypeInfo lhs, TypeInfo rhs)
        {
            return ! (lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as TypeInfo);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + TypeFullName.GetHashCode();
                return hash;
            }
        }
    }

    internal readonly struct GenericTypeInfoPair
    {
        public readonly TypeInfo OldType;
        public readonly TypeInfo NewType;

        public GenericTypeInfoPair(TypeInfo oldType, TypeInfo newType)
        {
            OldType = oldType;
            NewType = newType;
        }
    }
}