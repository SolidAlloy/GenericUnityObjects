#if (UNITY_EDITOR || UNITY_STANDALONE) && !ENABLE_IL2CPP && !NET_STANDARD_2_0
#define CAN_EMIT
#endif

namespace GenericUnityObjects.Util
{
    using System;
    using System.Collections.Generic;
    using System.Configuration.Assemblies;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using UnityEngine.Assertions;

#if CAN_EMIT
    using System.Reflection.Emit;
#endif

    /// <summary>
    /// A class that emits classes and stores them in cache so that they are not emitted twice.
    /// </summary>
    internal static class GeneratedUnityObjectCache
    {
        // MonoBehaviours and ScriptableObjects are emitted identically. There are two caches to reduce number of
        // objects in each cache and speed up the search.
        private static readonly CacheImplementation _behaviourCache = new CacheImplementation();
        private static readonly CacheImplementation _scriptableObjectCache = new CacheImplementation();

        public static Type GetBehaviourClass(Type genericBehaviourWithArgs) => _behaviourCache.GetClass(genericBehaviourWithArgs);

        public static Type GetSOClass(Type genericSOWithArgs) => _scriptableObjectCache.GetClass(genericSOWithArgs);

        private class CacheImplementation
        {
            public Type GetClass(Type genericTypeWithArgs)
            {
#if !CAN_EMIT
                return null;
            }
#else
                string className = GetClassName(genericTypeWithArgs);

                if (_classDict.TryGetValue(className, out Type classType))
                    return classType;

                classType = CreateClass(className, genericTypeWithArgs);
                _classDict[className] = classType;
                return classType;
            }

            // When module builder tries to emit symbols in a build, it throws NullReferenceException while initializing symbolWriter.
#if UNITY_EDITOR
            private const bool EmitSymbolInfo = true;
#else
            private const bool EmitSymbolInfo = false;
#endif

            private const string AssemblyName = "GenericUnityObjects.DynamicAssembly";

            private static readonly AssemblyBuilder _assemblyBuilder =
#if NET_STANDARD
                AssemblyBuilder
#else
                AppDomain.CurrentDomain
#endif
                .DefineDynamicAssembly(
                    new AssemblyName(AssemblyName)
                    {
                        CultureInfo = CultureInfo.InvariantCulture,
                        Flags = AssemblyNameFlags.None,
                        ProcessorArchitecture = ProcessorArchitecture.MSIL,
                        VersionCompatibility = AssemblyVersionCompatibility.SameDomain
                    }, AssemblyBuilderAccess.Run);

            private static readonly ModuleBuilder _moduleBuilder = _assemblyBuilder.DefineDynamicModule(AssemblyName
#if ! NET_STANDARD
                , EmitSymbolInfo
#endif
                );

            private readonly Dictionary<string, Type> _classDict = new Dictionary<string, Type>();

            private static string GetClassName(Type genericTypeWithArgs)
            {
                string GetIdentifierSafeName(Type type)
                {
                    string fullName = type.FullName;
                    Assert.IsNotNull(fullName);

                    return fullName
                        .Replace('.', '_')
                        .Replace('`', '_');
                }

                string genericTypeName = GetIdentifierSafeName(genericTypeWithArgs.GetGenericTypeDefinition());
                var argNames = genericTypeWithArgs.GetGenericArguments().Select(GetIdentifierSafeName);

                // Length shouldn't be an issue. The old length limit was 511 characters, but now there seems to be no limit.
                return $"{genericTypeName}_{string.Join("_", argNames)}";
            }

            private static Type CreateClass(string className, Type genericTypeWithArgs)
            {
                TypeBuilder typeBuilder = _moduleBuilder.DefineType(className, TypeAttributes.NotPublic, genericTypeWithArgs);
                return typeBuilder.CreateType();
            }
#endif
        }
    }
}