namespace GenericUnityObjects.Editor.ScriptableObject
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using SolidUtilities.Extensions;
    using UnityEditor;
    using Util;

    /// <summary>
    /// A class responsible for generating or updating a class that contains all the MenuItem methods needed for
    /// the asset creation.
    /// </summary>
    internal static class MenuItemsGenerator
    {
        private const string NewLine = Config.NewLine;
        private const string RawNewLine = Config.RawNewLine;
        private const string Tab = Config.Tab;

        private static string _oldContent;

        public static void GenerateClass(MenuItemMethod[] newMethods)
        {
            string classContent = GetClassContent();

            var oldMethods = PersistentStorage.MenuItemMethods;

            // Stop if the PersistentStorage asset was not found. It frequently happens on Unity Editor launch, leading
            // to the false assumption the MenuItemMethods collection is empty.
            if (oldMethods == null)
                return;

            var oldMethodsSet = new HashSet<MenuItemMethod>(PersistentStorage.MenuItemMethods, MenuItemMethod.Comparer);
            var newMethodsSet = new HashSet<MenuItemMethod>(newMethods, MenuItemMethod.Comparer);

            if (oldMethodsSet.SetEquals(newMethodsSet))
                return;

            classContent = RemoveOldMethods(classContent, oldMethodsSet, newMethodsSet);
            classContent = AddNewMethods(classContent, oldMethodsSet, newMethodsSet);

            PersistentStorage.MenuItemMethods = newMethods;

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
            if (File.Exists(Config.MenuItemsPath))
            {
                _oldContent = File.ReadAllText(Config.MenuItemsPath);
                return _oldContent;
            }

            string emptyClass = $"namespace GenericUnityObjects.Editor.ScriptableObject{NewLine}" +
                                $"{{{NewLine}" +
                                $"{Tab}using UnityEditor;{NewLine}{NewLine}" +
                                $"{Tab}internal class MenuItems : GenericSOCreator{NewLine}" +
                                $"{Tab}{{{NewLine}" +
                                $"{Tab}}}{NewLine}" +
                                $"}}";

            CreatorUtil.WriteAllText(Config.MenuItemsPath, emptyClass);
            return emptyClass;
        }

        private static string CreateMenuItemMethod(MenuItemMethod method)
        {
            string fileName = method.FileName == string.Empty ? $"New {method.Type.Name}" : method.FileName;

            string menuName = method.MenuName == string.Empty
                ? CreatorUtil.GetShortNameWithBrackets(method.Type, method.Type.GetGenericArguments())
                : method.MenuName;

            string attributeLine = $"{Tab}{Tab}[MenuItem(\"Assets/Create/{menuName}\", priority = {method.Order})]";
            string typeName = CreatorUtil.GetGenericTypeDefinitionName(method.Type);

            string methodLine = $"{Tab}{Tab}private static void Create{method.TypeName}() => CreateAsset(typeof({typeName}), \"{fileName}\");";

            return $"{attributeLine}{NewLine}{methodLine}{NewLine}{NewLine}";
        }

        private static void SaveToFile(string classContent)
        {
            if (_oldContent != null && _oldContent == classContent)
                return;

            File.WriteAllText(Config.MenuItemsPath, classContent);
            AssetDatabase.Refresh();
        }
    }
}