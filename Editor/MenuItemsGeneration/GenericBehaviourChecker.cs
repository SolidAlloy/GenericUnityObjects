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

                string fileName = GenericBehaviourUtil.GetGeneratedFileName(type);
                string filePath = $"{Config.GeneratedTypesPath}/{fileName}";

                if (File.Exists(filePath))
                    continue;

                string fileContent = GenerateSelectorScript(type, fileName);
                CreatorUtil.WriteAllText(filePath, fileContent, Config.GeneratedTypesPath);
                addedFiles = true;
            }

            if (addedFiles)
                AssetDatabase.Refresh();
        }

        private static string GenerateSelectorScript(Type genericType, string fileName)
        {
            var genericArgs = genericType.GetGenericArguments();
            string componentName = CreatorUtil.GetShortNameWithBrackets(genericType, genericArgs);
            string className = Path.GetFileNameWithoutExtension(fileName);
            string niceFullName = CreatorUtil.GetGenericTypeDefinitionName(genericType);

            const string tab = Config.Tab;
            const string nl = Config.NewLine;

            return $"namespace {Config.GeneratedTypesNamespace}{nl}" +
                   $"{{{nl}" +
                   $"{tab}[UnityEngine.AddComponentMenu(\"Scripts/{componentName}\")]{nl}" +
                   $"{tab}internal class {className} : {BehaviourSelectorFullName}{nl}" +
                   $"{tab}{{{nl}" +
                   $"{tab}{tab}public override System.Type {nameof(BehaviourSelector.GenericBehaviourType)} => typeof({niceFullName});{nl}" +
                   $"{tab}}}{nl}" +
                   $"{nl}" +
                   $"}}";
        }
    }
}