namespace GenericUnityObjects.Editor
{
    using System;
    using GeneratedTypesDatabase;
    using GenericUnityObjects.Util;
    using SolidUtilities;
    using SolidUtilities.Editor;
    using UnityEditor;
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
        public static void AddSelectorAssembly(GenericTypeInfo genericTypeInfo)
        {
            Type behaviourType = genericTypeInfo.Type;
            Assert.IsNotNull(behaviourType);

            string assemblyName = GetSelectorAssemblyName(behaviourType);
            genericTypeInfo.AssemblyGUID = AssetDatabaseHelper.GetUniqueGUID();

            string assemblyPath = AssemblyCreator.CreateSelectorAssembly(assemblyName, behaviourType, genericTypeInfo.AssemblyGUID);

            AssemblyGeneration.ImportAssemblyAsset(assemblyPath, genericTypeInfo.AssemblyGUID);
            IconSetter.AddAssemblyForIconChange(genericTypeInfo.GUID, genericTypeInfo.AssemblyGUID, false);
        }

        protected override bool AddNewGenericTypes(GenericTypeInfo[] genericTypes)
        {
            base.AddNewGenericTypes(genericTypes);
            return true;
        }

        protected override void AddNewGenericType(GenericTypeInfo genericTypeInfo)
        {
            AddSelectorAssembly(genericTypeInfo);
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
            IconSetter.AddAssemblyForIconChange(genericType.GUID, genericType.AssemblyGUID, false);
            return true;
        }

        protected override bool AdditionalTypeInfoCheck(GenericTypeInfo oldType, GenericTypeInfo newType)
        {
            var oldBehaviour = (BehaviourInfo) oldType;
            var newBehaviour = (BehaviourInfo) newType;

            bool foundMatching = false;

            if (oldBehaviour.ComponentName != newBehaviour.ComponentName)
            {
                UpdateBehaviourComponentName(oldBehaviour, newBehaviour.ComponentName);
                foundMatching = true;
            }

            if (oldBehaviour.Order != newBehaviour.Order)
            {
                UpdateBehaviourOrder(oldBehaviour, newBehaviour.Order);
                foundMatching = true;
            }

            return foundMatching;
        }

        protected override void UpdateGenericTypeNameAndArgs(GenericTypeInfo genericType, Type newType)
        {
            UpdateGenericTypeName(genericType, newType,
                () => UpdateSelectorAssembly(genericType.AssemblyGUID, newType));
        }

        private static string GetSelectorAssemblyName(Type genericTypeWithoutArgs) => genericTypeWithoutArgs.FullName.MakeClassFriendly();

        private static void UpdateSelectorAssembly(string selectorAssemblyGUID, Type newType)
        {
            string newAssemblyName = GetSelectorAssemblyName(newType);
            var assemblyReplacer = AssemblyAssetOperations.StartAssemblyReplacement(AssetDatabase.GUIDToAssetPath(selectorAssemblyGUID));
            string newAssemblyPath = AssemblyCreator.CreateSelectorAssembly(newAssemblyName, newType, selectorAssemblyGUID);
            assemblyReplacer.FinishReplacement(newAssemblyPath);
        }

        private void UpdateBehaviourComponentName(BehaviourInfo behaviour, string newComponentName)
        {
            DebugUtility.Log($"Behaviour component name updated: {behaviour.ComponentName} => {newComponentName}");

            // Update database before operating on assemblies
            BehavioursGenerationDatabase.UpdateComponentName(behaviour, newComponentName);

            UpdateAssemblies(behaviour);
        }

        private void UpdateBehaviourOrder(BehaviourInfo behaviour, int newOrder)
        {
            DebugUtility.Log($"Behaviour order updated: {behaviour.Order} => {newOrder}");

            // Update database before operating on assemblies
            BehavioursGenerationDatabase.UpdateOrder(behaviour, newOrder);

            UpdateAssemblies(behaviour);
        }

        private void UpdateAssemblies(BehaviourInfo behaviour)
        {
            var concreteClasses = BehavioursGenerationDatabase.GetConcreteClasses(behaviour);

            var type = behaviour.RetrieveType<MonoBehaviour>();

            UpdateSelectorAssembly(behaviour.AssemblyGUID, type);
            _concreteClassChecker.UpdateConcreteClassesAssemblies(behaviour.Type, concreteClasses, behaviour.GUID);
        }
    }
}