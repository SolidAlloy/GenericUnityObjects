namespace GenericUnityObjects.Editor
{
    using System;
    using GeneratedTypesDatabase;
    using GenericUnityObjects.Util;
    using SolidUtilities.Helpers;
    using UnityEngine;
    using UnityEngine.Assertions;
    using Util;

    /// <summary>
    /// Checks if any generic MonoBehaviour types were changed, removed, or updated, and regenerates DLLs if needed.
    /// Most of the work is done in the parent type. This class contains only methods where a task needs to be done
    /// differently for MonoBehaviour compared to ScriptableObject.
    /// </summary>
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
            BehaviourIconSetter.AddAssemblyForIconChange(genericTypeInfo.AssemblyGUID);
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
            BehaviourIconSetter.AddAssemblyForIconChange(genericType.AssemblyGUID);
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