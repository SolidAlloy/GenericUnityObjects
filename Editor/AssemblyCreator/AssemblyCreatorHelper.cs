namespace GenericUnityObjects.Editor
{
    using System;
    using System.Configuration.Assemblies;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Reflection.Emit;
    using GenericUnityObjects.Util;
    using UnityEngine;

    /// <summary>
    /// A storage for methods and properties used by multiple AssemblyCreator-related classes.
    /// </summary>
    internal static class AssemblyCreatorHelper
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
                        new[] { typeof(string), typeof(int) },
                        null);
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
                }

                return _getTypeFromHandle;
            }
        }

        public static AssemblyBuilder GetAssemblyBuilder(string assemblyDir, string generatedAssemblyName)
        {
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName(generatedAssemblyName)
                {
                    CultureInfo = CultureInfo.InvariantCulture,
                    Flags = AssemblyNameFlags.None,
                    ProcessorArchitecture = ProcessorArchitecture.MSIL,
                    VersionCompatibility = AssemblyVersionCompatibility.SameDomain
                },
                AssemblyBuilderAccess.RunAndSave, assemblyDir);

            return assemblyBuilder;
        }

        public static ModuleBuilder GetModuleBuilder(AssemblyBuilder assemblyBuilder, string generatedAssemblyName)
        {
            return assemblyBuilder.DefineDynamicModule($"{generatedAssemblyName}.dll", false);
        }

        public static void AddComponentMenuAttribute(TypeBuilder typeBuilder, Type genericType)
        {
            (string componentName, int order) = GetComponentMenu(genericType);
            var attributeBuilder = new CustomAttributeBuilder(AddComponentMenuConstructor, new object[] { componentName, order });
            typeBuilder.SetCustomAttribute(attributeBuilder);
        }

        public static void AddChildrenAttributes(TypeBuilder typeBuilder, Type genericType)
        {
            var childrenAttributes = genericType.GetCustomAttribute<ApplyToChildrenAttribute>();

            if (childrenAttributes == null)
                return;

            foreach (Type attributeType in childrenAttributes.Attributes)
            {
                var constructor = attributeType.GetConstructor(Type.EmptyTypes);

                if (constructor == null)
                    Debug.LogWarning($"Tried to add an attribute to a concrete class through the ApplyToChildren attribute, but the attribute does not have a default constructor: {attributeType}");

                var attributeBuilder = new CustomAttributeBuilder(constructor, Array.Empty<object>());
                typeBuilder.SetCustomAttribute(attributeBuilder);
            }
        }

        public static (string componentName, int order) GetComponentMenu(Type type)
        {
            string componentName;
            int order;

            // BehaviourInfo already contains values of AddComponentMenu, but passing those values here would require
            // changing a lot of parameters in other methods, so we are okay with fetching them again.
            var addComponentAttr = type.GetCustomAttribute<AddComponentMenu>();

            if (addComponentAttr == null)
            {
                componentName = $"Scripts/{TypeUtility.GetNiceNameOfGenericType(type, true)}";
                order = 0;
                return (componentName, order);
            }

            componentName = addComponentAttr.componentMenu;
            order = addComponentAttr.componentOrder;

            if ( ! componentName.StartsWith("Scripts/"))
            {
                componentName = $"Scripts/{componentName}";
            }

            if ( ! componentName.Contains("<"))
            {
                componentName = $"{componentName}{TypeUtility.GetNiceArgsOfGenericType(type)}";
            }

            return (componentName, order);
        }

        public static ConcreteTypeAssembly CreateConcreteClassAssembly(string assemblyName, string className, Type parentType)
        {
            string dirPath = Config.GetAssemblyPathForType(parentType);
            return new ConcreteTypeAssembly(dirPath, assemblyName, className, parentType);
        }

        public readonly struct ConcreteTypeAssembly : IDisposable
        {
            public readonly TypeBuilder TypeBuilder;
            private readonly AssemblyBuilder _assemblyBuilder;
            private readonly string _dllName;

            public readonly string Path;

            public ConcreteTypeAssembly(string dirPath, string assemblyName, string className, Type parentType)
            {
                assemblyName = $"z_{assemblyName}"; // We prefix assemblies with z_ to keep them at the bottom of dropdowns where the DLL files are listed.
                _dllName = $"{assemblyName}.dll";
                Path = System.IO.Path.Combine(dirPath, _dllName);

                _assemblyBuilder = GetAssemblyBuilder(dirPath, assemblyName);
                ModuleBuilder moduleBuilder = GetModuleBuilder(_assemblyBuilder, assemblyName);
                TypeBuilder = moduleBuilder.DefineType(className, TypeAttributes.NotPublic, parentType);
            }

            public void Dispose()
            {
                TypeBuilder.CreateType();
                _assemblyBuilder.Save(_dllName);
            }
        }
    }
}