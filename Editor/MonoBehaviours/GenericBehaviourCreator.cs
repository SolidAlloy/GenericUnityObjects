namespace GenericUnityObjects.Editor.MonoBehaviours
{
    using System;
    using GenericUnityObjects.Util;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;
    using UnityEngine.Assertions;
    using Util;
    using Object = UnityEngine.Object;

    /// <summary>
    /// A class responsible for adding a generic component to game objects. If a concrete class is not generated it,
    /// it generates one, then adds a component after recompilation.
    /// </summary>
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

            PersistentStorage.SaveForScriptsReload(gameObject, genericType);
            PersistentStorage.ExecuteOnScriptsReload(FinishBehaviourCreation);

            DestroySelectorComponent(gameObject, selectorComponentType);

            ConcreteClassCreator<MonoBehaviour>.CreateConcreteClass(genericTypeWithoutArgs, genericArgs);
            AssetDatabase.Refresh();
        }

        private static void FinishBehaviourCreation()
        {
            try
            {
                (GameObject gameObject, Type genericType) = PersistentStorage.GetGenericBehaviourDetails();
                Type concreteType = BehavioursDatabase.GetConcreteType(genericType);
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