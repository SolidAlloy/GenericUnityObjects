namespace GenericUnityObjects.Editor.MonoBehaviour
{
    using System;
    using SolidUtilities.Editor.Helpers.AssetSearch;
    using UnityEngine;

    [Serializable]
    internal class BehaviourInfo : TypeInfo
    {
        public string AssemblyGUID;

        public BehaviourInfo(string typeFullName, string guid)
            : base(typeFullName, guid) { }

        public BehaviourInfo(Type type)
            : base(type) { }
    }

    [Serializable]
    internal class ArgumentInfo : TypeInfo
    {
        public ArgumentInfo(string typeFullName, string guid)
            : base(typeFullName, guid) { }

        public ArgumentInfo(Type type)
            : base(type) { }
    }

    [Serializable]
    internal abstract class TypeInfo : IEquatable<TypeInfo>
    {
        [SerializeField] private string _typeFullName;
        [SerializeField] private string _guid;

        public string TypeFullName => _typeFullName;

        public string GUID => _guid;

        public TypeInfo(string typeFullName, string guid)
        {
            _typeFullName = typeFullName;
            _guid = guid;
        }

        public TypeInfo(Type type)
        {
            _typeFullName = type.FullName;
            _guid = AssetSearcher.GetClassGUID(type);
        }

        public void UpdateGUID(string newGUID) => _guid = newGUID;

        public void UpdateFullName(string newFullName) => _typeFullName = newFullName;

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
            return TypeFullName == p.TypeFullName && _guid == p._guid;
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
                hash = hash * 23 + _typeFullName.GetHashCode();
                hash = hash * 23 + _guid.GetHashCode();
                return hash;
            }
        }

        public override string ToString() => _typeFullName;
    }

    internal readonly struct BehaviourInfoPair
    {
        public readonly BehaviourInfo OldType;
        public readonly BehaviourInfo NewType;

        public BehaviourInfoPair(BehaviourInfo oldType, BehaviourInfo newType)
        {
            OldType = oldType;
            NewType = newType;
        }
    }
}