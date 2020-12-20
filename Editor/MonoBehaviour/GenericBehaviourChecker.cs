namespace GenericUnityObjects.Editor.MonoBehaviour
{
    using System;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;
    using Util;

    internal static class GenericBehaviourChecker
    {
        private static readonly string BehaviourSelectorFullName = typeof(BehaviourSelector).FullName;

        // [DidReloadScripts]
        private static void OnScriptsReload()
        {
            // Create the OldTypeAndAssembly field that will hold a previous type name if it was changed.
            // Search for that field in the database.

            // The type or namespace name 'GenericBehaviourTest<>' does not exist in the namespace 'Prototype' (are you missing an assembly reference?)
            // error CS0234: Assets\Generic Unity Objects\GeneratedTypes\Prototype_GenericBehaviourTest_1.cs(10,61)

            // If the error is output, the steps are:
            // 1. Assemble type name and namespace.
            // 2. Check if they match the file name.
            // 3. If they match, find the new type by guid. (old name and assembly => database search => new name and assembly)
            // 4. Get the full name of the new type.
            // 5. Use it to rename the selector class and file.
            // 6. Replace the full type name in the auto-generated property.
            // 7. Replace the full type name excluding brackets in the auto-generated concrete classes.

            var types = TypeCache
                .GetTypesDerivedFrom<MonoBehaviour>()
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