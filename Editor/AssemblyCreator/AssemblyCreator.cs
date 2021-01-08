namespace GenericUnityObjects.Editor
{
    using System;
    using Util;
    using Object = UnityEngine.Object;

    internal static class AssemblyCreator
    {
        public static void CreateSelectorAssembly(string assemblyName, Type genericBehaviourWithoutArgs) =>
            BehaviourSelectorCreator.CreateSelectorAssemblyImpl(assemblyName, genericBehaviourWithoutArgs);

        public static void CreateConcreteClass<TObject>(string assemblyName, Type genericBehaviourWithArgs)
            where TObject : Object
        {
            ConcreteClassCreator.CreateConcreteClass<TObject>(assemblyName, genericBehaviourWithArgs);
        }

        public static void CreateMenuItems(string assemblyName, MenuItemMethod[] menuItemMethods) =>
            MenuItemsCreator.CreateMenuItemsImpl(assemblyName, menuItemMethods);
    }
}