namespace GenericUnityObjects.Editor
{
    using System;
    using GeneratedTypesDatabase;
    using UnityEngine.Assertions;

    internal static partial class GenericTypesAnalyzer<TDatabase>
    {
        public static class ArgumentsChecker
        {
            public static bool Check()
            {
                bool needsAssetDatabaseRefresh = false;

                foreach (ArgumentInfo argument in GenerationDatabase<TDatabase>.Arguments)
                {
                    if (argument.RetrieveType<TDatabase>(out Type type, out bool retrievedFromGUID))
                    {
                        if (retrievedFromGUID)
                        {
                            needsAssetDatabaseRefresh = true;
                            UpdateArgumentTypeName(argument, type);
                        }
                    }
                    else
                    {
                        needsAssetDatabaseRefresh = true;
                        GenerationDatabase<TDatabase>.RemoveArgument(argument, AssemblyAssetOperations.RemoveAssemblyByGUID);
                    }
                }

                return needsAssetDatabaseRefresh;
            }

            public static void UpdateArgumentTypeName(ArgumentInfo argument, Type newType)
            {
                // Retrieve the array of generic arguments where the old argument was listed
                bool behavioursSuccess = GenerationDatabase<TDatabase>.TryGetReferencedGenericTypes(argument, out GenericTypeInfo[] referencedBehaviours);

                // update argument typename in database before updating assemblies and trying to find behaviour because behaviour might also need to be updated, and the argument should already be new
                GenerationDatabase<TDatabase>.UpdateArgumentNameAndAssembly(ref argument, newType);

                Assert.IsTrue(behavioursSuccess);

                // need to split the method into two types
                BehavioursChecker.UpdateReferencedGenericTypes(argument, referencedBehaviours);
            }
        }
    }
}