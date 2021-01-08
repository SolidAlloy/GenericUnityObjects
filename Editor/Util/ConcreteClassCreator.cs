namespace GenericUnityObjects.Editor.Util
{
    using System;
    using System.IO;
    using System.Linq;
    using GeneratedTypesDatabase;
    using GenericUnityObjects.Util;
    using SolidUtilities.Extensions;
    using UnityEditor;

    internal abstract class ConcreteClassCreator
    {
        protected void CreateConcreteClassImpl(Type genericTypeWithoutArgs, Type[] argumentTypes)
        {
            string assemblyGUID = CreateConcreteClassAssembly(genericTypeWithoutArgs, argumentTypes);
            AddToDatabase(genericTypeWithoutArgs, argumentTypes, assemblyGUID);
        }

        protected void UpdateConcreteClassAssemblyImpl(Type genericType, Type[] argumentTypes, ConcreteClass concreteClass)
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

            AssemblyAssetOperations.ReplaceAssemblyByGUID(concreteClass.AssemblyGUID, newAssemblyName,
                () => CreateConcreteClassAssembly(genericType, argumentTypes, newAssemblyName));
        }

        protected abstract void CreateConcreteClassAssembly(Type genericTypeWithoutArgs, Type[] argumentTypes,
            string newAssemblyName);

        protected abstract void AddToDatabase(Type genericTypeWithoutArgs, Type[] argumentTypes, string assemblyGUID);

        private string CreateConcreteClassAssembly(Type genericTypeWithoutArgs, Type[] argumentTypes)
        {
            string assemblyName = GetConcreteClassAssemblyName(genericTypeWithoutArgs, argumentTypes);
            string assemblyPath = $"{Config.AssembliesDirPath}/{assemblyName}.dll";

            string assemblyGUID;

            using (new DisabledAssetDatabase(null))
            {
                CreateConcreteClassAssembly(genericTypeWithoutArgs, argumentTypes, assemblyName);
                assemblyGUID = AssemblyGeneration.ImportAssemblyAsset(assemblyPath);
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

    internal class ConcreteBehaviourCreator : ConcreteClassCreator
    {
        private static readonly ConcreteBehaviourCreator _creatorInstance = new ConcreteBehaviourCreator();

        public static void CreateConcreteClass(Type genericTypeWithoutArgs, Type[] argumentTypes) =>
            _creatorInstance.CreateConcreteClassImpl(genericTypeWithoutArgs, argumentTypes);

        public static void UpdateConcreteClassAssembly(Type genericType, Type[] argumentTypes, ConcreteClass concreteClass) =>
            _creatorInstance.UpdateConcreteClassAssemblyImpl(genericType, argumentTypes, concreteClass);

        protected override void AddToDatabase(Type genericTypeWithoutArgs, Type[] argumentTypes, string assemblyGUID) =>
            BehavioursGenerationDatabase.AddConcreteClass(genericTypeWithoutArgs, argumentTypes, assemblyGUID);

        protected override void CreateConcreteClassAssembly(Type genericTypeWithoutArgs, Type[] argumentTypes, string newAssemblyName) =>
            AssemblyCreator.CreateBehaviourConcreteClass(newAssemblyName, genericTypeWithoutArgs.MakeGenericType(argumentTypes));
    }

    internal class ConcreteSOCreator : ConcreteClassCreator
    {
        private static readonly ConcreteSOCreator _creatorInstance = new ConcreteSOCreator();

        public static void CreateConcreteClass(Type genericTypeWithoutArgs, Type[] argumentTypes) =>
            _creatorInstance.CreateConcreteClassImpl(genericTypeWithoutArgs, argumentTypes);

        public static void UpdateConcreteClassAssembly(Type genericType, Type[] argumentTypes, ConcreteClass concreteClass) =>
            _creatorInstance.UpdateConcreteClassAssemblyImpl(genericType, argumentTypes, concreteClass);

        protected override void AddToDatabase(Type genericTypeWithoutArgs, Type[] argumentTypes, string assemblyGUID) =>
            SOGenerationDatabase.AddConcreteClass(genericTypeWithoutArgs, argumentTypes, assemblyGUID);

        protected override void CreateConcreteClassAssembly(Type genericTypeWithoutArgs, Type[] argumentTypes, string newAssemblyName) =>
            AssemblyCreator.CreateSOConcreteClass(newAssemblyName, genericTypeWithoutArgs.MakeGenericType(argumentTypes));
    }
}