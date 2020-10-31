namespace GenericScriptableObjects.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using SolidUtilities.Editor.Helpers;
    using UnityEditor;
    using UnityEngine;

    internal static class ClassContentsGenerator
    {
        private const string Folders = "Resources/Editor";
        private static readonly string FilePath = $"{Application.dataPath}/{Folders}/MenuItems.cs";
        private static string _oldContent;

        public static void GenerateClass(HashSet<string> newTypeSet, Dictionary<string, KeyValuePair<Type, CreateGenericAssetMenuAttribute>> typeDict)
        {
            string classContent = GetClassContents();

            var oldTypeSet = new HashSet<string>(AssetCreatorPersistentStorage.TypesWithAttribute);

            if (newTypeSet.SetEquals(oldTypeSet))
                return;

            classContent = RemoveOldMethods(classContent, oldTypeSet, newTypeSet);
            classContent = AddNewMethods(classContent, oldTypeSet, newTypeSet, typeDict);

            SaveToFile(classContent);
        }

        private static string RemoveOldMethods(string classContent, HashSet<string> oldTypeSet, HashSet<string> newTypeSet)
        {
            var typesToRemove = new HashSet<string>(oldTypeSet);
            typesToRemove.ExceptWith(newTypeSet);

            foreach (string typeName in typesToRemove)
            {
                var regex = new Regex($@"\[MenuItem.*?\n.*?{typeName}.*?\n");
                classContent = regex.Replace(classContent, string.Empty, 1);
            }

            return classContent;
        }

        private static string AddNewMethods(string classContent, HashSet<string> oldTypeSet,
            HashSet<string> newTypeSet, Dictionary<string, KeyValuePair<Type, CreateGenericAssetMenuAttribute>> typeDict)
        {
            var typesToAdd = new HashSet<string>(newTypeSet);
            typesToAdd.ExceptWith(oldTypeSet);

            if (typesToAdd.Count == 0)
                return classContent;

            string newMethods = string.Empty;

            foreach (string typeName in typesToAdd)
            {
                var kvp = typeDict[typeName];
                newMethods += CreateMenuItemMethod(typeName, kvp.Key, kvp.Value);
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

        private static string CreateMenuItemMethod(string classSafeName, Type type, CreateGenericAssetMenuAttribute attribute)
        {
            string fileName = attribute.FileName ?? classSafeName;
            string menuName = attribute.MenuName ?? GetGenericTypeName(type);

            string attributeLine = $"[MenuItem(\"Assets/Create/{menuName}\", priority = {attribute.Order})]";
            string typeName = GetGenericTypeDefinitionName(type);
            string methodLine = $"private static void Create{classSafeName}() => CreateAsset(typeof({typeName}), \"{attribute.NamespaceName}\", \"{attribute.ScriptsPath}\", \"{fileName}\");";
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
}