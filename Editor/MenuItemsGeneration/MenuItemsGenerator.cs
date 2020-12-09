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
        public static readonly string FilePath = $"{Application.dataPath}/{Folders}/MenuItems.cs";

        private const string NewLine = "\r\n";
        private const string RawNewLine = @"\r\n";
        private const string Folders = "Resources/Editor";

        private static string _oldContent;

        public static void GenerateClass(MenuItemMethod[] newMethods)
        {
            string classContent = GetClassContent();

            var oldMethods = GenericSOPersistentStorage.MenuItemMethods;

            // Stop if the PersistentStorage asset was not found. It frequently happens on Unity Editor launch, leading
            // to the false assumption the MenuItemMethods collection is empty.
            if (oldMethods == null)
                return;

            var oldMethodsSet = new HashSet<MenuItemMethod>(GenericSOPersistentStorage.MenuItemMethods, MenuItemMethod.Comparer);
            var newMethodsSet = new HashSet<MenuItemMethod>(newMethods, MenuItemMethod.Comparer);

            if (oldMethodsSet.SetEquals(newMethodsSet))
                return;

            classContent = RemoveOldMethods(classContent, oldMethodsSet, newMethodsSet);
            classContent = AddNewMethods(classContent, oldMethodsSet, newMethodsSet);

            GenericSOPersistentStorage.MenuItemMethods = newMethods;

            SaveToFile(classContent);
        }

        public static void RemoveMethod(string typeName)
        {
            string classContent = GetClassContent();
            classContent = RemoveMethod(classContent, typeName);
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

        private static string RemoveOldMethods(string classContent, HashSet<MenuItemMethod> oldMethodsSet, HashSet<MenuItemMethod> newMethodsSet)
        {
            var methodsToRemove = oldMethodsSet.ExceptWith(newMethodsSet, MenuItemMethod.Comparer);

            foreach (MenuItemMethod method in methodsToRemove)
            {
                classContent = RemoveMethod(classContent, method.TypeName);
            }

            return classContent;
        }

        private static string RemoveMethod(string classContent, string typeName)
        {
            var regex = new Regex($@"\[MenuItem.*?{RawNewLine}.*?{typeName}.*?{RawNewLine}{RawNewLine}");
            classContent = regex.Replace(classContent, string.Empty, 1);
            return classContent;
        }

        private static string GetClassContent()
        {
            if (File.Exists(FilePath))
            {
                _oldContent = File.ReadAllText(FilePath);
                return _oldContent;
            }

            string emptyClass = $"namespace GenericScriptableObjects.Editor.AssetCreation{NewLine}" +
                                $"{{{NewLine}" +
                                $"using UnityEditor;{NewLine}" +
                                $"internal class MenuItems : GenericSOCreator{NewLine}" +
                                $"{{{NewLine}" +
                                $"}}{NewLine}" +
                                $"}}";

            AssetDatabaseHelper.MakeSureFolderExists(Folders);
            File.WriteAllText(FilePath, emptyClass);
            return emptyClass;
        }

        private static string CreateMenuItemMethod(MenuItemMethod method)
        {
            string fileName = method.FileName == string.Empty ? $"New {method.Type.Name}" : method.FileName;
            string menuName = method.MenuName == string.Empty ? GetGenericTypeName(method.Type) : method.MenuName;

            string attributeLine = $"[MenuItem(\"Assets/Create/{menuName}\", priority = {method.Order})]";
            string typeName = TypeChecker.GetGenericTypeDefinitionName(method.Type);

            string methodLine = $"private static void Create{method.TypeName}() => CreateAsset(typeof({typeName}), " +
                                $"\"{method.NamespaceName}\", \"{method.ScriptsPath}\", \"{fileName}\");";

            return $"{attributeLine}{NewLine}{methodLine}{NewLine}{NewLine}";
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

            File.WriteAllText(FilePath, classContent);
            AssetDatabase.Refresh();
        }
    }
}