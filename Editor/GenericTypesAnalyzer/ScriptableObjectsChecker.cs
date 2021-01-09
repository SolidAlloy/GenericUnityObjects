namespace GenericUnityObjects.Editor
{
    using System;
    using GeneratedTypesDatabase;
    using SolidUtilities.Extensions;

    internal class ScriptableObjectsChecker : GenericTypesChecker<GenericScriptableObject>
    {
        protected override bool AddNewGenericTypes(GenericTypeInfo[] genericTypes)
        {
            base.AddNewGenericTypes(genericTypes);
            return false;
        }

        protected override void AddNewGenericType(GenericTypeInfo genericTypeInfo)
        {
            if ( ! genericTypeInfo.Type.HasAttribute<CreateGenericAssetMenuAttribute>())
                return;

            base.AddNewGenericType(genericTypeInfo);
        }

        protected override void UpdateGenericTypeName(GenericTypeInfo genericType, Type newType)
        {
            UpdateGenericTypeName(genericType, newType, null);
        }
    }
}