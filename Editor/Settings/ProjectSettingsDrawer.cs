namespace GenericUnityObjects.Editor
{
    using System.Collections.Generic;
    using System.Reflection;
    using SolidUtilities.Editor;
    using UnityEditor;
    using UnityEngine;

    public static class ProjectSettingsDrawer
    {
        private const string AlwaysCreatableLabel = "Enable + button for all GenericScriptableObject fields";

        private static readonly GUIContent _alwaysCreatableContent = new GUIContent(AlwaysCreatableLabel,
            "Use this to add the + button next to object field when using GenericScriptableObjects without the need to put [Creatable] attribute.");

        private static readonly MethodInfo _repaintAllMethod;

        static ProjectSettingsDrawer()
        {
            var inspectorType = typeof(SceneView).Assembly.GetType("UnityEditor.InspectorWindow");
            _repaintAllMethod = inspectorType.GetMethod("RepaintAllInspectors", BindingFlags.Static | BindingFlags.NonPublic);
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new SettingsProvider("Project/Packages/Generic Unity Objects", SettingsScope.Project)
            {
                guiHandler = OnGUI,
                keywords = GetKeywords()
            };
        }

        private static void OnGUI(string searchContext)
        {
            using var _ = EditorGUIUtilityHelper.LabelWidthBlock(310f);

            bool newValue = EditorGUILayout.Toggle(_alwaysCreatableContent, ProjectSettings.AlwaysCreatable);

            if (ProjectSettings.AlwaysCreatable == newValue)
                return;

            ProjectSettings.AlwaysCreatable = newValue;
            RepaintAllInspectors();
        }

        private static void RepaintAllInspectors() => _repaintAllMethod?.Invoke(null, null);

        private static HashSet<string> GetKeywords()
        {
            var keywords = new HashSet<string>();
            keywords.AddWords(AlwaysCreatableLabel);
            return keywords;
        }

        private static readonly char[] _separators = { ' ' };

        private static void AddWords(this HashSet<string> set, string phrase)
        {
            foreach (string word in phrase.Split(_separators))
            {
                set.Add(word);
            }
        }
    }
}