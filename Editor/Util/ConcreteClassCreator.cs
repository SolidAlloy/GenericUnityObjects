namespace GenericUnityObjects.Editor.Util
{
    using System;
    using System.IO;
    using System.Linq;
    using GeneratedTypesDatabase;
    using GenericUnityObjects.Util;
    using SolidUtilities.Extensions;
    using SolidUtilities.Helpers;
    using UnityEditor;
    using UnityEngine.Assertions;

    internal class BehaviourCreator : ConcreteClassCreator
    {
        public static void CreateConcreteClass(Type genericTypeWithoutArgs, Type[] argumentTypes)
        {
            string assemblyGUID = CreateConcreteClassAssembly(genericTypeWithoutArgs, argumentTypes, CreateConcreteClassAssembly);
            BehavioursGenerationDatabase.AddConcreteClass(genericTypeWithoutArgs, argumentTypes, assemblyGUID);
        }

        public static void CreateConcreteClassAssembly(Type genericTypeWithoutArgs, Type[] argumentTypes, string newAssemblyName)
        {
            string componentName = "Scripts/" + GetComponentName(genericTypeWithoutArgs, argumentTypes);
            AssemblyCreator.CreateConcreteClassForBehaviour(newAssemblyName, genericTypeWithoutArgs.MakeGenericType(argumentTypes), componentName);
        }

        private static string GetComponentName(Type genericTypeWithoutArgs, Type[] genericArgs)
        {
            Assert.IsTrue(genericTypeWithoutArgs.IsGenericTypeDefinition);

            string shortName = genericTypeWithoutArgs.Name;
            string typeNameWithoutSuffix = shortName.StripGenericSuffix();

            var argumentNames = genericArgs
                .Select(argument => argument.FullName)
                .Select(fullName => fullName.ReplaceWithBuiltInName())
                .Select(fullName => fullName.GetSubstringAfterLast('.'));

            return $"{typeNameWithoutSuffix}<{string.Join(",", argumentNames)}>";
        }
    }

    internal class ScriptableObjectCreator : ConcreteClassCreator
    {
        public static void CreateConcreteClass(Type genericTypeWithoutArgs, Type[] argumentTypes)
        {
            string assemblyGUID = CreateConcreteClassAssembly(genericTypeWithoutArgs, argumentTypes, CreateConcreteClassAssembly);
            SOGenerationDatabase.AddConcreteClass(genericTypeWithoutArgs, argumentTypes, assemblyGUID);
        }

        public static void CreateConcreteClassAssembly(Type genericTypeWithoutArgs, Type[] argumentTypes, string newAssemblyName)
        {
            AssemblyCreator.CreateConcreteClassForSO(newAssemblyName, genericTypeWithoutArgs.MakeGenericType(argumentTypes));
        }
    }

    internal class ConcreteClassCreator
    {
        public static string GetConcreteClassAssemblyName(Type genericTypeWithoutArgs, Type[] genericArgs)
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

        protected static string CreateConcreteClassAssembly(Type genericTypeWithoutArgs, Type[] argumentTypes, Action<Type, Type[], string> createAssemblyImpl)
        {
            string assemblyName = GetConcreteClassAssemblyName(genericTypeWithoutArgs, argumentTypes);
            string assemblyPath = $"{Config.AssembliesDirPath}/{assemblyName}.dll";

            string assemblyGUID = null;

            AssetDatabaseHelper.WithDisabledAssetDatabase(() =>
            {
                createAssemblyImpl(genericTypeWithoutArgs, argumentTypes, assemblyName);
                assemblyGUID = AssemblyGeneration.ImportAssemblyAsset(assemblyPath);
            });

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
    }
}