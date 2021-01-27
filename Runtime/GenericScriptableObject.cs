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
        [PublicAPI, CanBeNull, Pure]
        public static new ScriptableObject CreateInstance(Type type)
        {
            var instance = TryCreateInstance(type);

            if ( ! (instance is null))
                return instance;

            Debug.LogWarning(GetErrorText(type));
            return null;
        }

        /// <summary>
        /// Creates an instance of <typeparamref name="T"/>. Has support for generic scriptable objects.
        /// </summary>
        /// <typeparam name="T">Type derived from ScriptableObject.</typeparam>
        /// <returns>Instance of <typeparamref name="T"/>>.</returns>
        [PublicAPI, CanBeNull, Pure]
        public static new T CreateInstance<T>()
            where T : ScriptableObject
        {
            return (T) CreateInstance(typeof(T));
        }

        public static ScriptableObject CreateInstanceStrict(Type type)
        {
            ScriptableObject instance = TryCreateInstance(type);

            // ReSharper disable once Unity.NoNullCoalescing
            return instance ?? throw new NotSupportedException(GetErrorText(type));
        }

        public static T CreateInstanceStrict<T>() where T : ScriptableObject
        {
            return (T) CreateInstanceStrict(typeof(T));
        }

        private static string GetErrorText(Type type)
        {
            return $"There is no {type.GetGenericTypeDefinition()} derivative with type parameters " +
                   $"{string.Join(", ", type.GetGenericArguments().Select(typeParam => typeParam.Name))} " +
                   "and a type cannot be created dynamically in an IL2CPP build. " +
                   "Please create an asset with such type parameters once to be able to create it from code.";
        }

        [CanBeNull]
        private static ScriptableObject TryCreateInstance(Type type)
        {
            if ( ! type.IsGenericType)
                return ScriptableObject.CreateInstance(type);

            if (ScriptableObjectsDatabase.TryGetConcreteType(type, out Type concreteType))
                return ScriptableObject.CreateInstance(concreteType);

            concreteType = GeneratedUnityObjectCache.GetSOClass(type);
            return concreteType != null ? ScriptableObject.CreateInstance(concreteType) : null;
        }
    }
}