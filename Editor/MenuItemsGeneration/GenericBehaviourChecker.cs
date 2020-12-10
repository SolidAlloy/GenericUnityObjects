namespace GenericScriptableObjects.Editor.MenuItemsGeneration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using AssetCreation;
    using SolidUtilities.Helpers;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;
    using UnityEngine.Assertions;

    internal static class GenericBehaviourChecker
    {
        private const string NewLine = Config.NewLine;

        private static readonly HashSet<string> ProcessedTypes = new HashSet<string>();
        private static readonly string BehaviourSelectorFullName = typeof(BehaviourSelector).FullName;

        [DidReloadScripts]
        private static void OnScriptsReload()
        {
            var types = TypeCache
                .GetTypesWithAttribute<GenericMonoBehaviourAttribute>()
                .Where(type => type.IsGenericType);

            bool addedFiles = false;

            foreach (Type type in types)
            {
                string shortName = type.Name;

                if ( ! CheckForDuplicates(shortName))
                    continue;

                CreatorUtil.CheckInvalidName(shortName);

                var genericArgs = type.GetGenericArguments();
                string filePath = GenericBehaviourCreator.GetPathToGeneratedFile(type, genericArgs);

                if (File.Exists(filePath))
                    continue;

                string componentName = CreatorUtil.GetShortNameWithBrackets(type, genericArgs);
                string className = Path.GetFileNameWithoutExtension(filePath);
                string niceFullName = CreatorUtil.GetGenericTypeDefinitionName(type);

                string fileContent =
                    $"namespace {Config.DefaultNamespaceName}{NewLine}" +
                    $"{{{NewLine}" +
                    $"    [UnityEngine.AddComponentMenu(\"Scripts/{componentName}\")]{NewLine}" +
                    $"    internal class {className} : {BehaviourSelectorFullName}{NewLine}" +
                    $"    {{{NewLine}" +
                    $"        public override System.Type {nameof(BehaviourSelector.GenericBehaviourType)} => typeof({niceFullName});{NewLine}" +
                    $"    }}{NewLine}" +
                    $"{NewLine}" +
                    $"}}";

                string fileDir = Path.GetDirectoryName(filePath);
                Assert.IsNotNull(fileDir);
                Directory.CreateDirectory(fileDir);
                File.WriteAllText(filePath, fileContent);
                addedFiles = true;
            }

            if (addedFiles)
                AssetDatabase.Refresh();
        }

        private static bool CheckForDuplicates(string typeName)
        {
            if (ProcessedTypes.Contains(typeName))
            {
                Debug.LogError($"Generic MonoBehaviour called {typeName} exists in multiple namespaces. The plugin does not support this. Please rename one of the types");
                return false;
            }

            ProcessedTypes.Add(typeName);
            return true;
        }
    }
}