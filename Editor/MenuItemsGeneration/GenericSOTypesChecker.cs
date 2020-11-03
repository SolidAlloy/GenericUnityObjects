namespace GenericScriptableObjects.Editor.MenuItemsGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;
    using Util;

    /// <summary>
    /// This class verifies that the GenericScriptableObject scripts are named appropriately and regenerates the
    /// MenuItem methods so that assets can be created from the context menu.
    /// </summary>
    internal static class GenericSOTypesChecker
    {
        [DidReloadScripts]
        public static void OnScriptsReload()
        {
            var types = TypeCache.GetTypesDerivedFrom<GenericScriptableObject>().Where(type => type.IsGenericType).ToArray();
            int typesCount = types.Length;

            var menuItemMethods = new List<MenuItemMethod>();

            for (int i = 0; i < typesCount; ++i)
            {
                Type type = types[i];

                CheckInvalidName(type.Name);

                var assetMenuAttribute = type.GetCustomAttribute<CreateGenericAssetMenuAttribute>();
                if (assetMenuAttribute == null)
                    continue;

                menuItemMethods.Add(new MenuItemMethod
                {
                    TypeName = GenericSOUtil.GetClassSafeTypeName(type.FullName),
                    FileName = assetMenuAttribute.FileName,
                    MenuName = assetMenuAttribute.MenuName,
                    NamespaceName = assetMenuAttribute.NamespaceName,
                    ScriptsPath = assetMenuAttribute.ScriptsPath,
                    Order = assetMenuAttribute.Order,
                    Type = type
                });
            }

            MenuItemsGenerator.GenerateClass(menuItemMethods.ToArray());
        }

        private static void CheckInvalidName(string typeName)
        {
            if (AssetDatabase.FindAssets(typeName).Length != 0)
                return;

            Debug.LogWarning($"Make sure a script that contains the {typeName} type is named the same way, with specifying the number of arguments at the end.\n" +
                             "It will help the plugin not lose a reference to the type should you rename it later.");
        }
    }
}