namespace GenericScriptableObjects
{
    using System;
    using System.Linq;
    using JetBrains.Annotations;
    using UnityEngine;

    public class GenericScriptableObject : ScriptableObject
    {
        [PublicAPI, CanBeNull]
        public static new GenericScriptableObject CreateInstance(Type genericSOType)
        {
            if (GenericSODatabase.TryGetValue(genericSOType, out Type concreteType))
                return (GenericScriptableObject) ScriptableObject.CreateInstance(concreteType);

            Debug.LogWarning($"There is no {genericSOType.GetGenericTypeDefinition()} derivative with type parameters " +
                             $"{string.Join(", ", genericSOType.GetGenericArguments().Select(type => type.Name))}. " +
                             "Please create an asset with such type parameters once to be able to create it from code.");

            return null;
        }

        [PublicAPI, CanBeNull]
        public static new TGenericSO CreateInstance<TGenericSO>()
            where TGenericSO : GenericScriptableObject
        {
            return (TGenericSO) CreateInstance(typeof(TGenericSO));
        }

        [PublicAPI, CanBeNull]
        public static GenericScriptableObject CreateInstance(Type genericSOTypeWithoutTypeParams, params Type[] paramTypes)
        {
            Type genericSOType = genericSOTypeWithoutTypeParams.MakeGenericType(paramTypes);
            return CreateInstance(genericSOType);
        }
    }
}