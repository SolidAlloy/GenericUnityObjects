namespace GenericUnityObjects.Editor
{
    using System;
    using GeneratedTypesDatabase;
    using GenericUnityObjects.Util;
    using SolidUtilities.Helpers;
    using UnityEngine;
    using UnityEngine.Assertions;
    using Util;

    internal class BehavioursChecker : GenericTypesChecker<MonoBehaviour>
    {
        protected override bool AddNewGenericTypes(GenericTypeInfo[] genericTypes)
        {
            base.AddNewGenericTypes(genericTypes);
            return true;
        }

        protected override void AddNewGenericType(GenericTypeInfo genericTypeInfo)
        {
            Type behaviourType = genericTypeInfo.Type;
            Assert.IsNotNull(behaviourType);

            string assemblyName = GetSelectorAssemblyName(behaviourType);
            genericTypeInfo.AssemblyGUID = AssemblyGeneration.GetUniqueGUID();

            AssemblyCreator.CreateSelectorAssembly(assemblyName, behaviourType, genericTypeInfo.AssemblyGUID);

            string assemblyPath = $"{Config.AssembliesDirPath}/{assemblyName}.dll";
            AssemblyGeneration.ImportAssemblyAsset(assemblyPath, genericTypeInfo.AssemblyGUID);
            PersistentStorage.AddAssemblyForIconChange(genericTypeInfo.AssemblyGUID);
            base.AddNewGenericType(genericTypeInfo);
        }

        protected override bool RemoveGenericType(GenericTypeInfo genericType)
        {
            base.RemoveGenericType(genericType);
            return true;
        }

        protected override bool UpdateGenericTypeArgNames(GenericTypeInfo genericType, string[] newArgNames, Type newType)
        {
            base.UpdateGenericTypeArgNames(genericType, newArgNames, newType);
            UpdateSelectorAssembly(genericType.AssemblyGUID, newType);
            PersistentStorage.AddAssemblyForIconChange(genericType.AssemblyGUID);
            return true;
        }

        protected override void UpdateGenericTypeName(GenericTypeInfo genericType, Type newType)
        {
            UpdateGenericTypeName(genericType, newType,
                () => UpdateSelectorAssembly(genericType.AssemblyGUID, newType));
        }

        private static string GetSelectorAssemblyName(Type genericTypeWithoutArgs) => genericTypeWithoutArgs.FullName.MakeClassFriendly();

        private static void UpdateSelectorAssembly(string selectorAssemblyGUID, Type newType)
        {
            string newAssemblyName = GetSelectorAssemblyName(newType);

            using (AssemblyAssetOperations.AssemblyReplacer.UsingGUID(selectorAssemblyGUID, newAssemblyName))
            {
                AssemblyCreator.CreateSelectorAssembly(newAssemblyName, newType, selectorAssemblyGUID);
            }
        }
    }
}