namespace GenericUnityObjects.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using GenericUnityObjects.Util;
    using SolidUtilities;
    using SolidUtilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using Util;

    /// <summary>
    /// Gathers types that inherit from <see cref="ScriptableObject"/> and have
    /// <see cref="CreateGenericAssetMenuAttribute"/>, then generates a class with methods marked as
    /// <see cref="MenuItemMethod"/> so that they appear in the Assets/Create menu.
    /// </summary>
    public static class MenuItemsChecker
    {
        private const string AssemblyName = "GeneratedMenuItems";

        public static bool Check()
        {
            var newScriptableObjects = TypeCache.GetTypesDerivedFrom<ScriptableObject>()
                .Where(type => type.IsGenericType && ! type.IsAbstract)
                .ToArray();

            if (newScriptableObjects.Length == 0)
            {
                if (PersistentStorage.MenuItemMethods.Length == 0)
                {
                    // nothing to delete or create
                    return false;
                }

                RemoveMenuItemsAssembly();
                PersistentStorage.MenuItemMethods = new MenuItemMethod[0];
                return true;
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
            PersistentStorage.MenuItemsAssemblyPath = assemblyPath;
            string assemblyGUID = AssetDatabaseHelper.GetUniqueGUID();
            AssemblyGeneration.ImportAssemblyAsset(assemblyPath, assemblyGUID, true);
        }

        private static void RemoveMenuItemsAssembly()
        {
            string assemblyPath = PersistentStorage.MenuItemsAssemblyPath;

            if (!string.IsNullOrEmpty(assemblyPath))
                AssetDatabase.DeleteAsset(assemblyPath);
        }

        private static void UpdateMenuItemsAssembly(MenuItemMethod[] menuItemMethods)
        {
            // The name of the assembly shouldn't change, but we still use AssemblyReplace just to be save because the name can change because of a plugin update.
            string oldAssemblyPath = PersistentStorage.MenuItemsAssemblyPath;

            if (string.IsNullOrEmpty(oldAssemblyPath))
                oldAssemblyPath = $"{Config.GetAssemblyPathForType(null)}/{AssemblyName}.dll"; // The old name of the menu items assembly before it was made non-constant.

            var assemblyReplacer = AssemblyAssetOperations.StartAssemblyReplacement(oldAssemblyPath);
            string newAssemblyPath = AssemblyCreator.CreateMenuItems(AssemblyName, menuItemMethods);
            assemblyReplacer.FinishReplacement(newAssemblyPath);
            PersistentStorage.MenuItemsAssemblyPath = newAssemblyPath;
        }
    }
}