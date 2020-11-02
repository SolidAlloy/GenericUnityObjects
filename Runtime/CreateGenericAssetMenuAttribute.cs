namespace GenericScriptableObjects
{
    using System;
    using JetBrains.Annotations;

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    [BaseTypeRequired(typeof(GenericScriptableObject))]
    public class CreateGenericAssetMenuAttribute : Attribute
    {
        public const string DefaultNamespaceName = "GenericSOTypes";
        public const string DefaultScriptsPath = "Scripts/GenericSOTypes";

        /// <summary>The default file name used by newly created instances of this type.</summary>
        [PublicAPI, NotNull] public string FileName = string.Empty;

        /// <summary>The display name for this type shown in the Assets/Create menu.</summary>
        [PublicAPI, NotNull] public string MenuName = string.Empty;

        /// <summary>The position of the menu item within the Assets/Create menu.</summary>
        [PublicAPI] public int Order = 0;

        /// <summary>
        /// Custom namespace name to set for auto-generated non-generic types.
        /// Default is "GenericScriptableObjectsTypes".
        /// </summary>
        [PublicAPI, NotNull] public string NamespaceName = DefaultNamespaceName;

        /// <summary>
        /// Custom path to a folder where auto-generated non-generic types must be kept.
        /// Default is "Scripts/GenericScriptableObjectTypes".
        /// </summary>
        [PublicAPI, NotNull] public string ScriptsPath = DefaultScriptsPath;
    }
}