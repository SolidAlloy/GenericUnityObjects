namespace GenericScriptableObjects.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    internal static class MenuItemsCreator
    {
        // [DidReloadScripts]
        private static void OnScriptsReload()
        {
            var types = TypeCache.GetTypesDerivedFrom<GenericScriptableObject>().Where(type => type.IsGenericType).ToArray();
            int typesCount = types.Length;

            var newTypeSet = new HashSet<string>();
            var typeDict = new Dictionary<string, KeyValuePair<Type, CreateGenericAssetMenuAttribute>>();

            for (int i = 0; i < typesCount; ++i)
            {
                Type type = types[i];

                CheckInvalidName(type.Name);

                var assetMenuAttribute = type.GetCustomAttribute<CreateGenericAssetMenuAttribute>();
                if (assetMenuAttribute == null)
                    continue;

                string classSafeName = AssetCreatorHelper.GetClassSafeTypeName(type.FullName);
                newTypeSet.Add(classSafeName);
                typeDict[classSafeName] = new KeyValuePair<Type, CreateGenericAssetMenuAttribute>(type, assetMenuAttribute);
            }

            ClassContentsGenerator.GenerateClass(newTypeSet, typeDict);
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