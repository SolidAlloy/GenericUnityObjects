namespace GenericUnityObjects.Editor.Util
{
    using System;
    using System.IO;
    using System.Linq;
    using GeneratedTypesDatabase;
    using GenericUnityObjects.Util;
    using SolidUtilities.Extensions;
    using UnityEditor;
    using Object = UnityEngine.Object;

    internal static class ConcreteClassCreator<TObject>
        where TObject : Object
    {
        public static void CreateConcreteClass(Type genericTypeWithoutArgs, Type[] argumentTypes)
        {
            string assemblyGUID = CreateConcreteClassAssembly(genericTypeWithoutArgs, argumentTypes);
            PersistentStorage.AddAssemblyForIconChange<TObject>(assemblyGUID);
            AddToDatabase(genericTypeWithoutArgs, argumentTypes, assemblyGUID);
        }

        public static void UpdateConcreteClassAssembly(Type genericType, Type[] argumentTypes, ConcreteClass concreteClass)
        {
            string newAssemblyName;

            try
            {
                newAssemblyName = GetConcreteClassAssemblyName(genericType, argumentTypes);
            }
            catch (TypeLoadException)
            {
                return;
            }

            using (AssemblyAssetOperations.AssemblyReplacer.UsingGUID(concreteClass.AssemblyGUID, newAssemblyName))
            {
                CreateConcreteClassAssembly(genericType, argumentTypes, newAssemblyName, concreteClass.AssemblyGUID);
            }

            PersistentStorage.AddAssemblyForIconChange<TObject>(concreteClass.AssemblyGUID);
        }

        private static void CreateConcreteClassAssembly(Type genericTypeWithoutArgs, Type[] argumentTypes,
            string newAssemblyName, string assemblyGUID)
        {
            AssemblyCreator.CreateConcreteClass<TObject>(newAssemblyName, genericTypeWithoutArgs.MakeGenericType(argumentTypes), assemblyGUID);
        }

        private static void AddToDatabase(Type genericTypeWithoutArgs, Type[] argumentTypes, string assemblyGUID)
        {
            GenerationDatabase<TObject>.AddConcreteClass(genericTypeWithoutArgs, argumentTypes, assemblyGUID);
        }

        private static string CreateConcreteClassAssembly(Type genericTypeWithoutArgs, Type[] argumentTypes)
        {
            string assemblyName = GetConcreteClassAssemblyName(genericTypeWithoutArgs, argumentTypes);
            string assemblyPath = $"{Config.AssembliesDirPath}/{assemblyName}.dll";

            string assemblyGUID;

            using (new DisabledAssetDatabase(null))
            {
                assemblyGUID = AssemblyGeneration.GetUniqueGUID();
                CreateConcreteClassAssembly(genericTypeWithoutArgs, argumentTypes, assemblyName, assemblyGUID);
                AssemblyGeneration.ImportAssemblyAsset(assemblyPath, assemblyGUID);
            }

            return assemblyGUID;
        }

        private static string GetShortNameForNaming(string fullName)
        {
            int lastDotIndex = fullName.LastIndexOf('.');

            if (lastDotIndex != -1)
            {
                fullName = fullName.Substring(lastDotIndex + 1, fullName.Length - lastDotIndex - 1);
            }

            int backtickIndex = fullName.IndexOf('`');

            if (backtickIndex != -1)
            {
                fullName = fullName.Substring(0, backtickIndex);
            }

            return fullName;
        }

        private static string GetConcreteClassAssemblyName(Type genericTypeWithoutArgs, Type[] genericArgs)
        {
            var argumentsNames = genericArgs.Select(argument => GetShortNameForNaming(argument.FullName));
            string newAssemblyName = $"{GetShortNameForNaming(genericTypeWithoutArgs.FullName)}_{string.Join("_", argumentsNames)}";
            var dirInfo = new DirectoryInfo(Config.AssembliesDirPath);
            int identicalFilesCount = dirInfo.GetFiles($"{newAssemblyName}*.dll").Length;

            if (identicalFilesCount == 0)
                return newAssemblyName;

            Type genericTypeWithArgs = genericTypeWithoutArgs.MakeGenericType(genericArgs);
            bool typeExists = TypeCache.GetTypesDerivedFrom(genericTypeWithArgs).Any(type => type.IsEmpty());

            if (typeExists)
                throw new TypeLoadException($"The type {genericTypeWithArgs} already exists. No need to create one more.");

            newAssemblyName += $"_{identicalFilesCount}";
            return newAssemblyName;
        }
    }
}