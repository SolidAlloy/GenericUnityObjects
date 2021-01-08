namespace GenericUnityObjects.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GeneratedTypesDatabase;
    using GenericUnityObjects.Util;
    using SolidUtilities.Extensions;
    using UnityEditor;
    using UnityEngine.Assertions;

    internal static partial class GenericTypesAnalyzer<TObject>
    {
        public static class ScriptableObjectsChecker
        {
            public static bool Check()
            {
                var oldScriptableObjects = GenerationDatabase<TObject>.GenericTypes;
                var newScriptableObjects = TypeCache.GetTypesDerivedFrom<GenericScriptableObject>()
                    .Where(type => type.IsGenericType && ! type.IsAbstract)
                    .Select(type => new GenericTypeInfo(type))
                    .ToArray();

                int oldScriptableObjectsLength = oldScriptableObjects.Length;
                int newScriptableObjectsLength = newScriptableObjects.Length;

                // Optimizations for common cases.
                if (oldScriptableObjectsLength == 0 && newScriptableObjectsLength == 0)
                {
                    return false;
                }

                if (oldScriptableObjectsLength == 0)
                {
                    newScriptableObjects
                        .Where(scriptableObject => scriptableObject.Type.HasAttribute<CreateGenericAssetMenuAttribute>())
                        .ForEach(AddNewScriptableObject);

                    // AddNewScriptableObject only adds a reference to the database without changing any assemblies.
                    return false;
                }

                if (newScriptableObjectsLength == 0)
                {
                    return RemoveScriptableObjects(oldScriptableObjects);
                }

                var oldTypesSet = new HashSet<GenericTypeInfo>(oldScriptableObjects);
                var newTypesSet = new HashSet<GenericTypeInfo>(newScriptableObjects);

                var oldScriptableObjectsOnly = oldTypesSet.ExceptWithAndCreateNew(newTypesSet).ToList();
                var newScriptableObjectsOnly = newTypesSet.ExceptWithAndCreateNew(oldTypesSet).ToArray();

                bool needsAssetDatabaseRefresh = false;

                foreach (GenericTypeInfo newSO in newScriptableObjectsOnly)
                {
                    bool foundMatching = false;

                    for (int i = 0; i < oldScriptableObjectsOnly.Count; i++)
                    {
                        GenericTypeInfo oldSO = oldScriptableObjectsOnly[i];

                        if (newSO.GUID == oldSO.GUID)
                        {
                            if (newSO.TypeNameAndAssembly == oldSO.TypeNameAndAssembly)
                            {
                                GenerationDatabase<TObject>.UpdateGenericTypeArgs(ref oldSO, newSO.ArgNames);
                            }
                            else
                            {
                                needsAssetDatabaseRefresh = true;
                                UpdateScriptableObjectTypeName(oldSO, newSO.Type);
                            }

                            oldScriptableObjectsOnly.Remove(oldSO);
                            foundMatching = true;
                            break;
                        }

                        if (newSO.TypeNameAndAssembly == oldSO.TypeNameAndAssembly)
                        {
                            // if GUIDs were equal, the loop would already be broken, so no need to check equality
                            // new type GUID is empty -> leave the old GUID
                            if (!string.IsNullOrEmpty(newSO.GUID))
                            {
                                // new type GUID is not empty -> update old GUID
                                UpdateScriptableObjectGUID(oldSO, newSO.GUID);
                            }

                            if ( ! newSO.ArgNames.SequenceEqual(oldSO.ArgNames))
                            {
                                needsAssetDatabaseRefresh = true;
                                GenerationDatabase<TObject>.UpdateGenericTypeArgs(ref oldSO, newSO.ArgNames);
                            }

                            oldScriptableObjectsOnly.Remove(oldSO);
                            foundMatching = true;
                            break;
                        }
                    }

                    // There is no matching old type info, so a completely new assembly must be added for this type
                    if ( ! foundMatching && newSO.Type.HasAttribute<CreateGenericAssetMenuAttribute>())
                    {
                        needsAssetDatabaseRefresh = true;
                        AddNewScriptableObject(newSO);
                    }
                }

                if (oldScriptableObjectsOnly.Count == 0)
                    return needsAssetDatabaseRefresh;

                return RemoveScriptableObjects(oldScriptableObjectsOnly);
            }

            private static void AddNewScriptableObject(GenericTypeInfo scriptableObject)
            {
                DebugUtility.Log($"Scriptable Object added: {scriptableObject.TypeFullName}");
                GenerationDatabase<TObject>.AddGenericType(scriptableObject);
            }

            private static bool RemoveScriptableObjects(IEnumerable<GenericTypeInfo> scriptableObjects)
            {
                bool removedAssembly = false;

                foreach (GenericTypeInfo oldScriptableObject in scriptableObjects)
                {
                    removedAssembly = removedAssembly || RemoveScriptableObject(oldScriptableObject);
                }

                return removedAssembly;
            }

            private static bool RemoveScriptableObject(GenericTypeInfo scriptableObject)
            {
                DebugUtility.Log($"Scriptable Object removed: {scriptableObject.TypeFullName}");
                return GenerationDatabase<TObject>.RemoveGenericType(scriptableObject, AssemblyAssetOperations.RemoveAssemblyByGUID);
            }

            private static void UpdateScriptableObjectTypeName(GenericTypeInfo scriptableObject, Type newType)
            {
                DebugUtility.Log($"Scriptable Object typename updated: {scriptableObject.TypeFullName} => {newType.FullName}");

                bool success = GenerationDatabase<TObject>.TryGetConcreteClasses(scriptableObject, out ConcreteClass[] concreteClasses);

                Assert.IsTrue(success);

                // Update database before operating on assemblies
                GenerationDatabase<TObject>.UpdateGenericTypeNameAndAssembly(ref scriptableObject, newType);
                ConcreteClassChecker.UpdateConcreteClassesAssemblies(newType, concreteClasses);
            }

            private static void UpdateScriptableObjectGUID(GenericTypeInfo scriptableObject, string newGUID)
            {
                DebugUtility.Log($"Scriptable Object GUID updated: {scriptableObject.GUID} => {newGUID}");
                GenerationDatabase<TObject>.UpdateGenericTypeGUID(ref scriptableObject, newGUID);
            }
        }
    }
}