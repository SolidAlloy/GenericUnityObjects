namespace GenericUnityObjects.Editor
{
    using System;
    using GeneratedTypesDatabase;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Checks if any argument types were changed/updated, updates them in the database and regenerates DLLs if needed.
    /// </summary>
    /// <typeparam name="TObject"> A type derived from <see cref="UnityEngine.Object"/>. </typeparam>
    internal static class ArgumentsChecker<TObject>
        where TObject : Object
    {
        public static bool Check(GenericTypesChecker<TObject> genericTypesChecker)
        {
            bool needsAssetDatabaseRefresh = false;

            foreach (ArgumentInfo argument in GenerationDatabase<TObject>.ArgumentGenericTypes.Keys)
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
            // Retrieve an array of generic arguments where the old argument was listed
            var referencedGenericTypes = GenerationDatabase<TObject>.GetReferencedGenericTypes(argument);

            // Update argument typename in the database before updating assemblies and trying to find behaviour
            // because behaviour might also need to be updated, and the argument should already be new.
            GenerationDatabase<TObject>.UpdateArgumentNameAndAssembly(argument, newType);

            genericTypesChecker.UpdateReferencedGenericTypes(argument, referencedGenericTypes);
        }
    }
}