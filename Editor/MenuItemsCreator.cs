namespace GenericScriptableObjects.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;

    public static class MenuItemsCreator
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
                    TypeName = AssetCreatorHelper.GetClassSafeTypeName(type.FullName),
                    FileName = assetMenuAttribute.FileName,
                    MenuName = assetMenuAttribute.MenuName,
                    NamespaceName = assetMenuAttribute.NamespaceName,
                    ScriptsPath = assetMenuAttribute.ScriptsPath,
                    Order = assetMenuAttribute.Order,
                    Type = type
                });
            }

            ClassContentsGenerator.GenerateClass(menuItemMethods.ToArray());
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