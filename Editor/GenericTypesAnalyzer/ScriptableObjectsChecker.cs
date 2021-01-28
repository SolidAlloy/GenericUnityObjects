namespace GenericUnityObjects.Editor
{
    using System;
    using GeneratedTypesDatabase;
    using SolidUtilities.Extensions;

    /// <summary>
    /// Checks if any generic <see cref="GenericScriptableObject"/> types were changed, removed, or updated, and
    /// regenerates DLLs if needed. Most of the work is done in the parent type. This class contains only methods
    /// where a task needs to be done differently for GenericScriptableObject compared to MonoBehaviour.
    /// </summary>
    internal class ScriptableObjectsChecker : GenericTypesChecker<GenericScriptableObject>
    {
        protected override bool AddNewGenericTypes(GenericTypeInfo[] genericTypes)
        {
            base.AddNewGenericTypes(genericTypes);
            return false;
        }

        protected override void UpdateGenericTypeName(GenericTypeInfo genericType, Type newType)
        {
            // Use default implementation
            UpdateGenericTypeName(genericType, newType, null);
        }
    }
}