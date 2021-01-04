namespace GenericUnityObjects.Editor.MonoBehaviour
{
    using System;
    using System.Collections.Generic;
    using System.Configuration.Assemblies;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;
    using GenericUnityObjects.Util;
    using ScriptableObject;
    using UnityEngine;
    using UnityEngine.Assertions;

    internal static partial class AssemblyCreator
    {
        private static ConstructorInfo _addComponentMenuConstructor;
        private static MethodInfo _getTypeFromHandle;

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
            BehaviourCreator.CreateSelectorAssemblyImpl(assemblyName, genericBehaviourWithoutArgs, componentName);

        public static Type CreateConcreteClass(string assemblyName, Type genericBehaviourWithoutArgs, string componentName) =>
            ConcreteClassCreator.CreateConcreteClassImpl(assemblyName, genericBehaviourWithoutArgs, componentName);

        public static void CreateMenuItems(string assemblyName, MenuItemMethod[] menuItemMethods) =>
            MenuItemsCreator.CreateMenuItemsImpl(assemblyName, menuItemMethods);

        private static AssemblyBuilder GetAssemblyBuilder(string assemblyName)
        {
            return AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName(assemblyName)
                {
                    CultureInfo = CultureInfo.InvariantCulture,
                    Flags = AssemblyNameFlags.None,
                    ProcessorArchitecture = ProcessorArchitecture.MSIL,
                    VersionCompatibility = AssemblyVersionCompatibility.SameDomain
                },
                AssemblyBuilderAccess.RunAndSave, Config.AssembliesDirPath);
        }

        private static ModuleBuilder GetModuleBuilder(AssemblyBuilder assemblyBuilder, string assemblyName) =>
            assemblyBuilder.DefineDynamicModule($"{assemblyName}.dll", true);

        private static void AddComponentMenuAttribute(TypeBuilder typeBuilder, string componentName)
        {
            var attributeBuilder = new CustomAttributeBuilder(AddComponentMenuConstructor, new object[] { componentName });
            typeBuilder.SetCustomAttribute(attributeBuilder);
        }
    }
}