namespace GenericUnityObjects
{
    using System;
    using System.Linq;
    using JetBrains.Annotations;
    using UnityEngine;
    using Util;

    /// <summary>
    /// Inherit from this class to create a generic ScriptableObject.
    /// Add a menu to create assets using <see cref="CreateGenericAssetMenuAttribute"/>.
    /// </summary>
    [Serializable]
    public class GenericScriptableObject : ScriptableObject
    {
        /// <summary>Creates an instance of <see cref="ScriptableObject"/>. Has support for generic scriptable objects.</summary>
        /// <param name="type">Type derived from <see cref="ScriptableObject"/>.</param>
        /// <returns>Instance of <paramref name="type"/>.</returns>
        /// <exception cref="NotSupportedException">
        /// If an asset with such generic arguments was not created through the Assets/Create menu yet, and is
        /// instantiated in an IL2CPP build.
        /// </exception>
        [PublicAPI, Pure]
        public static new ScriptableObject CreateInstance(Type type)
        {
            if ( ! type.IsGenericType)
                return ScriptableObject.CreateInstance(type);

            if (ScriptableObjectsDatabase.TryGetConcreteType(type, out Type concreteType))
                return ScriptableObject.CreateInstance(concreteType);

            concreteType = GeneratedUnityObjectCache.GetSOClass(type);

            if (concreteType != null)
                return ScriptableObject.CreateInstance(concreteType);

            throw new NotSupportedException(
                $"There is no {type.GetGenericTypeDefinition()} derivative with type parameters " +
                $"{string.Join(", ", type.GetGenericArguments().Select(typeParam => typeParam.Name))} " +
                "and a type cannot be created dynamically in an IL2CPP build. " +
                "Please create an asset with such type parameters once to be able to create it at runtime.");
        }

        /// <summary>
        /// Creates an instance of <typeparamref name="T"/>. Has support for generic scriptable objects.
        /// </summary>
        /// <typeparam name="T">Type derived from ScriptableObject.</typeparam>
        /// <returns>Instance of <typeparamref name="T"/>>.</returns>
        /// <exception cref="NotSupportedException">
        /// If an asset with such generic arguments was not created through the Assets/Create menu yet, and is
        /// instantiated in an IL2CPP build.
        /// </exception>
        [PublicAPI, Pure]
        public static new T CreateInstance<T>()
            where T : ScriptableObject
        {
            return (T) CreateInstance(typeof(T));
        }
    }
}