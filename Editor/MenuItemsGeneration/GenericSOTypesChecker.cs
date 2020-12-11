namespace GenericScriptableObjects.Editor.MenuItemsGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AssetCreation;
    using SolidUtilities.Helpers;
    using UnityEditor;
    using UnityEditor.Callbacks;

    /// <summary>
    /// This class verifies that the GenericScriptableObject scripts are named appropriately and regenerates the
    /// MenuItem methods so that assets can be created from the context menu.
    /// </summary>
    internal static class GenericSOTypesChecker
    {
        [DidReloadScripts]
        private static void OnScriptsReload()
        {
            var types = TypeCache.GetTypesDerivedFrom<GenericScriptableObject>().Where(type => type.IsGenericType).ToArray();
            int typesCount = types.Length;

            var menuItemMethods = new List<MenuItemMethod>();

            for (int i = 0; i < typesCount; ++i)
            {
                Type type = types[i];

                CreatorUtil.CheckInvalidName(type.Name);

                var assetMenuAttribute = type.GetCustomAttribute<CreateGenericAssetMenuAttribute>();
                if (assetMenuAttribute == null)
                    continue;

                menuItemMethods.Add(new MenuItemMethod
                {
                    TypeName = type.FullName.MakeClassFriendly(),
                    FileName = assetMenuAttribute.FileName,
                    MenuName = assetMenuAttribute.MenuName,
                    Order = assetMenuAttribute.Order,
                    Type = type
                });
            }

            MenuItemsGenerator.GenerateClass(menuItemMethods.ToArray());
        }
    }
}