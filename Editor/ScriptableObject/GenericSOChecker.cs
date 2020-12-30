namespace GenericUnityObjects.Editor.ScriptableObject
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using GenericUnityObjects;
    using SolidUtilities.Helpers;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using Util;

    /// <summary>
    /// This class verifies that the GenericScriptableObject scripts are named appropriately and regenerates the
    /// MenuItem methods so that assets can be created from the context menu.
    /// </summary>
    internal static class GenericSOChecker
    {
        [DidReloadScripts]
        private static void OnScriptsReload()
        {
            var types = TypeCache.GetTypesDerivedFrom<GenericScriptableObject>()
                .Where(type => type.IsGenericType)
                .ToArray();

            int typesCount = types.Length;

            var menuItemMethods = new List<MenuItemMethod>();

            for (int i = 0; i < typesCount; ++i)
            {
                Type type = types[i];

                CreatorUtil.CheckInvalidName(type.Name);

                var assetMenuAttribute = type.GetCustomAttribute<CreateGenericAssetMenuAttribute>();
                if (assetMenuAttribute == null)
                    continue;

                menuItemMethods.Add(new MenuItemMethod(
                    type.FullName.MakeClassFriendly(),
                    assetMenuAttribute.FileName,
                    assetMenuAttribute.MenuName,
                    assetMenuAttribute.Order,
                    type));
            }

            MenuItemsGenerator.GenerateClass(menuItemMethods.ToArray());
        }
    }
}