namespace GenericScriptableObjects.Editor
{
    using System;
    using System.Reflection;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;

    public static class AssetMenuCreator
    {
        [DidReloadScripts]
        private static void OnScriptsReload()
        {
            TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<GenericScriptableObject>();
            int typesCount = types.Count;
            for (int i = 0; i < typesCount; ++i)
            {
                Type type = types[i];

                if (!type.IsGenericType)
                    continue;

                CheckInvalidName(type.Name);

                var assetMenuAttribute = type.GetCustomAttribute(typeof(CreateGenericAssetMenuAttribute));
                if (assetMenuAttribute == null)
                    continue;


            }
        }

        private static void CheckInvalidName(string typeName)
        {
            if (AssetDatabase.FindAssets(typeName).Length != 0)
                return;

            Debug.LogWarning($"Make sure a script that contains the {typeName} type is named the same way, with specifying the number of arguments at the end.\n" +
                             "It will help the plugin not lose a reference to the type should you rename it later.");
        }

        private static string CreateMenuItemMethod(CreateGenericAssetMenuAttribute attribute)
        {
            const string assetCreatePath = "Assets/Create/";

            string menuItemLine = $"[MenuItem(\"Assets/Create/{attribute.MenuName}\", priority = {attribute.Order})]";

            return menuItemLine;
        }
    }
}