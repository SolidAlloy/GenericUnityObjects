namespace GenericScriptableObjects.Editor.MenuItemsGeneration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;

    internal static class GenericMonoBehaviourChecker
    {
        private const string NewLine = "\r\n";

        private static readonly HashSet<string> ProcessedTypes = new HashSet<string>();
        private static readonly string BehaviourSelectorFullName = typeof(BehaviourSelector).FullName;

        [DidReloadScripts]
        private static void OnScriptsReload()
        {
            var types = TypeCache.GetTypesDerivedFrom<GenericMonoBehaviour>().Where(type => type.IsGenericType);
            bool addedFiles = false;

            foreach (Type type in types)
            {
                string shortName = type.Name;

                if ( ! CheckForDuplicates(shortName))
                    continue;

                TypeChecker.CheckInvalidName(shortName);

                string typeNameWithoutArguments = shortName.Split('`')[0];
                var genericArgs = type.GetGenericArguments();
                string fileName = $"{typeNameWithoutArguments}{genericArgs.Length}";
                string fileDir = $"Assets/{Config.DefaultScriptsPath}";
                string filePath = $"{fileDir}/{fileName}.cs";

                if (File.Exists(filePath))
                    continue;

                string componentName = GetComponentName(typeNameWithoutArguments, genericArgs);
                string niceFullName = TypeChecker.GetGenericTypeDefinitionName(type);

                string fileContent =
                    $"namespace {Config.DefaultNamespaceName}{NewLine}" +
                    $"{{{NewLine}" +
                    $"    [UnityEngine.AddComponentMenu(\"{componentName}\")]{NewLine}" +
                    $"    internal class {fileName} : {BehaviourSelectorFullName}{NewLine}" +
                    $"    {{{NewLine}" +
                    $"        public override System.Type {nameof(BehaviourSelector.GenericBehaviourType)} => typeof({niceFullName});{NewLine}" +
                    $"    }}{NewLine}" +
                    $"{NewLine}" +
                    $"}}{NewLine}";

                Directory.CreateDirectory(fileDir);
                File.WriteAllText(filePath, fileContent);
                addedFiles = true;
            }

            if (addedFiles)
                AssetDatabase.Refresh();
        }

        private static string GetComponentName(string typeNameWithoutArguments, Type[] genericArgs)
        {
            var argumentNames = genericArgs.Select(argument => argument.Name);
            return $"{typeNameWithoutArguments}<{string.Join(",", argumentNames)}>";
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