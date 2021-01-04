namespace GenericUnityObjects.Editor.MonoBehaviour
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using ScriptableObject;
    using UnityEditor;
    using UnityEngine.Assertions;

    internal static partial class AssemblyCreator
    {
        private static class MenuItemsCreator
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
                            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly,
                            null,
                            Type.EmptyTypes,
                            null);

                        Assert.IsNotNull(_createAsset);
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

                        Assert.IsNotNull(_menuItemConstructor);
                    }

                    return _menuItemConstructor;
                }
            }
            
            public static void CreateMenuItemsImpl(MenuItemMethod[] menuItemMethods)
            {
                const string assemblyName = "GeneratedMenuItems";
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
                ilGenerator.EmitCall(OpCodes.Call, GetTypeFromHandle, null);
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
}