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

    internal class GenericBehaviourCreator
    {
        public static void AddComponent(Type selectorComponentType, GameObject gameObject, Type genericTypeWithoutArgs, Type[] genericArgs)
        {
            var creator = new GenericBehaviourCreator(selectorComponentType, gameObject, genericTypeWithoutArgs, genericArgs);
            creator.AddComponent();
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

                Type concreteType = CreatorUtil.GetEmptyTypeDerivedFrom(genericType);
                Assert.IsNotNull(concreteType);

                GenericObjectDatabase.Add(genericType, concreteType);
                gameObject.AddComponent(concreteType);
            }
            finally
            {
                PersistentStorage.Clear();
            }
        }

        private readonly Type _selectorComponentType;
        private readonly Type _genericTypeWithoutArgs;
        private readonly Type[] _genericArgs;
        private readonly GameObject _gameObject;

        private GenericBehaviourCreator(Type selectorComponentType, GameObject gameObject, Type genericTypeWithoutArgs, Type[] genericArgs)
        {
            _selectorComponentType = selectorComponentType;
            _gameObject = gameObject;
            _genericTypeWithoutArgs = genericTypeWithoutArgs;
            _genericArgs = genericArgs;
        }

        private void AddComponent()
        {
            if (GenericObjectDatabase.TryGetValue(_genericTypeWithoutArgs, _genericArgs, out Type concreteComponent))
            {
                AddConcreteComponent(concreteComponent);
                return;
            }

            if (TryFindExistingType(out Type genericType))
                return;

            AddConcreteClassToFile();
            PersistentStorage.SaveForAssemblyReload(_gameObject, genericType);
            DestroySelectorComponent();
            AssetDatabase.Refresh();
        }

        private bool TryFindExistingType(out Type genericType)
        {
            genericType = _genericTypeWithoutArgs.MakeGenericType(_genericArgs);

            Type existingType = CreatorUtil.GetEmptyTypeDerivedFrom(genericType);

            if (existingType == null)
                return false;

            GenericObjectDatabase.Add(_genericTypeWithoutArgs, _genericArgs, existingType);
            AddConcreteComponent(existingType);
            return true;
        }

        private void AddConcreteClassToFile()
        {
            string generatedFileName = GenericBehaviourUtil.GetGeneratedFileName(_genericTypeWithoutArgs);
            string generatedFilePath = $"{Config.GeneratedTypesPath}/{generatedFileName}";
            string generatedFileContent = File.ReadAllText(generatedFilePath);

            string lineToAdd = GenerateConcreteClass(generatedFileContent);

            int insertPos = generatedFileContent.Length - 1;
            generatedFileContent = generatedFileContent.Insert(insertPos, lineToAdd);
            File.WriteAllText(generatedFilePath, generatedFileContent);
        }

        private string GenerateConcreteClass(string fileContent)
        {
            string uniqueClassName = GetUniqueClassName(fileContent);
            string typeWithBrackets = CreatorUtil.GetFullNameWithBrackets(_genericTypeWithoutArgs, _genericArgs);
            string componentName = GetComponentName();

            const string tab = Config.Tab;
            const string nl = Config.NewLine;

            return $"{tab}[UnityEngine.AddComponentMenu(\"Scripts/{componentName}\")]{nl}" +
                   $"{tab}internal class {uniqueClassName} : {typeWithBrackets} {{ }}{nl}{nl}";
        }

        private void AddConcreteComponent(Type concreteType)
        {
            DestroySelectorComponent();
            _gameObject.AddComponent(concreteType);
        }

        private void DestroySelectorComponent()
        {
            if (_gameObject.TryGetComponent(_selectorComponentType, out Component selectorComponent))
            {
                Object.DestroyImmediate(selectorComponent);
            }
        }

        private string GetComponentName()
        {
            Assert.IsTrue(_genericTypeWithoutArgs.IsGenericTypeDefinition);

            string shortName = _genericTypeWithoutArgs.Name;
            string typeNameWithoutSuffix = shortName.StripGenericSuffix();

            var argumentNames = _genericArgs
                .Select(argument => argument.FullName)
                .Select(fullName => fullName.ReplaceWithBuiltInName())
                .Select(fullName => fullName.GetSubstringAfterLast('.'));

            return $"{typeNameWithoutSuffix}<{string.Join(",", argumentNames)}>";
        }

        private string GetUniqueClassName(string generatedFileContent)
        {
            string argumentNames = string.Join("_", _genericArgs.Select(type => type.Name.MakeClassFriendly()));
            string defaultClassName = $"{_genericTypeWithoutArgs.Name.MakeClassFriendly().StripGenericSuffix()}_{argumentNames}";

            int sameClassNamesCount = generatedFileContent.CountSubstrings(defaultClassName);
            return sameClassNamesCount == 0 ? defaultClassName : $"{defaultClassName}_{sameClassNamesCount}";
        }
    }
}