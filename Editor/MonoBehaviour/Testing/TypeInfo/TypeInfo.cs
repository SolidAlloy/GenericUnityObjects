namespace GenericUnityObjects.Editor.MonoBehaviour
{
    using System;
    using SolidUtilities.Editor.Helpers;
    using SolidUtilities.Editor.Helpers.AssetSearch;
    using UnityEngine;

    [Serializable]
    internal abstract class TypeInfo : IEquatable<TypeInfo>
    {
        [SerializeField] private string _typeNameAndAssembly;
        [SerializeField] private string _guid;

        private Type _type;

        public string TypeNameAndAssembly => _typeNameAndAssembly;

        public string GUID => _guid;

        public Type Type => _type;

        public string TypeFullName
        {
            get
            {
                int comaIndex = _typeNameAndAssembly.IndexOf(',');
                return _typeNameAndAssembly.Substring(0, comaIndex);
            }
        }

        protected TypeInfo(string typeNameAndAssembly, string guid)
        {
            _typeNameAndAssembly = typeNameAndAssembly;
            _guid = guid;
        }

        protected TypeInfo(string typeFullName, string assemblyName, string guid)
        {
            _typeNameAndAssembly = GetTypeNameAndAssembly(typeFullName, assemblyName);
            _guid = guid;
        }

        protected TypeInfo(Type type)
        {
            _type = type;
            _typeNameAndAssembly = type.FullName;
            _guid = AssetSearcher.GetClassGUID(type);
        }

        public bool RetrieveType(out Type type, out bool retrievedFromGUID)
        {
            retrievedFromGUID = false;

            if (_type != null)
            {
                type = _type;
                return true;
            }

            _type = Type.GetType(TypeNameAndAssembly);

            if (_type != null)
            {
                type = _type;
                UpdateGUIDIfNeeded();
                return true;
            }

            if (string.IsNullOrEmpty(GUID))
            {
                type = null;
                return false;
            }

            _type = AssetDatabaseHelper.GetTypeFromGUID(GUID);
            type = _type;
            retrievedFromGUID = true;

            return _type != null;
        }

        private void UpdateGUIDIfNeeded()
        {
            string currentGUID = AssetSearcher.GetClassGUID(_type);
            if (GUID == currentGUID)
                return;

            if (this is ArgumentInfo)
            {
                UpdateArgumentInDatabase((ArgumentInfo) this, currentGUID);
            }
            else if (this is BehaviourInfo)
            {
                UpdateBehaviourInDatabase((BehaviourInfo) this, currentGUID);
            }
            else
            {
                throw new TypeLoadException($"{nameof(UpdateGUIDIfNeeded)} method doesn't know of this inheritor of {nameof(TypeInfo)} yet: {GetType()}.");
            }
        }

        private static void UpdateArgumentInDatabase(ArgumentInfo argument, string newGUID) =>
            GenericBehavioursDatabase.UpdateArgumentGUID(ref argument, newGUID);

        private static void UpdateBehaviourInDatabase(BehaviourInfo behaviour, string newGUID) =>
            GenericBehavioursDatabase.UpdateBehaviourGUID(ref behaviour, newGUID);

        public void UpdateGUID(string newGUID) => _guid = newGUID;

        public void UpdateNameAndAssembly(string newFullName, string newAssemblyName) =>
            _typeNameAndAssembly = GetTypeNameAndAssembly(newFullName, newAssemblyName);

        public void UpdateNameAndAssembly(Type newType) =>
            _typeNameAndAssembly = GetTypeNameAndAssembly(newType);

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
            return TypeNameAndAssembly == p.TypeNameAndAssembly && _guid == p._guid;
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
                hash = hash * 23 + _typeNameAndAssembly.GetHashCode();
                hash = hash * 23 + _guid.GetHashCode();
                return hash;
            }
        }

        public override string ToString() => _typeNameAndAssembly;

        public static string GetTypeNameAndAssembly(Type type)
        {
            if (type == null)
                return string.Empty;

            if (type.FullName == null)
                throw new ArgumentException($"'{type}' does not have full name.", nameof(type));

            return GetTypeNameAndAssembly(type.FullName, type.Assembly.GetName().Name);
        }

        public static string GetTypeNameAndAssembly(string typeFullName, string assemblyName) =>
            $"{typeFullName}, {assemblyName}";
    }
}