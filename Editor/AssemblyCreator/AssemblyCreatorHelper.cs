namespace GenericUnityObjects.Editor
{
    using System;
    using System.Configuration.Assemblies;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;
    using GenericUnityObjects.Util;
    using UnityEngine;
    using UnityEngine.Assertions;

    internal static class AssemblyCreatorHelper
    {
        private static ConstructorInfo _addComponentMenuConstructor;

        public static ConstructorInfo AddComponentMenuConstructor
        {
            get
            {
                if (_addComponentMenuConstructor == null)
                {
                    _addComponentMenuConstructor = typeof(AddComponentMenu).GetConstructor(
                        BindingFlags.Public | BindingFlags.Instance,
                        null,
                        new[] { typeof(string) },
                        null);

                    Assert.IsNotNull(_addComponentMenuConstructor);
                }

                return _addComponentMenuConstructor;
            }
        }

        private static MethodInfo _getTypeFromHandle;

        public static MethodInfo GetTypeFromHandle
        {
            get
            {
                if (_getTypeFromHandle == null)
                {
                    _getTypeFromHandle = typeof(Type).GetMethod(
                        nameof(Type.GetTypeFromHandle),
                        BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly,
                        null,
                        new[] { typeof(RuntimeTypeHandle) },
                        null);

                    Assert.IsNotNull(_getTypeFromHandle);
                }

                return _getTypeFromHandle;
            }
        }

        public static AssemblyBuilder GetAssemblyBuilder(string generatedAssemblyName)
        {
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName(generatedAssemblyName)
                {
                    CultureInfo = CultureInfo.InvariantCulture,
                    Flags = AssemblyNameFlags.None,
                    ProcessorArchitecture = ProcessorArchitecture.MSIL,
                    VersionCompatibility = AssemblyVersionCompatibility.SameDomain
                },
                AssemblyBuilderAccess.RunAndSave, Config.AssembliesDirPath);

            return assemblyBuilder;
        }

        public static ModuleBuilder GetModuleBuilder(AssemblyBuilder assemblyBuilder, string generatedAssemblyName)
        {
            return assemblyBuilder.DefineDynamicModule($"{generatedAssemblyName}.dll", true);
        }

        public static void AddComponentMenuAttribute(TypeBuilder typeBuilder, string componentName)
        {
            var attributeBuilder = new CustomAttributeBuilder(AddComponentMenuConstructor, new object[] { componentName });
            typeBuilder.SetCustomAttribute(attributeBuilder);
        }
    }
}