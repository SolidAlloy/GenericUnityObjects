namespace GenericUnityObjects.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GeneratedTypesDatabase;
    using GenericUnityObjects.Util;
    using SolidUtilities.Extensions;
    using SolidUtilities.Helpers;
    using UnityEditor;
    using UnityEngine.Assertions;
    using Util;

    internal static partial class GenericTypesAnalyzer<TDatabase>
    {
        private static class BehavioursChecker
        {
            public static bool CheckBehavioursImpl()
            {
                var oldBehaviours = GenerationDatabase<TDatabase>.GenericUnityObjects;
                var newBehaviours = TypeCache.GetTypesDerivedFrom<UnityEngine.MonoBehaviour>()
                    .Where(type => type.IsGenericType && ! type.IsAbstract)
                    .Select(type => new GenericTypeInfo(type))
                    .ToArray();

                int oldBehavioursLength = oldBehaviours.Length;
                int newBehavioursLength = newBehaviours.Length;

                // Optimizations for common cases.
                if (oldBehavioursLength == 0 && newBehavioursLength == 0)
                {
                    return false;
                }

                if (oldBehavioursLength == 0)
                {
                    newBehaviours.ForEach(AddNewBehaviour);
                    return true;
                }

                if (newBehavioursLength == 0)
                {
                    oldBehaviours.ForEach(RemoveBehaviour);
                    return true;
                }

                var oldTypesSet = new HashSet<GenericTypeInfo>(oldBehaviours);
                var newTypesSet = new HashSet<GenericTypeInfo>(newBehaviours);

                var oldBehavioursOnly = oldTypesSet.ExceptWithAndCreateNew(newTypesSet).ToList();
                var newBehavioursOnly = newTypesSet.ExceptWithAndCreateNew(oldTypesSet).ToArray();

                bool needsAssetDatabaseRefresh = false;

                foreach (GenericTypeInfo newBehaviour in newBehavioursOnly)
                {
                    bool foundMatching = false;

                    for (int i = 0; i < oldBehavioursOnly.Count; i++)
                    {
                        GenericTypeInfo oldBehaviour = oldBehavioursOnly[i];

                        if (newBehaviour.GUID == oldBehaviour.GUID)
                        {
                            if (newBehaviour.TypeNameAndAssembly == oldBehaviour.TypeNameAndAssembly)
                            {
                                UpdateBehaviourArgNames(oldBehaviour, newBehaviour.ArgNames, newBehaviour.Type);
                            }
                            else
                            {
                                UpdateBehaviourTypeName(oldBehaviour, newBehaviour.Type);
                            }

                            needsAssetDatabaseRefresh = true;
                            oldBehavioursOnly.Remove(oldBehaviour);
                            foundMatching = true;
                            break;
                        }

                        if (newBehaviour.TypeNameAndAssembly == oldBehaviour.TypeNameAndAssembly)
                        {
                            // new type GUID is empty -> leave the old GUID
                            if (!string.IsNullOrEmpty(newBehaviour.GUID))
                            {
                                // new type GUID is not empty -> update old GUID
                                UpdateBehaviourGUID(oldBehaviour, newBehaviour.GUID);
                            }

                            if ( ! newBehaviour.ArgNames.SequenceEqual(oldBehaviour.ArgNames))
                            {
                                needsAssetDatabaseRefresh = true;
                                UpdateBehaviourArgNames(oldBehaviour, newBehaviour.ArgNames, newBehaviour.Type);
                            }

                            oldBehavioursOnly.Remove(oldBehaviour);
                            foundMatching = true;
                            break;
                        }
                    }

                    // There is no matching old type info, so a completely new assembly must be added for this type
                    if ( ! foundMatching)
                    {
                        needsAssetDatabaseRefresh = true;
                        AddNewBehaviour(newBehaviour);
                    }
                }

                if (oldBehavioursOnly.Count == 0)
                    return needsAssetDatabaseRefresh;

                oldBehavioursOnly.ForEach(RemoveBehaviour);
                return true;
            }

            private static void UpdateBehaviourGUID(GenericTypeInfo behaviour, string newGUID)
            {
                DebugUtility.Log($"Behaviour GUID updated: {behaviour.GUID} => {newGUID}");
                GenerationDatabase<TDatabase>.UpdateGenericTypeGUID(ref behaviour, newGUID);
            }

            private static void RemoveBehaviour(GenericTypeInfo behaviour)
            {
                DebugUtility.Log($"Behaviour removed: {behaviour.TypeFullName}");
                GenerationDatabase<TDatabase>.RemoveGenericType(behaviour, AssemblyAssetOperations.RemoveAssemblyByGUID);
            }

            private static void UpdateBehaviourArgNames(GenericTypeInfo behaviour, string[] newArgNames, Type newType)
            {
                DebugUtility.Log($"Behaviour args updated: '{string.Join(", ", behaviour.ArgNames)}' => '{string.Join(", ", behaviour.ArgNames)}'");

                GenerationDatabase<TDatabase>.UpdateGenericTypeArgs(ref behaviour, newArgNames);

                UpdateSelectorAssembly(behaviour.AssemblyGUID, newType);
            }

            private static string GetSelectorAssemblyName(Type genericTypeWithoutArgs) => genericTypeWithoutArgs.FullName.MakeClassFriendly();

            private static void AddNewBehaviour(GenericTypeInfo behaviour)
            {
                DebugUtility.Log($"Behaviour added: {behaviour.TypeFullName}");

                Type behaviourType = behaviour.Type;
                Assert.IsNotNull(behaviourType);

                string assemblyName = GetSelectorAssemblyName(behaviourType);
                CreateSelectorAssembly(behaviourType, assemblyName);

                string assemblyPath = $"{Config.AssembliesDirPath}/{assemblyName}.dll";
                behaviour.AssemblyGUID = AssemblyGeneration.ImportAssemblyAsset(assemblyPath);

                GenerationDatabase<TDatabase>.AddGenericType(behaviour);
            }

            private static void UpdateSelectorAssembly(string selectorAssemblyGUID, Type newType)
            {
                string newAssemblyName = GetSelectorAssemblyName(newType);

                AssemblyAssetOperations.ReplaceAssemblyByGUID(selectorAssemblyGUID, newAssemblyName, () =>
                {
                    CreateSelectorAssembly(newType, newAssemblyName);
                });
            }

            private static void CreateSelectorAssembly(Type genericTypeWithoutArgs, string assemblyName)
            {
                Type[] genericArgs = genericTypeWithoutArgs.GetGenericArguments();
                string componentName = "Scripts/" + TypeUtility.GetShortNameWithBrackets(genericTypeWithoutArgs, genericArgs);
                AssemblyCreator.CreateSelectorAssembly(assemblyName, genericTypeWithoutArgs, componentName);
            }

            private static void UpdateBehaviourTypeName(GenericTypeInfo behaviour, Type newType)
            {
                DebugUtility.Log($"Behaviour typename updated: {behaviour.TypeFullName} => {newType.FullName}");

                bool success = GenerationDatabase<TDatabase>.TryGetConcreteClasses(behaviour, out ConcreteClass[] concreteClasses);

                Assert.IsTrue(success);

                // Update database before operating on assemblies
                GenerationDatabase<TDatabase>.UpdateBehaviourNameAndAssembly(ref behaviour, newType);

                UpdateSelectorAssembly(behaviour.AssemblyGUID, newType);

                ConcreteClassChecker.UpdateConcreteClassesAssemblies(newType, concreteClasses);
            }

            public static void UpdateReferencedBehaviours(ArgumentInfo argument, GenericTypeInfo[] referencedBehaviours)
            {
                foreach (GenericTypeInfo behaviour in referencedBehaviours)
                {
                    // Retrieve type of the behaviour
                    // If type cannot be retrieved, try finding by GUID
                    // If GUID didn't help, remove the whole collection of assemblies
                    if (behaviour.RetrieveType<TDatabase>(out Type behaviourType, out bool retrievedFromGUID))
                    {
                        if (retrievedFromGUID)
                        {
                            UpdateBehaviourTypeName(behaviour, behaviourType);
                        }
                    }
                    else
                    {
                        GenerationDatabase<TDatabase>.RemoveGenericType(behaviour, AssemblyAssetOperations.RemoveAssemblyByGUID);
                        continue; // concrete classes are already removed, no need to iterate through them.
                    }

                    bool concreteClassesSuccess = GenerationDatabase<TDatabase>.TryGetConcreteClassesByArgument(behaviour, argument, out ConcreteClass[] concreteClasses);

                    Assert.IsTrue(concreteClassesSuccess);

                    ConcreteClassChecker.UpdateConcreteClassesAssemblies(behaviourType, concreteClasses);
                }
            }
        }
    }
}