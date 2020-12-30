namespace GenericUnityObjects.Util
{
    using System;
    using System.Collections.Generic;
    using System.Configuration.Assemblies;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using UnityEngine.Assertions;

#if ! ENABLE_IL2CPP
    using System.Reflection.Emit;
#endif

    internal static class BehaviourCache
    {
#if ENABLE_IL2CPP
        public static Type GetClass(Type genericBehaviourWithArgs) => null;
#else

        // When module builder tries to emit symbols in a build, it throws NullReferenceException trying to initialize symbolWriter.
#if UNITY_EDITOR
        private const bool EmitSymbolInfo = true;
#else
        private const bool EmitSymbolInfo = false;
#endif

        private const string AssemblyName = "GenericUnityObjects.DynamicAssembly";

        private static readonly AssemblyBuilder AssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
            new AssemblyName(AssemblyName)
        {
            CultureInfo = CultureInfo.InvariantCulture,
            Flags = AssemblyNameFlags.None,
            ProcessorArchitecture = ProcessorArchitecture.MSIL,
            VersionCompatibility = AssemblyVersionCompatibility.SameDomain
        }, AssemblyBuilderAccess.Run);

        private static readonly ModuleBuilder ModuleBuilder = AssemblyBuilder.DefineDynamicModule(AssemblyName, EmitSymbolInfo);

        private static readonly Dictionary<string, Type> ClassDict = new Dictionary<string, Type>();

        public static Type GetClass(Type genericBehaviourWithArgs)
        {
            string className = GetClassName(genericBehaviourWithArgs);

            if (ClassDict.TryGetValue(className, out Type classType))
                return classType;

            classType = CreateClass(className, genericBehaviourWithArgs);
            ClassDict[className] = classType;
            return classType;
        }

        private static string GetClassName(Type genericBehaviourWithArgs)
        {
            string GetIdentifierSafeName(Type type)
            {
                string fullName = type.FullName;
                Assert.IsNotNull(fullName);

                return fullName
                    .Replace('.', '_')
                    .Replace('`', '_');
            }

            string genericTypeName = GetIdentifierSafeName(genericBehaviourWithArgs.GetGenericTypeDefinition());
            var argNames = genericBehaviourWithArgs.GetGenericArguments().Select(GetIdentifierSafeName);

            // Length shouldn't be an issue. The old length limit was 511 characters, but now there seems to be no limit.
            return $"{genericTypeName}_{string.Join("_", argNames)}";
        }

        private static Type CreateClass(string className, Type genericBehaviourWithArgs)
        {
            TypeBuilder typeBuilder = ModuleBuilder.DefineType(className, TypeAttributes.NotPublic, genericBehaviourWithArgs);
            return typeBuilder.CreateType();
        }
#endif
    }
}