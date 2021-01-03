namespace GenericUnityObjects.Editor.MonoBehaviour
{
    using System;
    using System.Configuration.Assemblies;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;
    using GenericUnityObjects.Util;
    using ScriptableObject;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Assertions;

    internal static class AssemblyCreator
    {
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
            const string concreteClassName = "ConcreteClass";

            AssemblyBuilder assemblyBuilder = GetAssemblyBuilder(assemblyName);
            ModuleBuilder moduleBuilder = GetModuleBuilder(assemblyBuilder, assemblyName);

            TypeBuilder typeBuilder = moduleBuilder.DefineType(concreteClassName, TypeAttributes.NotPublic, genericBehaviourWithArgs);

            AddComponentMenuAttribute(typeBuilder, componentName);

            Type type = typeBuilder.CreateType();

            assemblyBuilder.Save($"{assemblyName}.dll");

            return type;
        }

        public static void CreateMenuItems(string assemblyName, MenuItemMethod[] menuItemMethods)
        {
            const string menuItemsTypeName = "MenuItems";

            AssemblyBuilder assemblyBuilder = GetAssemblyBuilder(assemblyName);
            ModuleBuilder moduleBuilder = GetModuleBuilder(assemblyBuilder, assemblyName);

            TypeBuilder typeBuilder = moduleBuilder.DefineType(menuItemsTypeName, TypeAttributes.NotPublic, typeof(GenericSOCreator));

            int menuItemsLength = menuItemMethods.Length;

            for (int i = 0; i < menuItemsLength; i++)
            {
                ref MenuItemMethod menuItemMethod = ref menuItemMethods[i];
                AddMenuItemMethod(typeBuilder, menuItemMethod, i);
            }

            typeBuilder.CreateType();

            assemblyBuilder.Save($"{assemblyName}.dll");
        }

        private static void AddMenuItemMethod(TypeBuilder typeBuilder, MenuItemMethod menuItemMethod, int index)
        {
            MethodBuilder menuItemMethodBuilder = typeBuilder.DefineMethod(
                $"Method_{index}",
                MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static,
                typeof(void),
                Type.EmptyTypes);

            ILGenerator ilGenerator = menuItemMethodBuilder.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldtoken, menuItemMethod.Type);

            MethodInfo getTypeFromHandle = typeof(Type).GetMethod(
                nameof(Type.GetTypeFromHandle),
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly,
                null,
                new[] { typeof(RuntimeTypeHandle) },
                null);

            Assert.IsNotNull(getTypeFromHandle);

            ilGenerator.EmitCall(OpCodes.Call, getTypeFromHandle, null);

            ilGenerator.Emit(OpCodes.Ldstr, menuItemMethod.FileName);

            MethodInfo createAsset = typeof(GenericSOCreator).GetMethod(
                "CreateAsset",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly,
                null,
                Type.EmptyTypes,
                null);

            Assert.IsNotNull(createAsset);

            ilGenerator.EmitCall(OpCodes.Call, createAsset, new Type[] { typeof(Type), typeof(string) });

            ilGenerator.Emit(OpCodes.Nop);
            ilGenerator.Emit(OpCodes.Ret);

            SetMenuItemAttribute(menuItemMethodBuilder, menuItemMethod);
        }

        private static void SetMenuItemAttribute(MethodBuilder menuItemMethodBuilder, MenuItemMethod menuItemMethod)
        {
            ConstructorInfo classCtorInfo = typeof(MenuItem).GetConstructor(
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { typeof(string) },
                null);

            Assert.IsNotNull(classCtorInfo);

            var attributeBuilder = new CustomAttributeBuilder(classCtorInfo, new object[] { $"Assets/Create/{menuItemMethod.MenuName}", menuItemMethod.Order });

            menuItemMethodBuilder.SetCustomAttribute(attributeBuilder);
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