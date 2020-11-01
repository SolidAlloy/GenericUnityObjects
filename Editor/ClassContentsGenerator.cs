namespace GenericScriptableObjects.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using JetBrains.Annotations;
    using SolidUtilities.Editor.Helpers;
    using UnityEditor;
    using UnityEngine;

    internal static class ClassContentsGenerator
    {
        private const string Folders = "Resources/Editor";
        private static readonly string FilePath = $"{Application.dataPath}/{Folders}/MenuItems.cs";
        private static string _oldContent;

        private static Regex GetMethodRegex(string typeName) => new Regex($@"\[MenuItem.*?\n.*?{typeName}.*?\n");

        public static void GenerateClass(MenuItemMethod[] newMethods)
        {
            string classContent = GetClassContents();

            var oldMethodsSet = new HashSet<MenuItemMethod>(AssetCreatorPersistentStorage.MenuItemMethods, MenuItemMethod.Comparer);
            var newMethodsSet = new HashSet<MenuItemMethod>(newMethods, MenuItemMethod.Comparer);

            if (oldMethodsSet.SetEquals(newMethodsSet))
                return;

            classContent = RemoveOldMethods(classContent, oldMethodsSet, newMethodsSet);
            classContent = ModifyExistingMethods(classContent, oldMethodsSet, newMethodsSet);
            classContent = AddNewMethods(classContent, oldMethodsSet, newMethodsSet);

            AssetCreatorPersistentStorage.MenuItemMethods = newMethods;

            SaveToFile(classContent);
        }

        private static string ModifyExistingMethods(string classContent, HashSet<MenuItemMethod> oldMethodsSet, HashSet<MenuItemMethod> newMethodsSet)
        {
            var oldExistingMethods = new List<MenuItemMethod>();
            var newExistingMethods = new List<MenuItemMethod>();

            foreach (MenuItemMethod oldMethod in oldMethodsSet)
            {
                if (newMethodsSet.Contains(oldMethod))
                {
                    oldExistingMethods.Add(oldMethod);
                    newMethodsSet.TryGetValue(oldMethod, out MenuItemMethod newMethod);
                    newExistingMethods.Add(newMethod);
                }
            }

            for (int i = 0; i < oldExistingMethods.Count; ++i)
            {
                MenuItemMethod oldMethod = oldExistingMethods[i];
                MenuItemMethod newMethod = newExistingMethods[i];

                MenuItemMethod changedValues = new MenuItemMethod
                {
                    FileName =
                };
            }

            return classContent;
        }

        private static string ChangeMethod(string classContent, string fileName = null,
            string menuName = null, string namespaceName = null, string scriptsPath = null, int order = -1)
        {

        }

        private static string RemoveOldMethods(string classContent, HashSet<MenuItemMethod> oldMethodsSet, HashSet<MenuItemMethod> newMethodsSet)
        {
            var methodsToRemove = oldMethodsSet.ExceptWith(newMethodsSet, MenuItemMethod.Comparer);

            foreach (MenuItemMethod method in methodsToRemove)
            {
                var regex = GetMethodRegex(method.TypeName);
                classContent = regex.Replace(classContent, string.Empty, 1);
            }

            return classContent;
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

            int insertPos = classContent.Length - 3;
            return classContent.Insert(insertPos, newMethods);
        }

        private static string GetClassContents()
        {
            const string emptyClass =
                "namespace GenericScriptableObjects.Editor\n{\nusing UnityEditor;\ninternal class MenuItems : GenericSOCreator\n{\n}\n}";

            if (!File.Exists(FilePath))
                return emptyClass;

            _oldContent = File.ReadAllText(FilePath);
            return _oldContent;
        }

        private static string CreateMenuItemMethod(MenuItemMethod method)
        {
            string fileName = method.FileName ?? method.TypeName;
            string menuName = method.MenuName ?? GetGenericTypeName(method.Type);

            string attributeLine = $"[MenuItem(\"Assets/Create/{menuName}\", priority = {method.Order})]";
            string typeName = GetGenericTypeDefinitionName(method.Type);
            string methodLine = $"private static void Create{method.TypeName}() => CreateAsset(typeof({typeName}), \"{method.NamespaceName}\", \"{method.ScriptsPath}\", \"{fileName}\");";
            return $"{attributeLine}\n{methodLine}\n";
        }

        private static string GetGenericTypeDefinitionName(Type type)
        {
            string typeNameWithoutArguments = type.FullName.Split('`')[0];
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

    [Serializable]
    internal struct MenuItemMethod
    {
        public string TypeName;
        public string FileName;
        public string MenuName;
        public string NamespaceName;
        public string ScriptsPath;
        public int Order;
        public Type Type;

        private static EqualityComparer _comparer;

        public static EqualityComparer Comparer
        {
            get
            {
                if (_comparer == null)
                    _comparer = new EqualityComparer();

                return _comparer;
            }
        }

        public class EqualityComparer : EqualityComparer<MenuItemMethod>
        {
            public override bool Equals(MenuItemMethod x, MenuItemMethod y)
            {
                return x.TypeName == y.TypeName;
            }

            public override int GetHashCode(MenuItemMethod obj)
            {
                return obj.TypeName.GetHashCode();
            }
        }
    }

    internal static class HashSetExtensions
    {
        [PublicAPI, NotNull, Pure] public static HashSet<T> ExceptWith<T>(this HashSet<T> thisSet, HashSet<T> otherSet, EqualityComparer<T> comparer = null)
        {
            if (comparer == null)
                comparer = EqualityComparer<T>.Default;

            var newSet = new HashSet<T>(thisSet, comparer);
            newSet.ExceptWith(otherSet);
            return newSet;
        }
    }
}