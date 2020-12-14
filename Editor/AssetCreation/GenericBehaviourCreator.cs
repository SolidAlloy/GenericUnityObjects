namespace GenericScriptableObjects.Editor.AssetCreation
{
    using System;
    using System.IO;
    using System.Linq;
    using NUnit.Framework;
    using SolidUtilities.Extensions;
    using SolidUtilities.Helpers;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;
    using Util;
    using Object = UnityEngine.Object;

    internal static class GenericBehaviourCreator
    {
        public static void AddComponent(Type selectorComponentType, GameObject gameObject, Type genericTypeWithoutArgs, Type[] genericArgs)
        {
            if (GenericObjectDatabase.TryGetValue(genericTypeWithoutArgs, genericArgs, out Type concreteComponent))
            {
                DestroySelectorComponent(gameObject, selectorComponentType);
                gameObject.AddComponent(concreteComponent);
                return;
            }

            Type genericType = genericTypeWithoutArgs.MakeGenericType(genericArgs);

            Type existingType = CreatorUtil.GetEmptyTypeDerivedFrom(genericType);
            if (existingType != null)
            {
                DestroySelectorComponent(gameObject, selectorComponentType);
                GenericObjectDatabase.Add(genericTypeWithoutArgs, genericArgs, existingType);
                gameObject.AddComponent(existingType);
                return;
            }

            string generatedFileName = GetGeneratedFileName(genericTypeWithoutArgs);
            string generatedFilePath = $"{Config.GeneratedTypesPath}/{generatedFileName}";
            string generatedFileContent = File.ReadAllText(generatedFilePath);

            string uniqueClassName = GetUniqueClassName(genericTypeWithoutArgs, genericArgs, generatedFileContent);

            string typeWithBrackets = CreatorUtil.GetFullNameWithBrackets(genericTypeWithoutArgs, genericArgs);

            string componentName = GetComponentName(genericTypeWithoutArgs, genericArgs);

            string lineToAdd = $"{Config.Tab}[UnityEngine.AddComponentMenu(\"Scripts/{componentName}\")]{Config.NewLine}" +
                               $"{Config.Tab}internal class {uniqueClassName} : {typeWithBrackets} {{ }}{Config.NewLine}{Config.NewLine}";

            int insertPos = generatedFileContent.Length - 1;
            generatedFileContent = generatedFileContent.Insert(insertPos, lineToAdd);
            File.WriteAllText(generatedFilePath, generatedFileContent);

            PersistentStorage.SaveForAssemblyReload(gameObject, genericType);

            DestroySelectorComponent(gameObject, selectorComponentType);
            AssetDatabase.Refresh();
        }

        [DidReloadScripts]
        private static void OnScriptsReload()
        {
            if ( ! PersistentStorage.NeedsBehaviourCreation)
                return;

            try
            {
                (GameObject gameObject, Type genericType) =
                    PersistentStorage.GetGenericBehaviourDetails();

                Type existingType = CreatorUtil.GetEmptyTypeDerivedFrom(genericType);
                Assert.IsNotNull(existingType);

                GenericObjectDatabase.Add(genericType, existingType);
                gameObject.AddComponent(existingType);
            }
            finally
            {
                PersistentStorage.Clear();
            }
        }

        private static string GetComponentName(Type genericTypeWithoutArgs, Type[] genericArgs)
        {
            Assert.IsTrue(genericTypeWithoutArgs.IsGenericTypeDefinition);

            string shortName = genericTypeWithoutArgs.Name;
            string typeNameWithoutSuffix = shortName.StripGenericSuffix();

            var argumentNames = genericArgs
                .Select(argument => argument.FullName)
                .Select(fullName => fullName.ReplaceWithBuiltInName())
                .Select(fullName => fullName.GetSubstringAfterLast('.'));

            return $"{typeNameWithoutSuffix}<{string.Join(",", argumentNames)}>";
        }

        private static string GetUniqueClassName(Type genericTypeWithoutArgs, Type[] genericArgs, string generatedFileContent)
        {
            string argumentNames = string.Join("_", genericArgs.Select(type => type.Name.MakeClassFriendly()));
            string defaultClassName = $"{genericTypeWithoutArgs.Name.MakeClassFriendly().StripGenericSuffix()}_{argumentNames}";

            int sameClassNamesCount = generatedFileContent.CountMatches(defaultClassName);
            return sameClassNamesCount == 0 ? defaultClassName : $"{defaultClassName}_{sameClassNamesCount}";
        }

        public static string GetGeneratedFileName(Type genericTypeWithoutArgs)
        {
            string fileName = genericTypeWithoutArgs.FullName.MakeClassFriendly();
            return $"{fileName}.cs";
        }

        private static int CountMatches(this string text, string match)
        {
            return ( text.Length - text.Replace(match, string.Empty).Length ) / match.Length;
        }

        private static void DestroySelectorComponent(GameObject gameObject, Type selectorComponentType)
        {
            if (gameObject.TryGetComponent(selectorComponentType, out Component selectorComponent))
            {
                Object.DestroyImmediate(selectorComponent);
            }
        }
    }
}