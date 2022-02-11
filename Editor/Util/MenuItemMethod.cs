namespace GenericUnityObjects.Editor.Util
{
    using System;
    using GenericUnityObjects.Util;
    using SolidUtilities;
    using UnityEngine;

    /// <summary>
    /// A struct that holds all variables needed to create a MenuItem method.
    /// </summary>
    [Serializable]
    internal class MenuItemMethod : IEquatable<MenuItemMethod>
    {
        public readonly Type Type;

        [SerializeField] private string _typeName;
        [SerializeField] private string _fileName;
        [SerializeField] private string _menuName;
        [SerializeField] private int _order;

        public string TypeName => _typeName;

        public string FileName => _fileName;

        public string MenuName => _menuName;

        public int Order => _order;

        public MenuItemMethod(string fileName, string menuName, int order, Type genericTypeWithoutArgs)
        {
            Type = genericTypeWithoutArgs;

            _typeName = TypeUtility.GetTypeNameAndAssembly(Type);
            _fileName = fileName ?? $"New {Type.Name}";
            _menuName = menuName ?? TypeUtility.GetNiceNameOfGenericType(Type);
            _order = order;
        }

        public bool Equals(MenuItemMethod p)
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
            return TypeName == p.TypeName
                   && _fileName == p._fileName
                   && _menuName == p._menuName
                   && _order == p._order;
        }

        public static bool operator ==(MenuItemMethod lhs, MenuItemMethod rhs)
        {
            return lhs?.Equals(rhs) ?? ReferenceEquals(rhs, null);
        }

        public static bool operator !=(MenuItemMethod lhs, MenuItemMethod rhs)
        {
            return ! (lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as MenuItemMethod);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + _typeName.GetHashCode();
                hash = hash * 23 + _fileName.GetHashCode();
                hash = hash * 23 + _menuName.GetHashCode();
                hash = hash * 23 + _order.GetHashCode();
                return hash;
            }
        }
    }
}