namespace GenericScriptableObjects.Editor.MenuItemsGeneration
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A struct that holds all variables needed to create a MenuItem method.
    /// </summary>
    [Serializable]
    public struct MenuItemMethod
    {
        public string TypeName;
        public string FileName;
        public string MenuName;
        public int Order;
        public Type Type;

        private static EqualityComparer _comparer;

        public static EqualityComparer Comparer
        {
            get
            {
                if (_comparer == null)
                    _comparer = new EqualityComparer();

                return _comparer;
            }
        }

        public class EqualityComparer : EqualityComparer<MenuItemMethod>
        {
            public override bool Equals(MenuItemMethod x, MenuItemMethod y)
            {
                return x.TypeName == y.TypeName && x.FileName == y.FileName && x.MenuName == y.MenuName && x.Order == y.Order;
            }

            public override int GetHashCode(MenuItemMethod obj)
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 23 + obj.TypeName.GetHashCode();
                    hash = hash * 23 + obj.FileName.GetHashCode();
                    hash = hash * 23 + obj.MenuName.GetHashCode();
                    hash = hash * 23 + obj.Order.GetHashCode();
                    return hash;
                }
            }
        }
    }
}