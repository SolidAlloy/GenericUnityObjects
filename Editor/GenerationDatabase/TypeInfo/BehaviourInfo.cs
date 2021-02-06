namespace GenericUnityObjects.Editor.GeneratedTypesDatabase
{
    using System;
    using System.Reflection;
    using UnityEngine;

    [Serializable]
    internal class BehaviourInfo : GenericTypeInfo, IEquatable<BehaviourInfo>
    {
        [SerializeField] private string _componentName;
        [SerializeField] private int _order;

        public string ComponentName => _componentName;

        public int Order => _order;

        public BehaviourInfo(string typeNameAndAssembly, string guid, string[] argNames) : base(typeNameAndAssembly, guid, argNames)
        {
            throw new AccessViolationException(
                $"Cannot instantiate {nameof(BehaviourInfo)} directly. Use {nameof(GenericTypeInfo)}.{nameof(Instantiate)} instead.");
        }

        public BehaviourInfo(string typeFullName, string assemblyName, string guid, string[] argNames) : base(typeFullName, assemblyName, guid, argNames)
        {
            throw new AccessViolationException(
                $"Cannot instantiate {nameof(BehaviourInfo)} directly. Use {nameof(GenericTypeInfo)}.{nameof(Instantiate)} instead.");
        }

        public BehaviourInfo(Type type, string typeNameAndAssembly, string guid) : base(type, typeNameAndAssembly, guid)
        {
            var addComponentAttr = type.GetCustomAttribute<AddComponentMenu>();

            _componentName = addComponentAttr?.componentMenu ?? string.Empty;
            _order = addComponentAttr?.componentOrder ?? 0;
        }

        public void UpdateComponentName(string newName) => _componentName = newName;

        public void UpdateOrder(int newOrder) => _order = newOrder;

        public bool Equals(BehaviourInfo p)
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
            return base.Equals(p) && _componentName.Equals(p._componentName) && _order.Equals(p._order);
        }

        public static bool operator ==(BehaviourInfo lhs, BehaviourInfo rhs)
        {
            return lhs?.Equals(rhs) ?? ReferenceEquals(rhs, null);
        }

        public static bool operator !=(BehaviourInfo lhs, BehaviourInfo rhs)
        {
            return ! (lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as BehaviourInfo);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = base.GetHashCode();
                hash = hash * 23 + _componentName?.GetHashCode() ?? 0;
                hash = hash * 23 + _order.GetHashCode();
                return hash;
            }
        }
    }
}