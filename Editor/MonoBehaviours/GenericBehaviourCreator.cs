namespace GenericUnityObjects.Editor.MonoBehaviours
{
    using System;
    using GenericUnityObjects.Util;
    using TypeReferences;
    using UnityEditor;
    using UnityEngine;
    using Util;
    using Object = UnityEngine.Object;

    /// <summary>
    /// A class responsible for adding a generic component to game objects. If a concrete class is not generated it,
    /// it generates one, then adds a component after recompilation.
    /// </summary>
    internal static class GenericBehaviourCreator
    {
        private const string GameObjectKey = "GameObject";
        private const string GenericTypeKey = "GenericBehaviourType";

        public static void AddComponent(Type selectorComponentType, GameObject gameObject, Type genericTypeWithoutArgs, Type[] genericArgs)
        {
            Type genericType = genericTypeWithoutArgs.MakeGenericType(genericArgs);

            if (BehavioursDatabase.TryGetConcreteType(genericType, out Type concreteComponent))
            {
                DestroySelectorComponent(gameObject, selectorComponentType);
                gameObject.AddComponent(concreteComponent);
                return;
            }

            PersistentStorage.SaveData(GameObjectKey, gameObject);
            PersistentStorage.SaveData(GenericTypeKey, new TypeReference(genericType));
            PersistentStorage.ExecuteOnScriptsReload(FinishBehaviourCreation);

            DestroySelectorComponent(gameObject, selectorComponentType);

            ConcreteClassCreator<MonoBehaviour>.CreateConcreteClass(genericTypeWithoutArgs, genericArgs);
            AssetDatabase.Refresh();
        }

        private static void FinishBehaviourCreation()
        {
            try
            {
                var gameObject = PersistentStorage.GetData<GameObject>(GameObjectKey);
                Type genericType = PersistentStorage.GetData<TypeReference>(GenericTypeKey).Type;

                Type concreteType = BehavioursDatabase.GetConcreteType(genericType);
                gameObject.AddComponent(concreteType);
            }
            finally
            {
                PersistentStorage.DeleteData(GameObjectKey);
                PersistentStorage.DeleteData(GenericTypeKey);
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