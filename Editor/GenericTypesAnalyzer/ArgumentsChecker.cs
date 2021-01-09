namespace GenericUnityObjects.Editor
{
    using System;
    using GeneratedTypesDatabase;
    using UnityEngine.Assertions;
    using Object = UnityEngine.Object;

    internal static class ArgumentsChecker<TObject>
        where TObject : Object
    {
        public static bool Check(GenericTypesChecker<TObject> genericTypesChecker)
        {
            bool needsAssetDatabaseRefresh = false;

            foreach (ArgumentInfo argument in GenerationDatabase<TObject>.Arguments)
            {
                if (argument.RetrieveType<TObject>(out Type type, out bool retrievedFromGUID))
                {
                    if (retrievedFromGUID)
                    {
                        needsAssetDatabaseRefresh = true;
                        UpdateArgumentTypeName(argument, type, genericTypesChecker);
                    }
                }
                else
                {
                    needsAssetDatabaseRefresh = true;
                    GenerationDatabase<TObject>.RemoveArgument(argument, AssemblyAssetOperations.RemoveAssemblyByGUID);
                }
            }

            return needsAssetDatabaseRefresh;
        }

        public static void UpdateArgumentTypeName(ArgumentInfo argument, Type newType, GenericTypesChecker<TObject> genericTypesChecker)
        {
            // Retrieve the array of generic arguments where the old argument was listed
            var referencedGenericTypes = GenerationDatabase<TObject>.GetReferencedGenericTypes(argument);

            // update argument typename in database before updating assemblies and trying to find behaviour because behaviour might also need to be updated, and the argument should already be new
            GenerationDatabase<TObject>.UpdateArgumentNameAndAssembly(ref argument, newType);

            genericTypesChecker.UpdateReferencedGenericTypes(argument, referencedGenericTypes);
        }
    }
}