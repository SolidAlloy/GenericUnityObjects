namespace GenericUnityObjects.Editor.ScriptableObjects
{
    using System;
    using Cysharp.Threading.Tasks;
    using GenericUnityObjects;
    using GenericUnityObjects.Util;
    using JetBrains.Annotations;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Assertions;
    using Util;

    internal class GenericSOCreator
    {
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
            async UniTaskVoid CreateAssetImpl()
            {
                try
                {
                    (Type genericSOType, string fileName) = PersistentStorage.GetGenericSODetails();
                    var concreteType = BehavioursDatabase.GetConcreteType(genericSOType);
                    await WaitUntilEditorInitialized();
                    CreateAssetFromConcreteType(concreteType, asset => ProjectWindowUtil.CreateAsset(asset, $"{fileName}.asset"));
                }
                finally
                {
                    PersistentStorage.Clear();
                }
            }

            CreateAssetImpl().Forget();
        }

        private static void FinishSOCreationAtPath()
        {
            async UniTaskVoid CreateAssetImpl()
            {
                try
                {
                    (Type genericSOType, string path) = PersistentStorage.GetGenericSODetails();
                    var concreteType = BehavioursDatabase.GetConcreteType(genericSOType);
                    await WaitUntilEditorInitialized();
                    var createdAsset = CreateAssetFromConcreteType(concreteType, asset => AssetDatabase.CreateAsset(asset, path));

                    var property = PersistentStorage.GetSavedProperty();

                    if (property == null)
                        return;

                    property.objectReferenceValue = createdAsset;
                    property.serializedObject.ApplyModifiedProperties();
                }
                finally
                {
                    PersistentStorage.Clear();
                }
            }

            CreateAssetImpl().Forget();
        }

        private static async UniTask WaitUntilEditorInitialized()
        {
            try
            {
                var _ = EditorStyles.toolbar;
            }
            catch (NullReferenceException)
            {
                await UniTask.NextFrame();
            }
        }

        public static void CreateAssetInteractively(Type genericTypeWithoutArgs, Type[] genericArgs, string fileName)
        {
            Type genericType = genericTypeWithoutArgs.MakeGenericType(genericArgs);

            if (ScriptableObjectsDatabase.TryGetConcreteType(genericType, out var concreteType))
            {
                CreateAssetFromConcreteType(concreteType, asset => ProjectWindowUtil.CreateAsset(asset, $"{fileName}.asset"));
            }

            PersistentStorage.SaveForScriptsReload(genericType, fileName);
            PersistentStorage.ExecuteOnScriptsReload(FinishSOCreationInteractively);

            ConcreteClassCreator<GenericScriptableObject>.CreateConcreteClass(genericTypeWithoutArgs, genericArgs);
            AssetDatabase.Refresh();
        }

        public static ScriptableObject CreateAssetAtPath(SerializedProperty property, Type genericType, string path)
        {
            if (ScriptableObjectsDatabase.TryGetConcreteType(genericType, out var concreteType))
            {
                return CreateAssetFromConcreteType(concreteType, asset => AssetDatabase.CreateAsset(asset, path));
            }

            PersistentStorage.SaveForScriptsReload(genericType, path);
            PersistentStorage.SaveForScriptsReload(property);
            PersistentStorage.ExecuteOnScriptsReload(FinishSOCreationAtPath);

            ConcreteClassCreator<GenericScriptableObject>.CreateConcreteClass(genericType.GetGenericTypeDefinition(), genericType.GenericTypeArguments);
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
                    $"{nameof(CreateAssetFromConcreteType)} was most likely called too early. " +
                    "Add it to EditorApplication.delayCall instead.");

                throw;
            }
        }
    }
}