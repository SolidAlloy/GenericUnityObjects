namespace GenericUnityObjects.Editor.MonoBehaviours
{
    using System;
    using GenericUnityObjects.Util;
    using JetBrains.Annotations;
    using TypeReferences;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Events;
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

        [ContractAnnotation("=> null, reloadRequired: true; => notnull, reloadRequired: false")]
        public static Component AddComponent([CanBeNull] Type selectorComponentType, GameObject gameObject, Type genericTypeWithoutArgs, Type[] genericArgs, out bool reloadRequired)
        {
            Type genericType = genericTypeWithoutArgs.MakeGenericType(genericArgs);

            if (BehavioursDatabase.TryGetConcreteType(genericType, out Type concreteType))
            {
                DestroySelectorComponent(gameObject, selectorComponentType);
                reloadRequired = false;
                return Undo.AddComponent(gameObject, concreteType);
            }

            PersistentStorage.SaveData(GameObjectKey, gameObject);
            PersistentStorage.SaveData(GenericTypeKey, new TypeReference(genericType));
            PersistentStorage.ExecuteOnScriptsReload(FinishBehaviourCreation);

            DestroySelectorComponent(gameObject, selectorComponentType);

            ConcreteClassCreator<MonoBehaviour>.CreateConcreteClass(genericTypeWithoutArgs, genericArgs);
            reloadRequired = true;
            return null;
        }

        public static void AddComponent([CanBeNull] Type selectorComponentType, GameObject gameObject, Type genericTypeWithoutArgs, Type[] genericArgs)
        {
            AddComponent(selectorComponentType, gameObject, genericTypeWithoutArgs, genericArgs, out bool reloadRequired);

            if (reloadRequired)
                AssetDatabase.Refresh();
        }

        private static void FinishBehaviourCreation()
        {
            try
            {
                var gameObject = PersistentStorage.GetData<GameObject>(GameObjectKey);
                Type genericType = PersistentStorage.GetData<TypeReference>(GenericTypeKey).Type;

                Type concreteType = BehavioursDatabase.GetConcreteType(genericType);
                Undo.AddComponent(gameObject, concreteType);
            }
            finally
            {
                PersistentStorage.DeleteData(GameObjectKey);
                PersistentStorage.DeleteData(GenericTypeKey);
            }
        }

        private static void DestroySelectorComponent(GameObject gameObject, Type selectorComponentType)
        {
            if (selectorComponentType == null)
                return;

            if (gameObject.TryGetComponent(selectorComponentType, out Component selectorComponent))
            {
                Object.DestroyImmediate(selectorComponent);
            }
        }
    }
}