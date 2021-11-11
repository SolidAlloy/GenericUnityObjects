namespace GenericUnityObjects.Editor
{
    using System;
    using Util;
    using Object = UnityEngine.Object;

    /// <summary>
    /// A class responsible for emitting and saving assemblies.
    /// </summary>
    internal static class AssemblyCreator
    {
        // AssemblyGUID is needed to name concrete classes uniquely. This is required to correctly filter types in object fields.
        public static string CreateSelectorAssembly(string assemblyName, Type genericBehaviourWithoutArgs, string assemblyGUID) =>
            BehaviourSelectorCreator.CreateSelectorAssemblyImpl(assemblyName, genericBehaviourWithoutArgs, assemblyGUID);

        public static string CreateConcreteClass<TObject>(string assemblyName, Type genericBehaviourWithArgs, string assemblyGUID)
            where TObject : Object
        {
            return ConcreteClassCreator.CreateConcreteClass<TObject>(assemblyName, genericBehaviourWithArgs, assemblyGUID);
        }

        public static string CreateMenuItems(string assemblyName, MenuItemMethod[] menuItemMethods) =>
            MenuItemsCreator.CreateMenuItemsImpl(assemblyName, menuItemMethods);
    }
}