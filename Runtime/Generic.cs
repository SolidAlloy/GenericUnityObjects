namespace GenericScriptableObjects
{
    using System;
    using UnityEngine;

    public class Generic<T> : Generic
    {
        public string StringField;
        public T Field;

        public static Generic<T> Create()
        {
            if (GenericDerivativesDatabase.TryGetValue(typeof(T), out Type concreteType))
                return (Generic<T>) CreateInstance(concreteType);

            Debug.LogWarning($"There is no {nameof(Generic<T>)} derivative of type {typeof(T)}. Please add " +
                             $"it to GenericDerivatives Database.");
            return null;
        }
    }

    public class Generic : ScriptableObject
    {
        public static ScriptableObject Create(Type type)
        {
            if (GenericDerivativesDatabase.TryGetValue(type, out Type concreteType))
                return CreateInstance(concreteType);

            Debug.LogWarning($"There is no {nameof(Generic)} derivative of type {type}. Please add " +
                             $"it to GenericDerivatives Database.");
            return null;
        }
    }
}