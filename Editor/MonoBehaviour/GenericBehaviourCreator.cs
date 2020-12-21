namespace GenericUnityObjects.Editor.MonoBehaviour
{
    using System;
    using NUnit.Framework;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;
    using Util;
    using Object = UnityEngine.Object;
    using TypeHelper = Util.TypeHelper;

    internal class GenericBehaviourCreator
    {
        public static void AddComponent(Type selectorComponentType, GameObject gameObject, Type genericTypeWithoutArgs, Type[] genericArgs)
        {
            var creator = new GenericBehaviourCreator(selectorComponentType, gameObject, genericTypeWithoutArgs, genericArgs);
            creator.AddComponent();
        }

        [DidReloadScripts]
        private static void OnScriptsReload()
        {
            if ( ! PersistentStorage.NeedsBehaviourCreation)
                return;

            try
            {
                (GameObject gameObject, Type genericType) =
                    PersistentStorage.GetGenericBehaviourDetails();

                Type concreteType = TypeHelper.GetEmptyTypeDerivedFrom(genericType);
                Assert.IsNotNull(concreteType);

                GenericObjectDatabase.Add(genericType, concreteType);
                gameObject.AddComponent(concreteType);
            }
            finally
            {
                PersistentStorage.Clear();
            }
        }

        private readonly Type _selectorComponentType;
        private readonly Type _genericTypeWithoutArgs;
        private readonly Type[] _genericArgs;
        private readonly GameObject _gameObject;

        private GenericBehaviourCreator(Type selectorComponentType, GameObject gameObject, Type genericTypeWithoutArgs, Type[] genericArgs)
        {
            _selectorComponentType = selectorComponentType;
            _gameObject = gameObject;
            _genericTypeWithoutArgs = genericTypeWithoutArgs;
            _genericArgs = genericArgs;
        }

        private void AddComponent()
        {
            if (GenericObjectDatabase.TryGetValue(_genericTypeWithoutArgs, _genericArgs, out Type concreteComponent))
            {
                AddConcreteComponent(concreteComponent);
                return;
            }

            PersistentStorage.SaveForAssemblyReload(_gameObject, _genericTypeWithoutArgs.MakeGenericType(_genericArgs));
            DestroySelectorComponent();

            ConcreteClassCreator.CreateConcreteClassAssembly(_genericTypeWithoutArgs, _genericArgs);
            AssetDatabase.Refresh();
        }

        private void AddConcreteComponent(Type concreteType)
        {
            DestroySelectorComponent();
            _gameObject.AddComponent(concreteType);
        }

        private void DestroySelectorComponent()
        {
            if (_gameObject.TryGetComponent(_selectorComponentType, out Component selectorComponent))
            {
                Object.DestroyImmediate(selectorComponent);
            }
        }
    }
}