namespace GenericUnityObjects.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using GeneratedTypesDatabase;
    using GenericUnityObjects.Util;
    using JetBrains.Annotations;
    using SolidUtilities.Editor;
    using SolidUtilities;
    using UnityEditor;
    using Object = UnityEngine.Object;

    /// <summary>
    /// A class that checks changes to generic types in the project, and updates DLLs and info in the database based on them.
    /// </summary>
    /// <typeparam name="TObject"> A type derived from <see cref="UnityEngine.Object"/>. </typeparam>
    internal abstract class GenericTypesChecker<TObject>
        where TObject : Object
    {
        protected readonly ConcreteClassChecker<TObject> _concreteClassChecker;

        protected GenericTypesChecker() => _concreteClassChecker = new ConcreteClassChecker<TObject>(this);

        public bool Check()
        {
            var oldGenericTypes = GenerationDatabase<TObject>.GenericTypeArguments.Keys;
            var newGenericTypes = TypeCache.GetTypesDerivedFrom<TObject>()
                .Where(type => type.IsGenericType && ! type.IsAbstract)
                .Select(GenericTypeInfo.Instantiate<TObject>)
                .ToArray();

            int oldGenericTypesLength = oldGenericTypes.Count;
            int newGenericTypesLength = newGenericTypes.Length;

            // Optimizations for common cases.
            if (oldGenericTypesLength == 0 && newGenericTypesLength == 0)
            {
                return false;
            }

            if (oldGenericTypesLength == 0)
            {
                return AddNewGenericTypes(newGenericTypes);
            }

            if (newGenericTypesLength == 0)
            {
                return RemoveGenericTypes(oldGenericTypes);
            }

            // Per-type comparison
            return CompareTypes(oldGenericTypes, newGenericTypes);
        }

        private bool CompareTypes(IReadOnlyList<GenericTypeInfo> oldGenericTypes, GenericTypeInfo[] newGenericTypes)
        {
            var oldTypesSet = new HashSet<GenericTypeInfo>(oldGenericTypes);
            var newTypesSet = new HashSet<GenericTypeInfo>(newGenericTypes);

            var oldTypesOnly = oldTypesSet.ExceptWithAndCreateNew(newTypesSet).ToList();
            var newTypesOnly = newTypesSet.ExceptWithAndCreateNew(oldTypesSet).ToArray();

            bool needsAssetDatabaseRefresh = false;

            foreach (GenericTypeInfo newType in newTypesOnly)
            {
                bool foundMatching = false;

                for (int i = 0; i < oldTypesOnly.Count; i++)
                {
                    GenericTypeInfo oldType = oldTypesOnly[i];

                    bool guidsDontMatch = newType.GUID != oldType.GUID;
                    bool typeNamesDontMatch = newType.TypeNameAndAssembly != oldType.TypeNameAndAssembly;

                    // If both parameters don't match, the types are not equal
                    if (guidsDontMatch && typeNamesDontMatch && ! ActualTypesMatch(oldType, newType))
                        continue;

                    if (guidsDontMatch)
                    {
                        UpdateGenericTypeGUID(oldType, newType.GUID);
                        foundMatching = true;
                    }

                    if (typeNamesDontMatch)
                    {
                        needsAssetDatabaseRefresh = true;
                        UpdateGenericTypeNameAndArgs(oldType, newType.Type);
                        foundMatching = true;
                    }
                    else if ( ! newType.ArgNames.SequenceEqual(oldType.ArgNames))
                    {
                        needsAssetDatabaseRefresh |= UpdateGenericTypeArgNames(oldType, newType.ArgNames, newType.Type);
                        foundMatching = true;
                    }

                    if (AdditionalTypeInfoCheck(oldType, newType))
                        foundMatching = true;

                    if (foundMatching)
                    {
                        oldTypesOnly.Remove(oldType);
                        break;
                    }
                }

                // There is no matching old type info, so a completely new assembly must be added for this type
                if ( ! foundMatching)
                {
                    needsAssetDatabaseRefresh = true;
                    AddNewGenericType(newType);
                }
            }

            if (oldTypesOnly.Count == 0)
                return needsAssetDatabaseRefresh;

            return RemoveGenericTypes(oldTypesOnly);
        }

        protected abstract bool AdditionalTypeInfoCheck(GenericTypeInfo oldType, GenericTypeInfo newType);

        private static bool ActualTypesMatch(GenericTypeInfo oldTypeInfo, GenericTypeInfo newTypeInfo)
        {
            // When GUIDs and TypeNames don't match, there is a possibility that the user renamed class name without
            // renaming the file name. In this case, the new GUID is empty, but old GUID still exists, and the type
            // loaded from the old GUID matches the new one. In such case, we can treat these types as equal.
            return newTypeInfo.GUID.Length == 0
                   && oldTypeInfo.GUID.Length != 0
                   && oldTypeInfo.RetrieveType<TObject>() == newTypeInfo.Type;
        }

        private static void UpdateGenericTypeGUID(GenericTypeInfo genericType, string newGUID)
        {
            // new type GUID is empty -> leave the old GUID
            if (string.IsNullOrEmpty(newGUID))
                return;

            DebugUtility.Log($"{typeof(TObject).Name} GUID updated: {genericType.GUID} => {newGUID}");
            GenerationDatabase<TObject>.UpdateGenericType(genericType, genericTypeInfo => genericTypeInfo.UpdateGUID(newGUID));
        }

        public void UpdateReferencedGenericTypes(ArgumentInfo argument, GenericTypeInfo[] referencedGenericTypes)
        {
            foreach (GenericTypeInfo genericTypeInfo in referencedGenericTypes)
            {
                // Retrieve type of the behaviour
                // If type cannot be retrieved, try finding by GUID
                // If GUID didn't help, remove the whole collection of assemblies
                if (genericTypeInfo.RetrieveType<TObject>(out Type genericType, out bool retrievedFromGUID))
                {
                    if (retrievedFromGUID)
                    {
                        UpdateGenericTypeNameAndArgs(genericTypeInfo, genericType);
                    }
                }
                else
                {
                    GenerationDatabase<TObject>.RemoveGenericType(genericTypeInfo, AssemblyAssetOperations.RemoveAssemblyByGUID);
                    continue; // concrete classes are already removed, no need to iterate through them.
                }

                var concreteClasses = GenerationDatabase<TObject>.GetConcreteClassesByArgument(genericTypeInfo, argument);
                _concreteClassChecker.UpdateConcreteClassesAssemblies(genericType, concreteClasses, genericTypeInfo.GUID);
            }
        }

        protected abstract void UpdateGenericTypeNameAndArgs(GenericTypeInfo genericType, Type newType);

        protected virtual bool AddNewGenericTypes(GenericTypeInfo[] genericTypes)
        {
            foreach (GenericTypeInfo genericType in genericTypes)
            {
                AddNewGenericType(genericType);
            }

            return false;
        }

        protected virtual bool UpdateGenericTypeArgNames(GenericTypeInfo genericType, string[] newArgNames, Type newType)
        {
            DebugUtility.Log($"{typeof(TObject).Name} args updated: '{string.Join(", ", genericType.ArgNames)}' => '{string.Join(", ", newArgNames)}'");
            GenerationDatabase<TObject>.UpdateGenericType(genericType, genericTypeInfo => genericTypeInfo.UpdateArgNames(newArgNames));
            return false;
        }

        protected virtual void AddNewGenericType(GenericTypeInfo genericTypeInfo)
        {
            DebugUtility.Log($"{typeof(TObject).Name} added: {genericTypeInfo.TypeFullName}");
            GenerationDatabase<TObject>.AddGenericType(genericTypeInfo);
        }

        protected virtual bool RemoveGenericType(GenericTypeInfo genericType)
        {
            // 'ConcreteClass_ade148c5c4a7ea64bb9a635005ef6220' is missing the class attribute 'ExtensionOfNativeClass'!
            LogHelper.RemoveLogEntriesByMode(LogModes.EditorErrors);

            DebugUtility.Log($"{typeof(TObject).Name} removed: {genericType.TypeFullName}");
            return GenerationDatabase<TObject>.RemoveGenericType(genericType, AssemblyAssetOperations.RemoveAssemblyByGUID);
        }

        protected void UpdateGenericTypeName(GenericTypeInfo genericType, Type newType, [CanBeNull] Action additionalGenericTypeUpdate)
        {
            DebugUtility.Log($"{typeof(TObject).Name} typename updated: {genericType.TypeFullName} => {newType.FullName}");

            ConcreteClass[] concreteClasses = GenerationDatabase<TObject>.GetConcreteClasses(genericType);

            // Update database before operating on assemblies
            GenerationDatabase<TObject>.UpdateGenericType(genericType, newType);
            additionalGenericTypeUpdate?.Invoke();

            _concreteClassChecker.UpdateConcreteClassesAssemblies(newType, concreteClasses, genericType.GUID);
        }

        private bool RemoveGenericTypes(IEnumerable<GenericTypeInfo> genericTypes)
        {
            bool removedAssembly = false;

            foreach (GenericTypeInfo genericType in genericTypes)
            {
                removedAssembly |= RemoveGenericType(genericType);
            }

            return removedAssembly;
        }
    }
}