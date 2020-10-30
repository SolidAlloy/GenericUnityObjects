namespace GenericScriptableObjects
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CreateGenericAssetMenuAttribute : Attribute
    {
        /// <summary>The default file name used by newly created instances of this type.</summary>
        public string FieldName;

        /// <summary>The display name for this type shown in the Assets/Create menu.</summary>
        public string MenuName;

        /// <summary>The position of the menu item within the Assets/Create menu.</summary>
        public int Order;
    }
}