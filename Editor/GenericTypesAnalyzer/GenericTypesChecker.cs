namespace GenericUnityObjects.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GeneratedTypesDatabase;
    using GenericUnityObjects.Util;
    using JetBrains.Annotations;
    using SolidUtilities.Extensions;
    using UnityEditor;
    using Object = UnityEngine.Object;

    internal abstract class GenericTypesChecker<TObject>
        where TObject : Object
    {
        private readonly ConcreteClassChecker<TObject> _concreteClassChecker;

        protected GenericTypesChecker() => _concreteClassChecker = new ConcreteClassChecker<TObject>(this);

        public bool Check()
        {
            var oldGenericTypes = GenerationDatabase<TObject>.GenericTypes;
            var newGenericTypes = TypeCache.GetTypesDerivedFrom<TObject>()
                .Where(type => type.IsGenericType && ! type.IsAbstract)
                .Select(type => new GenericTypeInfo(type))
                .ToArray();

            int oldGenericTypesLength = oldGenericTypes.Length;
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
                    else if (typeNamesDontMatch)
                    {
                        needsAssetDatabaseRefresh = true;
                        UpdateGenericTypeName(oldType, newType.Type);
                        foundMatching = true;
                    }

                    // If type names don't match, the old type name and arguments have already been updated,
                    // so no need to check them twice.
                    if ( ! (typeNamesDontMatch || newType.ArgNames.SequenceEqual(oldType.ArgNames)))
                    {
                        needsAssetDatabaseRefresh |= UpdateGenericTypeArgNames(oldType, newType.ArgNames, newType.Type);
                        foundMatching = true;
                    }

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

        private static bool ActualTypesMatch(GenericTypeInfo oldTypeInfo, GenericTypeInfo newTypeInfo)
        {
            // When GUIDs and TypeNames don't match, there is possibility that the user renamed class name without
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
            GenerationDatabase<TObject>.UpdateGenericTypeGUID(genericType, newGUID);
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
                        UpdateGenericTypeName(genericTypeInfo, genericType);
                    }
                }
                else
                {
                    GenerationDatabase<TObject>.RemoveGenericType(genericTypeInfo, AssemblyAssetOperations.RemoveAssemblyByGUID);
                    continue; // concrete classes are already removed, no need to iterate through them.
                }

                var concreteClasses = GenerationDatabase<TObject>.GetConcreteClassesByArgument(genericTypeInfo, argument);
                _concreteClassChecker.UpdateConcreteClassesAssemblies(genericType, concreteClasses);
            }
        }

        protected abstract void UpdateGenericTypeName(GenericTypeInfo genericType, Type newType);

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
            GenerationDatabase<TObject>.UpdateGenericTypeArgs(genericType, newArgNames);
            return false;
        }

        protected virtual void AddNewGenericType(GenericTypeInfo genericTypeInfo)
        {
            DebugUtility.Log($"{typeof(TObject).Name} added: {genericTypeInfo.TypeFullName}");
            GenerationDatabase<TObject>.AddGenericType(genericTypeInfo);
        }

        protected virtual bool RemoveGenericType(GenericTypeInfo genericType)
        {
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

            _concreteClassChecker.UpdateConcreteClassesAssemblies(newType, concreteClasses);
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