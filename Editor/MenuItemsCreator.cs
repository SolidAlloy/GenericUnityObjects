namespace GenericScriptableObjects.Editor
{
    using System;
    using System.IO;
    using System.Reflection;
    using NUnit.Framework;
    using SolidUtilities.Editor.Helpers;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;

    public static class MenuItemsCreator
    {
        // [DidReloadScripts]
        private static void OnScriptsReload()
        {
            CreateMenuItemsClassIfMissing();

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

        private static string CreateMenuItemMethod(CreateGenericAssetMenuAttribute attribute, Type type)
        {
            const string assetCreatePath = "Assets/Create/";

            string attributeLine = $"[MenuItem(\"Assets/Create/{attribute.MenuName}\", priority = {attribute.Order})]";
            string typeFullName = type.FullName;
            Assert.IsNotNull(typeFullName);
            string classSafeName = AssetCreatorHelper.GetClassSafeTypeName(typeFullName);
            string typeName = GetCorrectTypeName(type, typeFullName);
            string methodLine = $"private static void Create{classSafeName}() => CreateAsset(typeof({typeName}), {attribute.NamespaceName}, {attribute.ScriptsPath});";
            return $"{attributeLine}\n{methodLine}";
        }

        private static string GetCorrectTypeName(Type type, string typeName)
        {
            string typeNameWithoutArguments = typeName.Split('`')[0];
            int argsCount = type.GetGenericArguments().Length;
            string suffix = $"<{new string(',', argsCount-1)}>";
            return typeNameWithoutArguments + suffix;
        }

        private static void CreateMenuItemsClassIfMissing()
        {
            const string folders = "Resources/Editor";

            string filePath = $"{Application.dataPath}/{folders}/MenuItems.cs";

            if (File.Exists(filePath))
                return;

            AssetDatabaseHelper.MakeSureFolderExists(folders);

            const string emptyClass =
                "namespace GenericScriptableObjects.Editor\n{\nusing UnityEditor;\ninternal class MenuItems : GenericSOCreator\n{\n}\n}";

            File.WriteAllText(filePath, emptyClass);
        }
    }
}