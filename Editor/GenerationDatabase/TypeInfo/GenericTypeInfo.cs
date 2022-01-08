namespace GenericUnityObjects.Editor.GeneratedTypesDatabase
{
    using System;
    using System.Linq;
    using GenericUnityObjects.Util;
    using SolidUtilities;
    using UnityEngine;

#if GENERIC_UNITY_OBJECTS_DEBUG
    using UnityEngine.Assertions;
#endif

    [Serializable]
    internal class GenericTypeInfo : TypeInfo, IEquatable<GenericTypeInfo>
    {
        private static readonly ArrayEqualityComparer<string> _stringArrayComparer = new ArrayEqualityComparer<string>();

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

        protected GenericTypeInfo(Type type, string typeNameAndAssembly, string guid)
            : base(type, typeNameAndAssembly, guid)
        {
            // Assert can be expensive when called many times, and there may be hundreds of GenericTypeInfos in
            // a large project that are constructed after each domain reload.
#if GENERIC_UNITY_OBJECTS_DEBUG
            Assert.IsTrue(type.IsGenericTypeDefinition);
#endif

            _argNames = type.GetGenericArguments().Select(argType => argType.Name).ToArray();
        }

        public static GenericTypeInfo Instantiate<TObject>(Type type)
            where TObject : UnityEngine.Object
        {
            string typeNameAndAssembly = TypeUtility.GetTypeNameAndAssembly(type);
            string guid = GenerationDatabase<TObject>.GetCachedGenericTypeGUID(typeNameAndAssembly);

            if (typeof(TObject) == typeof(MonoBehaviour))
            {
                return new BehaviourInfo(type, typeNameAndAssembly, guid);
            }
            else
            {
                return new GenericTypeInfo(type, typeNameAndAssembly, guid);
            }
        }

        public void UpdateArgNames(string[] newNames) => _argNames = newNames;

        public void UpdateArgNames(Type[] newTypes)
        {
            var newNames = new string[newTypes.Length];

            for (int i = 0; i < newTypes.Length; i++)
            {
                newNames[i] = newTypes[i].Name;
            }

            _argNames = newNames;
        }

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
                hash = hash * 23 + _stringArrayComparer.GetHashCode(_argNames);
                return hash;
            }
        }
    }
}