namespace GenericUnityObjects.Editor.ScriptableObjects
{
    using System;
    using GenericUnityObjects;
    using GenericUnityObjects.Util;
    using JetBrains.Annotations;
    using TypeReferences;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Assertions;
    using Util;

    public class GenericSOCreator
    {
        private const string GenericSOTypeKey = "GenericSOType";
        private const string FileNameKey = "FileName";
        private const string PathKey = "Path";
        private const string InstanceIDKey = "InstanceId";
        private const string PropertyPathKey = "PropertyPath";

        /// <summary>
        /// Creates a <see cref="GenericScriptableObject"/> asset when used in a method with the <see cref="UnityEditor.MenuItem"/>
        /// attribute. A class derived from <see cref="GenericSOCreator"/> is generated automatically in a separate DLL
        /// and is filled with methods that call CreateAsset with different parameters.
        /// </summary>
        /// <param name="genericTypeWithoutArgs">The type of <see cref="GenericScriptableObject"/> to create.</param>
        /// <param name="fileName">Name for an asset.</param>
        [UsedImplicitly]
        protected static void CreateAsset(Type genericTypeWithoutArgs, string fileName)
        {
            Assert.IsTrue(genericTypeWithoutArgs.IsGenericTypeDefinition);

            TypeSelectionWindow.Create(genericTypeWithoutArgs, genericArgs =>
            {
                CreateAssetInteractively(genericTypeWithoutArgs, genericArgs, fileName);
            });
        }

        private static void FinishSOCreationInteractively()
        {
            try
            {
                Type genericSOType = PersistentStorage.GetData<TypeReference>(GenericSOTypeKey).Type;
                string fileName = PersistentStorage.GetData<string>(FileNameKey);

                var concreteType = ScriptableObjectsDatabase.GetConcreteType(genericSOType);
                CreateAssetFromConcreteType(concreteType, asset => ProjectWindowUtil.CreateAsset(asset, $"{fileName}.asset"));
            }
            finally
            {
                PersistentStorage.DeleteData(GenericSOTypeKey);
                PersistentStorage.DeleteData(FileNameKey);
            }
        }

        private static void FinishSOCreationAtPath()
        {
            try
            {
                Type genericSOType = PersistentStorage.GetData<TypeReference>(GenericSOTypeKey).Type;
                string path = PersistentStorage.GetData<string>(PathKey);

                var concreteType = ScriptableObjectsDatabase.GetConcreteType(genericSOType);
                var createdAsset = CreateAssetFromConcreteType(concreteType, asset => AssetDatabase.CreateAsset(asset, path));
                EditorGUIUtility.PingObject(createdAsset);

                var property = GetSavedProperty();

                if (property == null)
                    return;

                property.objectReferenceValue = createdAsset;
                property.serializedObject.ApplyModifiedProperties();
            }
            finally
            {
                PersistentStorage.DeleteData(InstanceIDKey);
                PersistentStorage.DeleteData(PropertyPathKey);
                PersistentStorage.DeleteData(GenericSOTypeKey);
                PersistentStorage.DeleteData(PathKey);
            }
        }

        private static SerializedProperty GetSavedProperty()
        {
            int instanceID = PersistentStorage.GetData<int>(InstanceIDKey);
            var targetObject = EditorUtility.InstanceIDToObject(instanceID);

            if (targetObject == null)
                return null;

            var serializedObject = new SerializedObject(targetObject);
            string propertyPath = PersistentStorage.GetData<string>(PropertyPathKey);
            return serializedObject.FindProperty(propertyPath);
        }

        private static void SaveProperty(SerializedProperty property)
        {
            PersistentStorage.SaveData(InstanceIDKey, property.serializedObject.targetObject.GetInstanceID());
            PersistentStorage.SaveData(PropertyPathKey, property.propertyPath);
        }

        public static void CreateAssetInteractively(Type genericTypeWithoutArgs, Type[] genericArgs, string fileName)
        {
            Type genericType = genericTypeWithoutArgs.MakeGenericType(genericArgs);

            if (ScriptableObjectsDatabase.TryGetConcreteType(genericType, out var concreteType))
            {
                CreateAssetFromConcreteType(concreteType, asset => ProjectWindowUtil.CreateAsset(asset, $"{fileName}.asset"));
                return;
            }

            PersistentStorage.SaveData(GenericSOTypeKey, new TypeReference(genericType));
            PersistentStorage.SaveData(FileNameKey, fileName);
            PersistentStorage.ExecuteOnScriptsReload(FinishSOCreationInteractively);

            ConcreteClassCreator<ScriptableObject>.CreateConcreteClass(genericTypeWithoutArgs, genericArgs);
            AssetDatabase.Refresh();
        }

        public static ScriptableObject CreateAssetAtPath(SerializedProperty property, Type genericType, string path)
        {
            if (ScriptableObjectsDatabase.TryGetConcreteType(genericType, out var concreteType))
            {
                return CreateAssetFromConcreteType(concreteType, asset => AssetDatabase.CreateAsset(asset, path));
            }

            PersistentStorage.SaveData(GenericSOTypeKey, new TypeReference(genericType));
            PersistentStorage.SaveData(PathKey, path);
            SaveProperty(property);
            PersistentStorage.ExecuteOnScriptsReload(FinishSOCreationAtPath);

            ConcreteClassCreator<ScriptableObject>.CreateConcreteClass(genericType.GetGenericTypeDefinition(), genericType.GenericTypeArguments);
            AssetDatabase.Refresh();
            // At this point the domain is reloaded, so the method doesn't return.
            return null;
        }

        private static ScriptableObject CreateAssetFromConcreteType(Type concreteType, Action<ScriptableObject> saveAsset)
        {
            var asset = ScriptableObject.CreateInstance(concreteType);

            try
            {
                saveAsset(asset);
                return asset;
            }
            catch (NullReferenceException)
            {
                Debug.LogError(
                    $"{nameof(CreateAssetFromConcreteType)} was most likely called too early. Delay it more.");

                throw;
            }
        }
    }
}