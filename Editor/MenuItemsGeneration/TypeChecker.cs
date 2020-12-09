namespace GenericScriptableObjects.Editor.MenuItemsGeneration
{
    using System;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Assertions;

    internal class TypeChecker
    {
        public static void CheckInvalidName(string typeName)
        {
            if (AssetDatabase.FindAssets(typeName).Length != 0)
                return;

            Debug.LogWarning($"Make sure a script that contains the {typeName} type is named the same way, with specifying the number of arguments at the end.\n" +
                             "It will help the plugin not lose a reference to the type should you rename it later.");
        }

        public static string GetGenericTypeDefinitionName(Type type)
        {
            string fullTypeName = type.FullName;
            Assert.IsNotNull(fullTypeName);
            string typeNameWithoutArguments = fullTypeName.Split('`')[0];
            int argsCount = type.GetGenericArguments().Length;
            string suffix = $"<{new string(',', argsCount-1)}>";
            return typeNameWithoutArguments + suffix;
        }
    }
}