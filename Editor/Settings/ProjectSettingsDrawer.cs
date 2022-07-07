namespace GenericUnityObjects.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using SolidUtilities.Editor;
    using UnityEditor;
    using UnityEngine;

    public static class ProjectSettingsDrawer
    {
        private const string AlwaysCreatableScriptableObjectLabel = "Enable + button for all generic ScriptableObject fields";
        private const string AlwaysCreatableMonoBehaviourLabel = "Enable + button for all generic MonoBehaviour fields";

        private static readonly GUIContent _alwaysCreatableScriptableObjectContent = new GUIContent(AlwaysCreatableScriptableObjectLabel,
            "Use this to add the + button next to object field when using generic ScriptableObjects without the need to put [Creatable] attribute.");

        private static readonly GUIContent _alwaysCreatableMonoBehaviourContent = new GUIContent(AlwaysCreatableMonoBehaviourLabel,
            "Use this to add the + button next to object field when using generic MonoBehaviours without the need to put [Creatable] attribute.");

        private static readonly Action _repaintAllInspectors;

        static ProjectSettingsDrawer()
        {
            var inspectorType = typeof(SceneView).Assembly.GetType("UnityEditor.InspectorWindow");
            var repaintAllMethod = inspectorType.GetMethod("RepaintAllInspectors", BindingFlags.Static | BindingFlags.NonPublic);

            if (repaintAllMethod != null)
                _repaintAllInspectors = (Action) Delegate.CreateDelegate(typeof(Action), repaintAllMethod);
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
            using var _ = EditorGUIUtilityHelper.LabelWidthBlock(320f);

            bool newScriptableObjectValue = EditorGUILayout.Toggle(_alwaysCreatableScriptableObjectContent, ProjectSettings.AlwaysCreatableScriptableObject);

            if (ProjectSettings.AlwaysCreatableScriptableObject != newScriptableObjectValue)
            {
                ProjectSettings.AlwaysCreatableScriptableObject = newScriptableObjectValue;
                RepaintAllInspectors();
            }

            bool newMonoBehaviourValue = EditorGUILayout.Toggle(_alwaysCreatableMonoBehaviourContent, ProjectSettings.AlwaysCreatableMonoBehaviour);

            if (ProjectSettings.AlwaysCreatableMonoBehaviour != newMonoBehaviourValue)
            {
                ProjectSettings.AlwaysCreatableMonoBehaviour = newMonoBehaviourValue;
                RepaintAllInspectors();
            }
        }

        private static void RepaintAllInspectors()
        {
            if (_repaintAllInspectors == null)
            {
                Debug.LogError("There is no method called UnityEditor.InspectorWindow.RepaintAllInspectors() in this version of Unity.");
                return;
            }

            _repaintAllInspectors();
        }

        private static HashSet<string> GetKeywords()
        {
            var keywords = new HashSet<string>();
            keywords.AddWords(AlwaysCreatableScriptableObjectLabel);
            keywords.AddWords(AlwaysCreatableMonoBehaviourLabel);
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