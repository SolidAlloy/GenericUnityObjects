namespace GenericUnityObjects
{
    using System;
    using JetBrains.Annotations;
    using UnityEngine;

    /// <summary>
    /// Marks a ScriptableObject-derived type to be automatically listed in the Assets/Create submenu, so that
    /// instances of the type can be easily created and stored in the project as ".asset" files.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    [BaseTypeRequired(typeof(ScriptableObject))]
    public class CreateGenericAssetMenuAttribute : Attribute
    {
        /// <summary>The default file name used by newly created instances of this type.</summary>
        [PublicAPI] public string FileName;

        /// <summary>The display name for this type shown in the Assets/Create menu.</summary>
        [PublicAPI] public string MenuName;

        /// <summary>The position of the menu item within the Assets/Create menu.</summary>
        [PublicAPI] public int Order = 0;
    }
}