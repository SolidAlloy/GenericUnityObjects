namespace GenericUnityObjects.Editor
{
    using System;
    using Util;

    internal static class AssemblyCreator
    {
        public static void CreateSelectorAssembly(string assemblyName, Type genericBehaviourWithoutArgs) =>
            BehaviourSelectorCreator.CreateSelectorAssemblyImpl(assemblyName, genericBehaviourWithoutArgs);

        public static void CreateBehaviourConcreteClass(string assemblyName, Type genericBehaviourWithArgs) =>
            ConcreteClassCreator.CreateBehaviourConcreteClass(assemblyName, genericBehaviourWithArgs);

        public static void CreateSOConcreteClass(string assemblyName, Type genericBehaviourWithArgs) =>
            ConcreteClassCreator.CreateSOConcreteClass(assemblyName, genericBehaviourWithArgs);

        public static void CreateMenuItems(string assemblyName, MenuItemMethod[] menuItemMethods) =>
            MenuItemsCreator.CreateMenuItemsImpl(assemblyName, menuItemMethods);
    }
}