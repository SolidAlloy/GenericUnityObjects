namespace GenericScriptableObjects
{
    using System;
    using System.Linq;
    using JetBrains.Annotations;
    using UnityEngine;

    /// <summary>
    /// Inherit from this class to create a generic ScriptableObject.
    /// Add an AssetCreate menu using <see cref="GenericSOCreator"/>.
    /// </summary>
    public class GenericScriptableObject : ScriptableObject
    {
        /// <summary>Creates an instance of <see cref="ScriptableObject"/>. Has support for generic scriptable objects.</summary>
        /// <param name="type">Type derived from <see cref="ScriptableObject"/>.</param>
        /// <returns>Instance of <paramref name="type"/>.</returns>
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

        /// <summary>
        /// Creates an instance of <typeparamref name="T"/>. Has support for generic scriptable objects.
        /// </summary>
        /// <typeparam name="T">Type derived from ScriptableObject.</typeparam>
        /// <returns>Instance of <typeparamref name="T"/>>.</returns>
        [PublicAPI, CanBeNull]
        public static new T CreateInstance<T>()
            where T : ScriptableObject
        {
            return (T) CreateInstance(typeof(T));
        }

        /// <summary>
        /// Creates an instance of <see cref="GenericScriptableObject"/> using a generic type definition
        /// (generic type without generic argument types) and an array of generic argument types.
        /// </summary>
        /// <param name="genericTypeWithoutTypeParams">
        /// Generic type derived from <see cref="GenericScriptableObject"/> without generic argument types (e.g. Generic&lt;,>).
        /// </param>
        /// <param name="paramTypes">Types of the generic arguments.</param>
        /// <returns>
        /// Instance of <paramref name="genericTypeWithoutTypeParams"/> with param types <paramref name="paramTypes"/>>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the number of generic argument types passed into the method is not equal to the number of generic
        /// arguments the type can take.
        /// </exception>
        [PublicAPI, CanBeNull]
        public static GenericScriptableObject CreateInstance(Type genericTypeWithoutTypeParams, params Type[] paramTypes)
        {
            int genericArgCount = genericTypeWithoutTypeParams.GetGenericArguments().Length;
            int paramTypesCount = paramTypes.Length;

            if (genericArgCount != paramTypesCount)
            {
                throw new ArgumentException(
                    $"Number of generic argument types ({paramTypesCount}) is not equal to the number of " +
                    $"generic arguments the type can take ({genericArgCount}).", nameof(genericTypeWithoutTypeParams));
            }

            Type genericSOType = genericTypeWithoutTypeParams.MakeGenericType(paramTypes);
            return (GenericScriptableObject) CreateInstance(genericSOType);
        }
    }
}