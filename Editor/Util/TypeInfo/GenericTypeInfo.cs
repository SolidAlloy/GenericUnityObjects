namespace GenericUnityObjects.Editor.MonoBehaviour
{
    using System;
    using System.Linq;
    using NUnit.Framework;
    using SolidUtilities.Helpers;
    using UnityEngine;

    [Serializable]
    internal class GenericTypeInfo : TypeInfo
    {
        public string AssemblyGUID;

        [SerializeField] private string[] _argNames;

        public string[] ArgNames => _argNames;

        public GenericTypeInfo(string typeNameAndAssembly, string guid, string[] argNames)
            : base(typeNameAndAssembly, guid)
        {
            _argNames = argNames;
        }

        public GenericTypeInfo(string typeFullName, string assemblyName, string guid, string[] argNames)
            : base(typeFullName, assemblyName, guid)
        {
            _argNames = argNames;
        }

        public GenericTypeInfo(Type type, string typeGUID = null)
            : base(type, typeGUID)
        {
            Assert.IsTrue(type.IsGenericTypeDefinition);
            _argNames = type.GetGenericArguments().Select(argType => argType.Name).ToArray();
        }

        public void UpdateArgNames(string[] newNames) => _argNames = newNames;

        public bool Equals(GenericTypeInfo p)
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
            return base.Equals(p) && _argNames.SequenceEqual(p._argNames);
        }

        public static bool operator ==(GenericTypeInfo lhs, GenericTypeInfo rhs)
        {
            return lhs?.Equals(rhs) ?? ReferenceEquals(rhs, null);
        }

        public static bool operator !=(GenericTypeInfo lhs, GenericTypeInfo rhs)
        {
            return ! (lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as GenericTypeInfo);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = base.GetHashCode();
                hash = hash * 23 + new ArrayEqualityComparer<string>().GetHashCode(_argNames);
                return hash;
            }
        }
    }
}