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
    using Util;

    internal static partial class AssemblyCreator
    {
        private static ConstructorInfo _addComponentMenuConstructor;

        private static ConstructorInfo AddComponentMenuConstructor
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

        private static MethodInfo GetTypeFromHandle
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

        public static void CreateSelectorAssembly(string assemblyName, Type genericBehaviourWithoutArgs, string componentName) =>
            BehaviourSelectorCreator.CreateSelectorAssemblyImpl(assemblyName, genericBehaviourWithoutArgs, componentName);

        public static Type CreateConcreteClassForBehaviour(string assemblyName, Type genericBehaviourWithArgs, string componentName) =>
            ConcreteClassCreator.CreateConcreteClassImpl(assemblyName, genericBehaviourWithArgs, componentName);

        public static Type CreateConcreteClassForSO(string assemblyName, Type genericBehaviourWithArgs) =>
            ConcreteClassCreator.CreateConcreteClassImpl(assemblyName, genericBehaviourWithArgs);

        public static void CreateMenuItems(string assemblyName, MenuItemMethod[] menuItemMethods) =>
            MenuItemsCreator.CreateMenuItemsImpl(assemblyName, menuItemMethods);

        private static AssemblyBuilder GetAssemblyBuilder(string generatedAssemblyName)
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

        private static ModuleBuilder GetModuleBuilder(AssemblyBuilder assemblyBuilder, string generatedAssemblyName)
        {
            return assemblyBuilder.DefineDynamicModule($"{generatedAssemblyName}.dll", true);
        }

        private static void AddComponentMenuAttribute(TypeBuilder typeBuilder, string componentName)
        {
            var attributeBuilder = new CustomAttributeBuilder(AddComponentMenuConstructor, new object[] { componentName });
            typeBuilder.SetCustomAttribute(attributeBuilder);
        }
    }
}