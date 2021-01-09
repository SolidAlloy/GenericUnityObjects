namespace GenericUnityObjects.Editor.ScriptableObject
{
    using System;
    using System.Linq;
    using GenericUnityObjects;
    using GenericUnityObjects.Util;
    using JetBrains.Annotations;
    using SolidUtilities.Editor.EditorWindows;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;
    using UnityEngine.Assertions;
    using Util;

    internal class GenericSOCreator
    {
        /// <summary>
        /// Creates a <see cref="GenericScriptableObject"/> asset when used in a method with the
        /// <see cref="UnityEditor.MenuItem"/> attribute. Use it in classes that derive from <see cref="GenericSOCreator"/>.
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

        [DidReloadScripts(Config.UnityObjectCreationOrder)]
        private static void OnScriptsReload()
        {
            if ( ! PersistentStorage.NeedsSOCreation)
                return;

            try
            {
                (Type genericSOType, string fileName) =
                    PersistentStorage.GetGenericSODetails();

                bool success = ScriptableObjectsDatabase.TryGetConcreteType(genericSOType, out Type concreteType);
                Assert.IsTrue(success);
                CreateAssetInteractively(concreteType, fileName);
            }
            finally
            {
                PersistentStorage.Clear();
            }
        }

        private static void CreateAsset(Type genericTypeWithoutArgs, Type[] genericArgs, string fileName)
        {
            Type genericType = genericTypeWithoutArgs.MakeGenericType(genericArgs);

            if (ScriptableObjectsDatabase.TryGetConcreteType(genericType, out Type concreteComponent))
            {
                CreateAssetInteractively(concreteComponent, fileName);
                return;
            }

            PersistentStorage.SaveForAssemblyReload(genericType, fileName);

            ConcreteClassCreator<GenericScriptableObject>.CreateConcreteClass(genericTypeWithoutArgs, genericArgs);
            AssetDatabase.Refresh();
        }

        private static void CreateAssetInteractively(Type concreteType, string fileName)
        {
            var asset = ScriptableObject.CreateInstance(concreteType);
            AssetCreator.Create(asset, $"{fileName}.asset");
        }
    }
}