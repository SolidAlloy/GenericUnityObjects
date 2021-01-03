namespace GenericUnityObjects.Editor.ScriptableObject
{
    using System;
    using System.Linq;
    using SolidUtilities.Helpers;
    using UnityEngine;
    using Util;
    using TypeHelper = GenericUnityObjects.Util.TypeHelper;

    /// <summary>
    /// A struct that holds all variables needed to create a MenuItem method.
    /// </summary>
    [Serializable]
    internal struct MenuItemMethod : IEquatable<MenuItemMethod>
    {
        private static readonly ArrayEqualityComparer<string> StringArrayComparer = new ArrayEqualityComparer<string>();

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

            _typeName = TypeHelper.GetTypeNameAndAssembly(Type);
            _fileName = fileName ?? $"New {Type.Name}";
            _menuName = menuName ?? CreatorUtil.GetShortNameWithBrackets(Type);
            _order = order;
        }

        public override bool Equals(object obj)
        {
            return obj is MenuItemMethod method && this.Equals(method);
        }

        public bool Equals(MenuItemMethod p)
        {
            return TypeName == p.TypeName
                   && _fileName == p._fileName
                   && _menuName == p._menuName
                   && _order == p._order;
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

        public static bool operator ==(MenuItemMethod lhs, MenuItemMethod rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(MenuItemMethod lhs, MenuItemMethod rhs)
        {
            return ! lhs.Equals(rhs);
        }
    }
}