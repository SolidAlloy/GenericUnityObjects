namespace GenericUnityObjects.Editor.ScriptableObjects
{
    using System;
    using System.Linq;
    using GenericUnityObjects;
    using GenericUnityObjects.Util;
    using JetBrains.Annotations;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;
    using UnityEngine.Assertions;
    using Util;

    internal class GenericSOCreator
    {
        private static Type _concreteType;
        private static string _fileName;

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

            var constraints = genericTypeWithoutArgs.GetGenericArguments()
                .Select(type => type.GetGenericParameterConstraints())
                .ToArray();

            TypeSelectionWindow.Create(constraints, genericArgs =>
            {
                CreateAsset(genericTypeWithoutArgs, genericArgs, fileName);
            });
        }

        private static void FinishSOCreation()
        {
            try
            {
                Type genericSOType;
                (genericSOType, _fileName) = PersistentStorage.GetGenericSODetails();
                _concreteType = BehavioursDatabase.GetConcreteType(genericSOType);

                EditorApplication.update += CreateAssetInteractively;
            }
            finally
            {
                PersistentStorage.Clear();
            }
        }

        private static void CreateAsset(Type genericTypeWithoutArgs, Type[] genericArgs, string fileName)
        {
            Type genericType = genericTypeWithoutArgs.MakeGenericType(genericArgs);

            if (ScriptableObjectsDatabase.TryGetConcreteType(genericType, out _concreteType))
            {
                _fileName = fileName;
                CreateAssetInteractively();
                return;
            }

            PersistentStorage.SaveForScriptsReload(genericType, fileName);
            PersistentStorage.ExecuteOnScriptsReload(FinishSOCreation);

            ConcreteClassCreator<GenericScriptableObject>.CreateConcreteClass(genericTypeWithoutArgs, genericArgs);
            AssetDatabase.Refresh();
        }

        private static void CreateAssetInteractively()
        {
            // If CreateAssetInteractively is called too early, editor styles are not initialized yet and throw
            // NullReference exception. It usually takes one frame to initialize them, so it's not expensive to catch
            // the exception once and proceed to create the asset in the next frame.
            try
            {
                var _ = EditorStyles.toolbar;
            }
            catch (NullReferenceException)
            {
                return;
            }

            var asset = ScriptableObject.CreateInstance(_concreteType);

            try
            {
                ProjectWindowUtil.CreateAsset(asset, $"{_fileName}.asset");
            }
            catch (NullReferenceException)
            {
                Debug.LogError(
                    $"{nameof(CreateAssetInteractively)} was most likely called too early. " +
                    "Add it to EditorApplication.delayCall instead.");

                throw;
            }

            EditorApplication.update -= CreateAssetInteractively;
        }
    }
}