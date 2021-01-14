namespace GenericUnityObjects.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using GenericUnityObjects.Util;
    using SolidUtilities.Extensions;
    using UnityEditor;
    using Util;

    public static class MenuItemsChecker
    {
        private const string AssemblyName = Config.MenuItemsAssemblyName;

        public static bool Check()
        {
            var newScriptableObjects = TypeCache.GetTypesDerivedFrom<GenericScriptableObject>()
                .Where(type => type.IsGenericType && ! type.IsAbstract)
                .ToArray();

            if (newScriptableObjects.Length == 0)
            {
                if (PersistentStorage.MenuItemMethods.Length == 0)
                {
                    // nothing to delete or create
                    return false;
                }
                else
                {
                    RemoveMenuItemsAssembly();
                    PersistentStorage.MenuItemMethods = new MenuItemMethod[0];
                    return true;
                }
            }

            var newMenuItemMethods = GetMenuItemMethods(newScriptableObjects);

            if (newMenuItemMethods.Length == 0)
            {
                RemoveMenuItemsAssembly();
                PersistentStorage.MenuItemMethods = new MenuItemMethod[0];
                return true;
            }

            if (PersistentStorage.MenuItemMethods.Length == 0)
            {
                CreateMenuItemsAssembly(newMenuItemMethods);
                PersistentStorage.MenuItemMethods = newMenuItemMethods;
                return true;
            }

            var oldTypesSet = new HashSet<MenuItemMethod>(PersistentStorage.MenuItemMethods);

            if (oldTypesSet.SetEqualsArray(newMenuItemMethods))
                return false;

            UpdateMenuItemsAssembly(newMenuItemMethods);
            PersistentStorage.MenuItemMethods = newMenuItemMethods;
            return true;
        }

        private static MenuItemMethod[] GetMenuItemMethods(Type[] scriptableObjects)
        {
            int typesCount = scriptableObjects.Length;
            var newMenuItemMethodsList = new List<MenuItemMethod>(typesCount);

            for (int i = 0; i < typesCount; ++i)
            {
                Type type = scriptableObjects[i];

                var assetMenuAttribute = type.GetCustomAttribute<CreateGenericAssetMenuAttribute>();
                if (assetMenuAttribute == null)
                    continue;

                newMenuItemMethodsList.Add(new MenuItemMethod(
                    assetMenuAttribute.FileName,
                    assetMenuAttribute.MenuName,
                    assetMenuAttribute.Order,
                    type));
            }

            return newMenuItemMethodsList.ToArray();
        }

        private static void CreateMenuItemsAssembly(MenuItemMethod[] menuItemMethods)
        {
            AssemblyCreator.CreateMenuItems(AssemblyName, menuItemMethods);
            string assemblyPath = $"{Config.AssembliesDirPath}/{AssemblyName}.dll";
            AssemblyGeneration.ImportAssemblyAsset(assemblyPath, true);
        }

        private static void RemoveMenuItemsAssembly()
        {
            string assemblyPath = $"{Config.AssembliesDirPath}/{AssemblyName}.dll";
            AssemblyAssetOperations.RemoveAssemblyByPath(assemblyPath);
        }

        private static void UpdateMenuItemsAssembly(MenuItemMethod[] menuItemMethods)
        {
            string assemblyPath = $"{Config.AssembliesDirPath}/{AssemblyName}.dll";

            using (AssemblyAssetOperations.AssemblyReplacer.UsingPath(assemblyPath, AssemblyName))
            {
                AssemblyCreator.CreateMenuItems(AssemblyName, menuItemMethods);
            }
        }
    }
}