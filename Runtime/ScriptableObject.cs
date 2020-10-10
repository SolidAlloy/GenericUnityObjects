namespace GenericScriptableObjects
{
    using System;
    using System.CodeDom;
    using System.ComponentModel;
    using UnityEngine;

    public abstract class ScriptableObject<T> : GenericScriptableObject
    {
        public static ScriptableObject<T> Create(Type genericSOType)
        {
            if (GenericSODatabase.TryGetValue(genericSOType, typeof(T), out Type concreteType))
                return (ScriptableObject<T>) CreateInstance(concreteType);

            Debug.LogWarning($"There is no {nameof(ScriptableObject<T>)} derivative of type {typeof(T)}. Please add " +
                             "it to GenericDerivatives Database."); // TODO: change the message.
            return null;
        }
    }

    public class GenericScriptableObject : ScriptableObject
    {
        public static ScriptableObject Create(Type genericSOType, Type type)
        {
            if (GenericSODatabase.TryGetValue(genericSOType, type, out Type concreteType)) // TODO: Replace with a dedicated database
                return CreateInstance(concreteType);

            throw new ArgumentOutOfRangeException($"There is no {nameof(GenericScriptableObject)} derivative of type " +
                                                  $"{type}. Please add it to GenericDerivatives Database.");
        }
    }
}