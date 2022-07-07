namespace GenericUnityObjects.Editor
{
    using System;
    using System.IO;
    using System.Reflection;
    using MonoBehaviours;
    using ScriptableObjects;
    using SolidUtilities;
    using SolidUtilities.Editor;
    using SolidUtilities.UnityEditorInternals;
    using TypeReferences;
    using UnityEditor;
    using UnityEngine;
    using Util;
    using Object = UnityEngine.Object;

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

        private static CreatableObjectDrawer _instance;
        public static CreatableObjectDrawer Instance => _instance ??= new CreatableObjectDrawer();

        static CreatableObjectDrawer()
        {
            _projectPath = Directory.GetCurrentDirectory();
            _assetsPath = $"{_projectPath}/Assets";
            _packagesPath = $"{_projectPath}/Packages";
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            (_, Type type) = property.GetFieldInfoAndType();

            if (!type.InheritsFrom(typeof(ScriptableObject)) && !type.InheritsFrom(typeof(MonoBehaviour)))
            {
                EditorGUILayoutHelper.DrawErrorMessage("Creatable attribute can only be used on ScriptableObjects and MonoBehaviours.");
                return;
            }

            if (type.IsAbstract)
            {
                EditorGUILayoutHelper.DrawErrorMessage("Creatable attribute can only be used on fields of non-abstract type.");
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

            // this is for scriptable objects
            if (type.InheritsFrom(typeof(ScriptableObject)))
            {
                CreateScriptableObject(property, type);
            }
            else
            {
                CreateMonoBehaviour(property, type);
            }
        }

        #region Create Scriptable Object

        private void CreateScriptableObject(SerializedProperty property, Type type)
        {
            var asset = CreateAsset(property, type);
            property.objectReferenceValue = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private ScriptableObject CreateAsset(SerializedProperty property, Type type)
        {
            var folderPath = ProjectWindowUtilProxy.GetActiveFolderPath();

            bool isGeneric = type.IsGenericType;

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

        #endregion

        #region Create MonoBehaviour

        private const string TargetComponentKey = "CreatableObjectDrawer_TargetComponent";
        private const string PropertyPathKey = "CreatableObjectDrawer_PropertyPath";
        private const string AddedComponentTypeKey = "CreatableObjectDrawer_AddedComponent";

        private void CreateMonoBehaviour(SerializedProperty property, Type type)
        {
            var parentComponent = property.serializedObject.targetObject as MonoBehaviour;

            if (parentComponent == null)
                return;

            var gameObject = parentComponent.gameObject;

            var component = AddComponentHelper.AddComponent(gameObject, type, out bool reloadRequired);

            if (reloadRequired)
            {
                PersistentStorage.SaveData(TargetComponentKey, property.serializedObject.targetObject);
                PersistentStorage.SaveData(PropertyPathKey, property.propertyPath);
                PersistentStorage.SaveData(AddedComponentTypeKey, new TypeReference(type));
                PersistentStorage.ExecuteOnScriptsReload(OnAfterComponentAdded);
                AssetDatabase.Refresh();
            }
            else
            {
                property.objectReferenceValue = component;
            }
        }

        private static void OnAfterComponentAdded()
        {
            try
            {
                var targetComponent = PersistentStorage.GetData<Object>(TargetComponentKey);
                var propertyPath = PersistentStorage.GetData<string>(PropertyPathKey);
                var componentType = PersistentStorage.GetData<TypeReference>(AddedComponentTypeKey).Type;

                var addedComponent = ((MonoBehaviour)targetComponent).gameObject.GetComponent(componentType);
                var property = Editor.CreateEditor(targetComponent).serializedObject.FindProperty(propertyPath);
                property.objectReferenceValue = addedComponent;
                property.serializedObject.ApplyModifiedProperties();
            }
            finally
            {
                PersistentStorage.DeleteData(TargetComponentKey);
                PersistentStorage.DeleteData(PropertyPathKey);
                PersistentStorage.DeleteData(AddedComponentTypeKey);
            }
        }

        #endregion
    }

    public static class AddComponentHelper
    {
        public static Component AddComponent(GameObject gameObject, Type componentType, out bool reloadRequired)
        {
            if (!componentType.IsGenericType)
            {
                reloadRequired = false;
                return Undo.AddComponent(gameObject, componentType);
            }

            return GenericBehaviourCreator.AddComponent(null, gameObject, componentType.GetGenericTypeDefinition(), componentType.GetGenericArguments(), out reloadRequired);
        }
    }
}