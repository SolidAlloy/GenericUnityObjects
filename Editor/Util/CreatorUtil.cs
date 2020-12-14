namespace GenericUnityObjects.Editor
{
    using System;
    using System.IO;
    using System.Linq;
    using JetBrains.Annotations;
    using SolidUtilities.Extensions;
    using SolidUtilities.Helpers;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Assertions;
    using TypeCache = UnityEditor.TypeCache;

    internal static class CreatorUtil
    {
        [CanBeNull]
        public static Type GetEmptyTypeDerivedFrom(Type parentType)
        {
            var foundTypes = TypeCache.GetTypesDerivedFrom(parentType);

            if (foundTypes.Count == 0)
                return null;

            // Why would there be another empty type derived from the GenericScriptableObject other than the auto-generated one?
            Assert.IsTrue(foundTypes.Count == 1);

            Type matchingType = foundTypes.FirstOrDefault(type => type.IsEmpty());
            return matchingType;
        }

        /// <summary>
        /// Gets the type name for using in scripts.
        /// It looks like this: Namespace.ClassName&lt;Namespace.FirstGenericArg, Namespace.SecondGenericArg>.
        /// </summary>
        public static string GetFullNameWithBrackets(Type genericTypeWithoutArgs, Type[] genericArgs)
        {
            Assert.IsTrue(genericTypeWithoutArgs.IsGenericTypeDefinition);

            string typeFullName = genericTypeWithoutArgs.FullName;
            Assert.IsNotNull(typeFullName);

            string typeNameWithoutBrackets = typeFullName.StripGenericSuffix();

            var argumentNames = genericArgs
                .Select(argument => argument.FullName)
                .Select(fullName => fullName.ReplaceWithBuiltInName());

            return $"{typeNameWithoutBrackets}<{string.Join(",", argumentNames)}>";
        }

        /// <summary>
        /// Gets the type name for nice representation of the type. It looks like this: ClassName&lt;T1,T2>.
        /// </summary>
        public static string GetShortNameWithBrackets(Type type, Type[] genericArgs)
        {
            string typeNameWithoutBrackets = type.Name.StripGenericSuffix();
            var argumentNames = genericArgs.Select(argument => argument.Name);
            return $"{typeNameWithoutBrackets}<{string.Join(",", argumentNames)}>";
        }

        public static void CheckInvalidName(string typeName)
        {
            if (AssetDatabase.FindAssets(typeName).Length != 0)
                return;

            Debug.LogWarning($"Make sure a script that contains the {typeName} type is named the same way, with specifying the number of arguments at the end.\n" +
                             "It will help the plugin not lose a reference to the type should you rename it later.");
        }

        /// <summary>
        /// Gets the full type name with brackets but without generic arguments names.
        /// It looks like this: Namespace.ClassName&lt;,>.
        /// </summary>
        public static string GetGenericTypeDefinitionName(Type type)
        {
            string fullTypeName = type.FullName;
            Assert.IsNotNull(fullTypeName);
            string typeNameWithoutBrackets = fullTypeName.StripGenericSuffix();
            int argsCount = type.GetGenericArguments().Length;
            return $"{typeNameWithoutBrackets}<{new string(',', argsCount-1)}>";
        }

        public static void WriteAllText(string filePath, string text, string directoryPath = null)
        {
            if (directoryPath == null)
            {
                directoryPath = Path.GetDirectoryName(filePath);
                Assert.IsNotNull(directoryPath);
            }

            Directory.CreateDirectory(directoryPath);
            File.WriteAllText(filePath, text);
        }
    }
}