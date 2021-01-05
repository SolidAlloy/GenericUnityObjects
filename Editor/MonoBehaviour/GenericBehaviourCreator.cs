namespace GenericUnityObjects.Editor.MonoBehaviour
{
    using System;
    using GeneratedTypesDatabase;
    using GenericUnityObjects.Util;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;
    using UnityEngine.Assertions;
    using Util;
    using Object = UnityEngine.Object;

    internal static class GenericBehaviourCreator
    {
        public static void AddComponent(Type selectorComponentType, GameObject gameObject, Type genericTypeWithoutArgs, Type[] genericArgs)
        {
            Type genericType = genericTypeWithoutArgs.MakeGenericType(genericArgs);

            if (BehavioursDatabase.TryGetConcreteType(genericType, out Type concreteComponent))
            {
                DestroySelectorComponent(gameObject, selectorComponentType);
                gameObject.AddComponent(concreteComponent);
                return;
            }

            PersistentStorage.SaveForAssemblyReload(gameObject, genericType);
            DestroySelectorComponent(gameObject, selectorComponentType);

            ConcreteClassCreator.CreateConcreteClassAssembly<BehavioursGenerationDatabase>(genericTypeWithoutArgs, genericArgs);
            AssetDatabase.Refresh();
        }

        [DidReloadScripts(Config.UnityObjectCreationOrder)]
        private static void OnScriptsReload()
        {
            if ( ! PersistentStorage.NeedsBehaviourCreation)
                return;

            try
            {
                (GameObject gameObject, Type genericType) =
                    PersistentStorage.GetGenericBehaviourDetails();

                bool success = BehavioursDatabase.TryGetConcreteType(genericType, out Type concreteType);
                Assert.IsTrue(success);
                gameObject.AddComponent(concreteType);
            }
            finally
            {
                PersistentStorage.Clear();
            }
        }

        private static void DestroySelectorComponent(GameObject gameObject, Type selectorComponentType)
        {
            if (gameObject.TryGetComponent(selectorComponentType, out Component selectorComponent))
            {
                Object.DestroyImmediate(selectorComponent);
            }
        }
    }
}