namespace GenericUnityObjects.Editor
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using ScriptableObjects;
    using UnityEditor;
    using UnityEngine.Assertions;
    using Util;

    /// <summary>
    /// Emits and saves an assembly that represents a class derived from <see cref="GenericSOCreator"/> and containing
    /// a number of methods marked with <see cref="SetMenuItemAttribute"/>. Each method represents a generic
    /// ScriptableObject that can be created using the Assets/Create menu.
    /// </summary>
    internal static class MenuItemsCreator
    {
        private static MethodInfo _createAsset;
        private static FieldInfo _priorityField;
        private static ConstructorInfo _menuItemConstructor;

        private static MethodInfo CreateAsset
        {
            get
            {
                if (_createAsset == null)
                {
                    _createAsset = typeof(GenericSOCreator).GetMethod(
                        "CreateAsset",
                        BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly,
                        null,
                        new[] { typeof(Type), typeof(string) },
                        null);
                }

                return _createAsset;
            }
        }

        private static FieldInfo PriorityField
        {
            get
            {
                if (_priorityField == null)
                {
                    _priorityField = typeof(MenuItem).GetField(
                        nameof(MenuItem.priority),
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                    Assert.IsNotNull(_priorityField);
                }

                return _priorityField;
            }
        }

        private static ConstructorInfo MenuItemConstructor
        {
            get
            {
                if (_menuItemConstructor == null)
                {
                    _menuItemConstructor = typeof(MenuItem).GetConstructor(
                        BindingFlags.Public | BindingFlags.Instance,
                        null,
                        new[] { typeof(string) },
                        null);
                }

                return _menuItemConstructor;
            }
        }

        public static string CreateMenuItemsImpl(string assemblyName, MenuItemMethod[] menuItemMethods)
        {
            const string menuItemsTypeName = "MenuItems";

            using var concreteClassAssembly = AssemblyCreatorHelper.CreateConcreteClassAssembly(assemblyName, menuItemsTypeName, typeof(GenericSOCreator));

            int menuItemsLength = menuItemMethods.Length;

            for (int i = 0; i < menuItemsLength; i++)
            {
                AddMenuItemMethod(concreteClassAssembly.TypeBuilder, menuItemMethods[i], i);
            }

            return concreteClassAssembly.Path;
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
            ilGenerator.EmitCall(OpCodes.Call, AssemblyCreatorHelper.GetTypeFromHandle, null);
            ilGenerator.Emit(OpCodes.Ldstr, menuItemMethod.FileName);
            ilGenerator.EmitCall(OpCodes.Call, CreateAsset, new[] { typeof(Type), typeof(string) });
            ilGenerator.Emit(OpCodes.Nop);
            ilGenerator.Emit(OpCodes.Ret);

            SetMenuItemAttribute(menuItemMethodBuilder, menuItemMethod);
        }

        private static void SetMenuItemAttribute(MethodBuilder menuItemMethodBuilder, MenuItemMethod menuItemMethod)
        {
            var attributeBuilder = new CustomAttributeBuilder(
                MenuItemConstructor, new object[] { $"Assets/Create/{menuItemMethod.MenuName}" },
                new PropertyInfo[0], new object[0],
                new[] { PriorityField }, new object[] { menuItemMethod.Order });

            menuItemMethodBuilder.SetCustomAttribute(attributeBuilder);
        }
    }
}