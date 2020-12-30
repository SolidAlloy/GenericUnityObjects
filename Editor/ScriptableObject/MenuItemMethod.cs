namespace GenericUnityObjects.Editor.ScriptableObject
{
    using System;

    /// <summary>
    /// A struct that holds all variables needed to create a MenuItem method.
    /// </summary>
    [Serializable]
    internal struct MenuItemMethod : IEquatable<MenuItemMethod>
    {
        public readonly Type Type;

        public string TypeName;
        public string FileName;
        public string MenuName;
        public int Order;

        public MenuItemMethod(string typeName, string fileName, string menuName, int order, Type type)
        {
            TypeName = typeName;
            FileName = fileName;
            MenuName = menuName;
            Order = order;
            Type = type;
        }

        public override bool Equals(object obj)
        {
            return obj is MenuItemMethod method && this.Equals(method);
        }

        public bool Equals(MenuItemMethod p)
        {
            return TypeName == p.TypeName && FileName == p.FileName && MenuName == p.MenuName && Order == p.Order;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + TypeName.GetHashCode();
                hash = hash * 23 + FileName.GetHashCode();
                hash = hash * 23 + MenuName.GetHashCode();
                hash = hash * 23 + Order.GetHashCode();
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