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
    using UnityEngine.Assertions;
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

                    if (newType.GUID == oldType.GUID)
                    {
                        if (newType.TypeNameAndAssembly == oldType.TypeNameAndAssembly)
                        {
                            needsAssetDatabaseRefresh |= UpdateGenericTypeArgNames(oldType, newType.ArgNames, newType.Type);
                        }
                        else
                        {
                            needsAssetDatabaseRefresh = true;
                            UpdateGenericTypeName(oldType, newType.Type);
                        }

                        oldTypesOnly.Remove(oldType);
                        foundMatching = true;
                        break;
                    }

                    if (newType.TypeNameAndAssembly == oldType.TypeNameAndAssembly)
                    {
                        // new type GUID is empty -> leave the old GUID
                        if (!string.IsNullOrEmpty(newType.GUID))
                        {
                            // new type GUID is not empty -> update old GUID
                            UpdateGenericTypeGUID(oldType, newType.GUID);
                        }

                        if ( ! newType.ArgNames.SequenceEqual(oldType.ArgNames))
                        {
                            needsAssetDatabaseRefresh |= UpdateGenericTypeArgNames(oldType, newType.ArgNames, newType.Type);
                        }

                        oldTypesOnly.Remove(oldType);
                        foundMatching = true;
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

        private static void UpdateGenericTypeGUID(GenericTypeInfo genericType, string newGUID)
        {
            DebugUtility.Log($"{typeof(TObject).Name} GUID updated: {genericType.GUID} => {newGUID}");
            GenerationDatabase<TObject>.UpdateGenericTypeGUID(ref genericType, newGUID);
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

                bool concreteClassesSuccess = GenerationDatabase<TObject>.TryGetConcreteClassesByArgument(genericTypeInfo, argument, out ConcreteClass[] concreteClasses);

                Assert.IsTrue(concreteClassesSuccess);

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
            GenerationDatabase<TObject>.UpdateGenericTypeArgs(ref genericType, newArgNames);
            return false;
        }

        protected virtual void AddNewGenericType(GenericTypeInfo genericTypeInfo)
        {
            DebugUtility.Log($"{typeof(TObject).Name} added: {genericTypeInfo.TypeFullName}");
            GenerationDatabase<TObject>.AddGenericType(genericTypeInfo);
        }

        protected virtual bool RemoveGenericType(GenericTypeInfo genericType)
        {
            DebugUtility.Log($"Behaviour removed: {genericType.TypeFullName}");
            return GenerationDatabase<TObject>.RemoveGenericType(genericType, AssemblyAssetOperations.RemoveAssemblyByGUID);
        }

        protected void UpdateGenericTypeName(GenericTypeInfo genericType, Type newType, [CanBeNull] Action additionalGenericTypeUpdate)
        {
            DebugUtility.Log($"{typeof(TObject).Name} typename updated: {genericType.TypeFullName} => {newType.FullName}");

            bool success = GenerationDatabase<TObject>.TryGetConcreteClasses(genericType, out ConcreteClass[] concreteClasses);

            Assert.IsTrue(success);

            // Update database before operating on assemblies
            GenerationDatabase<TObject>.UpdateGenericTypeNameAndAssembly(ref genericType, newType);
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