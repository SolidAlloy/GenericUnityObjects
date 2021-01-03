namespace GenericUnityObjects.Editor.MonoBehaviour
{
    using System;
    using System.IO;
    using System.Linq;
    using GenericUnityObjects.Util;
    using ScriptableObject;
    using SolidUtilities.Extensions;
    using SolidUtilities.Helpers;
    using UnityEditor;
    using UnityEngine.Assertions;
    using Util;

    internal static class ConcreteClassCreator
    {
        public static void CreateConcreteClassAssemblyForSO(Type genericTypeWithoutArgs, Type[] argumentTypes)
        {
            string assemblyGUID = CreateConcreteClassAssembly(genericTypeWithoutArgs, argumentTypes);
            SOGenerationDatabase.AddConcreteClass(genericTypeWithoutArgs, argumentTypes, assemblyGUID);
        }

        public static void CreateConcreteClassAssemblyForBehaviour(Type genericTypeWithoutArgs, Type[] argumentTypes)
        {
            string assemblyGUID = CreateConcreteClassAssembly(genericTypeWithoutArgs, argumentTypes);
            BehavioursGenerationDatabase.AddConcreteClass(genericTypeWithoutArgs, argumentTypes, assemblyGUID);
        }

        private static string CreateConcreteClassAssembly(Type genericTypeWithoutArgs, Type[] argumentTypes)
        {
            string assemblyName = GetConcreteClassAssemblyName(genericTypeWithoutArgs, argumentTypes);
            string assemblyPath = $"{Config.AssembliesDirPath}/{assemblyName}.dll";

            string assemblyGUID = null;

            CreatorUtil.WithDisabledAssetDatabase(() =>
            {
                CreateConcreteClassAssembly(genericTypeWithoutArgs, argumentTypes, assemblyName);
                assemblyGUID = AssemblyGeneration.ImportAssemblyAsset(assemblyPath);
            });

            return assemblyGUID;
        }

        public static Type CreateConcreteClassAssembly(Type genericTypeWithoutArgs, Type[] argumentTypes, string newAssemblyName)
        {
            string componentName = "Scripts/" + GetComponentName(genericTypeWithoutArgs, argumentTypes);
            return AssemblyCreator.CreateConcreteClass(newAssemblyName, genericTypeWithoutArgs.MakeGenericType(argumentTypes), componentName);
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