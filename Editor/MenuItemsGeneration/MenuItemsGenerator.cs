namespace GenericScriptableObjects.Editor.MenuItemsGeneration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using NUnit.Framework;
    using SolidUtilities.Editor.Helpers;
    using SolidUtilities.Extensions;
    using UnityEditor;
    using UnityEngine;
    using Util;

    /// <summary>
    /// A class responsible for generating or updating a class that contains all the MenuItem methods needed for
    /// the asset creation.
    /// </summary>
    internal static class MenuItemsGenerator
    {
        private const string Folders = "Resources/Editor";
        public static readonly string FilePath = $"{Application.dataPath}/{Folders}/MenuItems.cs";
        private static string _oldContent;

        public static void GenerateClass(List<MenuItemMethod> newMethods)
        {
            string classContent = GetClassContent();

            var oldMethodsSet = new HashSet<MenuItemMethod>(GenericSOPersistentStorage.MenuItemMethods, MenuItemMethod.Comparer);
            var newMethodsSet = new HashSet<MenuItemMethod>(newMethods, MenuItemMethod.Comparer);

            if (oldMethodsSet.SetEquals(newMethodsSet))
                return;

            classContent = AddNewMethods(classContent, oldMethodsSet, newMethodsSet);

            GenericSOPersistentStorage.MenuItemMethods = newMethods;

            SaveToFile(classContent);
        }

        public static void RemoveMethod(string typeName)
        {
            string classContent = GetClassContent();
            var regex = new Regex($@"\[MenuItem.*?\n.*?{typeName}.*?\n\n");
            classContent = regex.Replace(classContent, string.Empty, 1);
            SaveToFile(classContent);
        }

        private static string AddNewMethods(string classContent, HashSet<MenuItemMethod> oldMethodsSet,
            HashSet<MenuItemMethod> newMethodsSet)
        {
            var methodsToAdd = newMethodsSet.ExceptWith(oldMethodsSet, MenuItemMethod.Comparer);

            if (methodsToAdd.Count == 0)
                return classContent;

            string newMethods = string.Empty;

            foreach (MenuItemMethod method in methodsToAdd)
            {
                newMethods += CreateMenuItemMethod(method);
            }

            int insertPos = classContent.Length - 4;
            return classContent.Insert(insertPos, newMethods);
        }

        private static string GetClassContent()
        {
            const string emptyClass =
                "namespace GenericScriptableObjects.Editor.AssetCreation\n{\nusing UnityEditor;\ninternal class MenuItems : GenericSOCreator\n{\n}\n}";

            if (!File.Exists(FilePath))
                return emptyClass;

            _oldContent = File.ReadAllText(FilePath);
            return _oldContent;
        }

        private static string CreateMenuItemMethod(MenuItemMethod method)
        {
            string fileName = method.FileName == string.Empty ? $"New {method.TypeName}" : method.FileName;
            string menuName = method.MenuName == string.Empty ? GetGenericTypeName(method.Type) : method.MenuName;

            string attributeLine = $"[MenuItem(\"Assets/Create/{menuName}\", priority = {method.Order})]";
            string typeName = GetGenericTypeDefinitionName(method.Type);
            string methodLine = $"private static void Create{method.TypeName}() => CreateAsset(typeof({typeName}), \"{method.NamespaceName}\", \"{method.ScriptsPath}\", \"{fileName}\");";
            return $"{attributeLine}\n{methodLine}\n\n";
        }

        private static string GetGenericTypeDefinitionName(Type type)
        {
            string fullTypeName = type.FullName;
            Assert.IsNotNull(fullTypeName);
            string typeNameWithoutArguments = fullTypeName.Split('`')[0];
            int argsCount = type.GetGenericArguments().Length;
            string suffix = $"<{new string(',', argsCount-1)}>";
            return typeNameWithoutArguments + suffix;
        }

        private static string GetGenericTypeName(Type type)
        {
            string typeNameWithoutArguments = type.Name.Split('`')[0];
            var argumentNames = type.GetGenericArguments().Select(argument => argument.Name);
            return $"{typeNameWithoutArguments}<{string.Join(",", argumentNames)}>";
        }

        private static void SaveToFile(string classContent)
        {
            if (_oldContent != null && _oldContent == classContent)
                return;

            AssetDatabaseHelper.MakeSureFolderExists(Folders);
            File.WriteAllText(FilePath, classContent);
            AssetDatabase.Refresh();
        }
    }
}