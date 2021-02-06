namespace GenericUnityObjects.Util
{
    using System;
    using System.Linq;
    using JetBrains.Annotations;
    using SolidUtilities.Extensions;
    using SolidUtilities.Helpers;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    internal static class TypeUtility
    {
        [CanBeNull]
        public static Type GetEmptyTypeDerivedFrom(Type parentType)
        {
#if UNITY_EDITOR
            return TypeCache.GetTypesDerivedFrom(parentType)
                .FirstOrDefault(type => type.IsEmpty());
#else
            return parentType.Assembly.GetTypes()
                .Where(parentType.IsAssignableFrom)
                .FirstOrDefault(type => type.IsEmpty());
#endif
        }

        public static string GetTypeNameAndAssembly(Type type)
        {
            if (type == null)
                return string.Empty;

            if (type.FullName == null)
                throw new ArgumentException($"'{type}' does not have full name.", nameof(type));

            return GetTypeNameAndAssembly(type.FullName, type.Assembly.GetName().Name);
        }

        public static string GetTypeNameAndAssembly(string typeFullName, string assemblyName) =>
            $"{typeFullName}, {assemblyName}";

        /// <summary>
        /// Gets a type name for nice representation of the type. It looks like this: ClassName&lt;T1,T2>.
        /// </summary>
        public static string GetNiceNameOfGenericTypeDefinition(Type genericTypeWithoutArgs, bool fullName = false)
        {
            string typeNameWithoutBrackets = fullName
                ? genericTypeWithoutArgs.FullName.StripGenericSuffix().Replace('.', '/')
                : genericTypeWithoutArgs.Name.StripGenericSuffix();

            var argumentNames = GetNiceArgsOfGenericTypeDefinition(genericTypeWithoutArgs);
            return $"{typeNameWithoutBrackets}<{string.Join(",", argumentNames)}>";
        }

        public static string[] GetNiceArgsOfGenericTypeDefinition(Type genericTypeWithoutArgs)
        {
            Type[] genericArgs = genericTypeWithoutArgs.GetGenericArguments();
            return genericArgs.Select(argument => argument.Name).ToArray();
        }

        /// <summary>
        /// Gets a type name for nice representation of the type. It looks like this: ClassName&lt;int,TestArg>.
        /// </summary>
        public static string GetNiceNameOfGenericType(Type genericTypeWithArgs, bool fullName = false)
        {
            string typeNameWithoutSuffix = fullName
                ? genericTypeWithArgs.FullName.StripGenericSuffix().Replace('.', '/')
                : genericTypeWithArgs.Name.StripGenericSuffix();

            var argumentNames = genericTypeWithArgs.GetGenericArguments()
                .Select(argument => argument.FullName)
                .Select(argFullName => argFullName.ReplaceWithBuiltInName())
                .Select(argFullName => argFullName.GetSubstringAfterLast('.'));

            return $"{typeNameWithoutSuffix}<{string.Join(",", argumentNames)}>";
        }
    }
}