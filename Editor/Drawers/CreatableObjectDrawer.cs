namespace GenericUnityObjects.Editor
{
    using System;
    using System.IO;
    using System.Reflection;
    using ScriptableObjects;
    using SolidUtilities.Editor.Helpers;
    using SolidUtilities.Extensions;
    using SolidUtilities.Helpers;
    using SolidUtilities.UnityEditorInternals;
    using UnityEditor;
    using UnityEngine;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector.Editor;
#endif

    [CustomPropertyDrawer(typeof(CreatableAttribute))]
#if ODIN_INSPECTOR
    [DrawerPriority(0, 0, 3)]
#endif
    public class CreatableObjectDrawer : PropertyDrawer
    {
        private static readonly string _projectPath;
        private static readonly string _assetsPath;
        private static readonly string _packagesPath;

        static CreatableObjectDrawer()
        {
            _projectPath = Directory.GetCurrentDirectory();
            _assetsPath = $"{_projectPath}/Assets";
            _packagesPath = $"{_projectPath}/Packages";
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            (_, Type type) = property.GetFieldInfoAndType();

            if (!type.InheritsFrom(typeof(ScriptableObject)))
            {
                EditorGUILayoutHelper.DrawErrorMessage("Creatable attribute can only be used on ScriptableObjects.");
                return;
            }

            if (property.objectReferenceValue != null)
            {
                GenericObjectDrawer.ObjectField(rect, property, label);
                return;
            }

            const float buttonWidth = 18f;
            const float paddingBetween = 2f;
            (var objectRect, var buttonRect) = rect.CutVertically(buttonWidth, true);
            objectRect.width -= paddingBetween;

            GenericObjectDrawer.ObjectField(objectRect, property, label);

            if ( ! GUI.Button(buttonRect, "+"))
                return;

            var asset = CreateAsset(property, type);
            property.objectReferenceValue = asset;
        }

        private ScriptableObject CreateAsset(SerializedProperty property, Type type)
        {
            var folderPath = ProjectWindowUtilProxy.GetActiveFolderPath();

            bool isGeneric = type.InheritsFrom(typeof(GenericScriptableObject));

            string fileName = isGeneric
                ? type.GetCustomAttribute<CreateGenericAssetMenuAttribute>()?.FileName
                : type.GetCustomAttribute<CreateAssetMenuAttribute>()?.fileName;

            fileName ??= $"New {type.Name}";

            var path = EditorUtility.SaveFilePanel(
                "Create a scriptable object",
                folderPath ?? Application.dataPath,
                $"{fileName}.asset",
                "asset");

            if ( ! IsValidPath(path))
                return null;

            string relativePath = PathHelper.MakeRelative(path, _projectPath);

            if (isGeneric)
                return GenericSOCreator.CreateAssetAtPath(property, type, relativePath);

            var asset = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(asset, relativePath);
            return asset;
        }

        private bool IsValidPath(string path)
        {
            return ! string.IsNullOrEmpty(path) && IsInProject(path) && HasCorrectExtension(path);
        }

        private bool IsInProject(string path)
        {
            if (PathHelper.IsSubPathOf(path, _assetsPath) || PathHelper.IsSubPathOf(path, _packagesPath))
                return true;

            Debug.LogError("The path is outside of the Assets or Packages folder. Cannot save an asset there.");
            return false;
        }

        private bool HasCorrectExtension(string path)
        {
            if (path.EndsWith(".asset"))
                return true;

            Debug.LogError("The file must have the '.asset' extension.");
            return false;
        }
    }
}