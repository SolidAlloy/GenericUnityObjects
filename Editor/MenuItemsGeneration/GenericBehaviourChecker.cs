namespace GenericScriptableObjects.Editor.MenuItemsGeneration
{
    using System;
    using System.IO;
    using System.Linq;
    using AssetCreation;
    using UnityEditor;
    using UnityEditor.Callbacks;

    internal static class GenericBehaviourChecker
    {
        private const string NewLine = Config.NewLine;

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

                CreatorUtil.CheckInvalidName(shortName);

                var genericArgs = type.GetGenericArguments();
                string fileName = GenericBehaviourCreator.GetGeneratedFileName(type);
                string filePath = $"{Config.GeneratedTypesPath}/{fileName}";

                if (File.Exists(filePath))
                    continue;

                string componentName = CreatorUtil.GetShortNameWithBrackets(type, genericArgs);
                string className = Path.GetFileNameWithoutExtension(fileName);
                string niceFullName = CreatorUtil.GetGenericTypeDefinitionName(type);

                string fileContent =
                    $"namespace {Config.GeneratedTypesNamespace}{NewLine}" +
                    $"{{{NewLine}" +
                    $"    [UnityEngine.AddComponentMenu(\"Scripts/{componentName}\")]{NewLine}" +
                    $"    internal class {className} : {BehaviourSelectorFullName}{NewLine}" +
                    $"    {{{NewLine}" +
                    $"        public override System.Type {nameof(BehaviourSelector.GenericBehaviourType)} => typeof({niceFullName});{NewLine}" +
                    $"    }}{NewLine}" +
                    $"{NewLine}" +
                    $"}}";

                CreatorUtil.WriteAllText(filePath, fileContent, Config.GeneratedTypesPath);
                addedFiles = true;
            }

            if (addedFiles)
                AssetDatabase.Refresh();
        }
    }
}