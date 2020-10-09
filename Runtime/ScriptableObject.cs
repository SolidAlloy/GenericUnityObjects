namespace GenericScriptableObjects
{
    using System;
    using System.ComponentModel;
    using UnityEngine;

    [TypeDescriptionProvider(typeof(GenericSODescriptionProvider))]
    public abstract class ScriptableObject<T> : GenericScriptableObject
    {
        public static ScriptableObject<T> Create()
        {
            if (GenericSODatabase.TryGetValue(typeof(T), out Type concreteType))
                return (ScriptableObject<T>) CreateInstance(concreteType);

            Debug.LogWarning($"There is no {nameof(ScriptableObject<T>)} derivative of type {typeof(T)}. Please add " +
                             "it to GenericDerivatives Database.");
            return null;
        }

        protected static void CreateAsset<TClass>()
        {
            // GenericScriptableObjectCreator.CreateAsset();
        }
    }

    public class GenericScriptableObject : ScriptableObject
    {
        public static ScriptableObject Create(Type type)
        {
            if (GenericSODatabase.TryGetValue(type, out Type concreteType)) // TODO: Replace with a dedicated database
                return CreateInstance(concreteType);

            throw new ArgumentOutOfRangeException($"There is no {nameof(GenericScriptableObject)} derivative of type " +
                                                  $"{type}. Please add it to GenericDerivatives Database.");
        }
    }
}