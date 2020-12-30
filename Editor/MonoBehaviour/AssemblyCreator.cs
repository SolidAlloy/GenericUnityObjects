namespace GenericUnityObjects.Editor.MonoBehaviour
{
    using System;
    using System.Configuration.Assemblies;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;
    using GenericUnityObjects.Util;
    using UnityEngine;
    using UnityEngine.Assertions;

    public static class AssemblyCreator
    {
        public const string ConcreteClassName = "ConcreteClass";

        public static void CreateSelectorAssembly(string assemblyName, Type genericBehaviourWithoutArgs, string componentName)
        {
            const string className = "ClassSelector";

            AssemblyBuilder assemblyBuilder = GetAssemblyBuilder(assemblyName);
            ModuleBuilder moduleBuilder = GetModuleBuilder(assemblyBuilder, assemblyName);

            TypeBuilder typeBuilder = moduleBuilder.DefineType(className, TypeAttributes.NotPublic, typeof(BehaviourSelector));

            CreateBehaviourTypeProperty(typeBuilder, genericBehaviourWithoutArgs);
            AddComponentMenuAttribute(typeBuilder, componentName);

            typeBuilder.CreateType();

            assemblyBuilder.Save($"{assemblyName}.dll");
        }

        public static Type CreateConcreteClass(string assemblyName, Type genericBehaviourWithArgs, string componentName)
        {
            AssemblyBuilder assemblyBuilder = GetAssemblyBuilder(assemblyName);
            ModuleBuilder moduleBuilder = GetModuleBuilder(assemblyBuilder, assemblyName);

            TypeBuilder typeBuilder = moduleBuilder.DefineType(ConcreteClassName, TypeAttributes.NotPublic, genericBehaviourWithArgs);

            AddComponentMenuAttribute(typeBuilder, componentName);

            Type type = typeBuilder.CreateType();

            assemblyBuilder.Save($"{assemblyName}.dll");

            return type;
        }

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
            ConstructorInfo classCtorInfo = typeof(AddComponentMenu).GetConstructor(
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { typeof(string) },
                null);

            Assert.IsNotNull(classCtorInfo);

            var attributeBuilder = new CustomAttributeBuilder(classCtorInfo, new object[] { componentName });

            typeBuilder.SetCustomAttribute(attributeBuilder);
        }

        private static void CreateBehaviourTypeProperty(TypeBuilder typeBuilder, Type propertyValue)
        {
            PropertyBuilder property = typeBuilder.DefineProperty(
                "GenericBehaviourType",
                PropertyAttributes.None,
                typeof(Type),
                null);

            MethodBuilder pGet = typeBuilder.DefineMethod(
                "get_GenericBehaviourType",
                MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                typeof(Type),
                Type.EmptyTypes);

            ILGenerator pILGet = pGet.GetILGenerator();

            pILGet.Emit(OpCodes.Ldtoken, propertyValue);

            MethodInfo getTypeFromHandle = typeof(Type).GetMethod(
                nameof(Type.GetTypeFromHandle),
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly,
                null,
                new[] { typeof(RuntimeTypeHandle) },
                null);

            Assert.IsNotNull(getTypeFromHandle);

            pILGet.EmitCall(OpCodes.Call, getTypeFromHandle, null);
            pILGet.Emit(OpCodes.Ret);

            property.SetGetMethod(pGet);
        }
    }
}