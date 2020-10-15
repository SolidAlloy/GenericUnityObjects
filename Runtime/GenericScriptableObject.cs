namespace GenericScriptableObjects
{
    using System;
    using System.Linq;
    using JetBrains.Annotations;
    using UnityEngine;

    public class GenericScriptableObject : ScriptableObject
    {
        [PublicAPI, CanBeNull]
        public static new ScriptableObject CreateInstance(Type type)
        {
            if ( ! type.IsGenericType)
                return ScriptableObject.CreateInstance(type);

            if (GenericSODatabase.TryGetValue(type, out Type concreteType))
                return ScriptableObject.CreateInstance(concreteType);

            Debug.LogWarning($"There is no {type.GetGenericTypeDefinition()} derivative with type parameters " +
                             $"{string.Join(", ", type.GetGenericArguments().Select(typeParam => typeParam.Name))}. " +
                             "Please create an asset with such type parameters once to be able to create it from code.");

            return null;
        }

        [PublicAPI, CanBeNull]
        public static new T CreateInstance<T>()
            where T : GenericScriptableObject
        {
            return (T) CreateInstance(typeof(T));
        }

        [PublicAPI, CanBeNull]
        public static ScriptableObject CreateInstance(Type genericSOTypeWithoutTypeParams, params Type[] paramTypes)
        {
            Type genericSOType = genericSOTypeWithoutTypeParams.MakeGenericType(paramTypes);
            return CreateInstance(genericSOType);
        }
    }
}