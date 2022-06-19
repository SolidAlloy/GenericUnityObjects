namespace GenericUnityObjects.Editor
{
    using System;
    using GeneratedTypesDatabase;
    using UnityEngine;

    /// <summary>
    /// Checks if any generic <see cref="ScriptableObject"/> types were changed, removed, or updated, and
    /// regenerates DLLs if needed. Most of the work is done in the parent type. This class contains only methods
    /// where a task needs to be done differently for ScriptableObject compared to MonoBehaviour.
    /// </summary>
    internal class ScriptableObjectsChecker : GenericTypesChecker<ScriptableObject>
    {
        protected override bool AddNewGenericTypes(GenericTypeInfo[] genericTypes)
        {
            base.AddNewGenericTypes(genericTypes);
            return false;
        }

        protected override bool AdditionalTypeInfoCheck(GenericTypeInfo oldType, GenericTypeInfo newType)
        {
            return false;
        }

        protected override void UpdateGenericTypeNameAndArgs(GenericTypeInfo genericType, Type newType)
        {
            // Use default implementation
            UpdateGenericTypeName(genericType, newType, null);
        }
    }
}