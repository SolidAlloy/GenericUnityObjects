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

    /// <summary>
    /// Gathers types that inherit from <see cref="GenericScriptableObject"/> and have
    /// <see cref="CreateGenericAssetMenuAttribute"/>, then generates a class with methods marked as
    /// <see cref="MenuItemMethod"/> so that they appear in the Assets/Create menu.
    /// </summary>
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
            string assemblyPath = AssemblyCreator.CreateMenuItems(AssemblyName, menuItemMethods);
            string assemblyGUID = AssemblyGeneration.GetUniqueGUID();
            AssemblyGeneration.ImportAssemblyAsset(assemblyPath, assemblyGUID, true);
        }

        private static void RemoveMenuItemsAssembly()
        {
            AssetDatabase.DeleteAsset($"{Config.AssembliesDirPath}/{AssemblyName}.dll");
        }

        private static void UpdateMenuItemsAssembly(MenuItemMethod[] menuItemMethods)
        {
            // No need to update it using AssemblyReplacer because the name doesn't change.
            AssemblyCreator.CreateMenuItems(AssemblyName, menuItemMethods);
        }
    }
}