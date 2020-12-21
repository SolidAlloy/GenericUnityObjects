namespace GenericUnityObjects.Editor.MonoBehaviour
{
    using System;
    using System.IO;
    using System.Linq;
    using SolidUtilities.Extensions;
    using SolidUtilities.Helpers;
    using UnityEditor;
    using UnityEngine.Assertions;
    using Util;

    internal static class ConcreteClassCreator
    {
        public static void CreateConcreteClassAssembly(Type genericTypeWithoutArgs, Type[] argumentTypes)
        {
            string assemblyName = GetConcreteClassAssemblyName(genericTypeWithoutArgs, argumentTypes);
            CreateConcreteClassAssembly(genericTypeWithoutArgs, argumentTypes, assemblyName);
            string assemblyPath = $"{Config.AssembliesDirPath}/{assemblyName}.dll";

            AssetDatabase.StartAssetEditing();
            AssetDatabase.ImportAsset(assemblyPath);
            AssetDatabase.ImportAsset(assemblyPath + ".mdb");
            AssetDatabase.StopAssetEditing();

            string assemblyGUID = AssetDatabase.AssetPathToGUID(assemblyPath);
            Assert.IsFalse(string.IsNullOrEmpty(assemblyGUID));

            GenericBehavioursDatabase.AddConcreteClass(genericTypeWithoutArgs, argumentTypes, assemblyGUID);
        }

        public static void CreateConcreteClassAssembly(Type genericTypeWithoutArgs, Type[] argumentTypes, string newAssemblyName)
        {
            string componentName = "Scripts/" + GetComponentName(genericTypeWithoutArgs, argumentTypes);
            AssemblyCreator.CreateConcreteClass(newAssemblyName, genericTypeWithoutArgs.MakeGenericType(argumentTypes), componentName);
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