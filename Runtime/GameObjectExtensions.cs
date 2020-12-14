namespace GenericUnityObjects
{
    using System;
    using System.Linq;
    using JetBrains.Annotations;
    using UnityEngine;
    using Util;

    /// <summary>
    /// Extensions that allow adding generic components to game objects.
    /// </summary>
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Adds a component class of the generic type <paramref name="componentType"/> to the game object.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="componentType"></param>
        [PublicAPI, CanBeNull]
        public static Component AddGenericComponent(this GameObject gameObject, Type componentType)
        {
            return GenericObjectDatabase.TryGetValue(componentType, out Type concreteType)
                ? gameObject.AddComponent(concreteType)
                : LogFailure(componentType);
        }

        /// <summary>
        /// Adds a component class of the generic type <typeparamref name="T"/> to the game object.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [PublicAPI, CanBeNull]
        public static T AddGenericComponent<T>(this GameObject gameObject)
            where T : MonoBehaviour
        {
            return (T) gameObject.AddGenericComponent(typeof(T));
        }

        /// <summary>
        /// Returns the component of type <paramref name="componentType"/> if the game object has one attached,
        /// null if it doesn't.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="componentType">The type of component to retrieve.</param>
        [PublicAPI, CanBeNull]
        public static Component GetGenericComponent(this GameObject gameObject, Type componentType)
        {
            return GenericObjectDatabase.TryGetValue(componentType, out Type concreteType)
                ? gameObject.GetComponent(concreteType)
                : LogFailure(componentType);
        }

        /// <summary>
        /// Returns the component of type <typeparamref name="T"/> if the game object has one attached,
        /// null if it doesn't.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [PublicAPI, CanBeNull]
        public static T GetGenericComponent<T>(this GameObject gameObject)
            where T : MonoBehaviour
        {
            return (T) gameObject.GetGenericComponent(typeof(T));
        }

        /// <summary>
        /// Returns the component of type <paramref name="componentType"/> in the GameObject or any of its children
        /// using depth first search.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="componentType">The type of Component to retrieve.</param>
        /// <param name="includeInactive"></param>
        /// <returns>A component of the matching type, if found.</returns>
        [PublicAPI, CanBeNull]
        public static Component GetGenericComponentInChildren(this GameObject gameObject, Type componentType,
            bool includeInactive = false)
        {
            return GenericObjectDatabase.TryGetValue(componentType, out Type concreteType)
                ? gameObject.GetComponentInChildren(concreteType, includeInactive)
                : LogFailure(componentType);
        }

        /// <summary>
        /// Returns the component of type <typeparamref name="T"/> in the GameObject or any of its children using
        /// depth first search.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="includeInactive"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns>A component of the matching type, if found.</returns>
        [PublicAPI, CanBeNull]
        public static T GetGenericComponentInChildren<T>(this GameObject gameObject, bool includeInactive = false)
            where T : MonoBehaviour
        {
            return (T) gameObject.GetGenericComponentInChildren(typeof(T), includeInactive);
        }

        /// <summary>
        /// Retrieves the component of type <paramref name="componentType"/> in the GameObject or any of its parents.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="componentType">Type of the component to find.</param>
        /// <param name="includeInactive"></param>
        /// <returns>Returns a component if a component matching the type is found. Returns null otherwise.</returns>
        [PublicAPI, CanBeNull]
        public static Component GetGenericComponentInParent(this GameObject gameObject, Type componentType, bool includeInactive = false)
        {
            return GenericObjectDatabase.TryGetValue(componentType, out Type concreteType)
                ? gameObject.GetComponentInParent(concreteType, includeInactive)
                : LogFailure(componentType);
        }

        /// <summary>
        /// Retrieves the component of type <typeparamref name="T"/> in the GameObject or any of its parents.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="includeInactive"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns>Returns a component if a component matching the type is found. Returns null otherwise.</returns>
        [PublicAPI, CanBeNull]
        public static T GetGenericComponentInParent<T>(this GameObject gameObject, bool includeInactive)
            where T : MonoBehaviour
        {
            return (T) gameObject.GetGenericComponentInParent(typeof(T), includeInactive);
        }

        /// <summary> Returns all components of type <paramref name="componentType"/> in the GameObject. </summary>
        /// <param name="gameObject"></param>
        /// <param name="componentType">The type of component to retrieve.</param>
        /// <returns></returns>
        [PublicAPI, NotNull]
        public static Component[] GetGenericComponents(this GameObject gameObject, Type componentType)
        {
            return GenericObjectDatabase.TryGetValue(componentType, out Type concreteType)
                ? gameObject.GetComponents(concreteType)
                : LogFailureArray(componentType);
        }

        /// <summary> Returns all components of type <typeparamref name="T"/> in the GameObject. </summary>
        /// <param name="gameObject"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [PublicAPI, NotNull]
        public static T[] GetGenericComponents<T>(this GameObject gameObject)
            where T : MonoBehaviour
        {
            return (T[]) gameObject.GetGenericComponents(typeof(T));
        }

        /// <summary> Returns all components of type <paramref name="componentType"/> in the GameObject or any of its children. </summary>
        /// <param name="gameObject"></param>
        /// <param name="componentType">The type of Component to retrieve.</param>
        /// <param name="includeInactive">Whether components on inactive GameObjects should be included in the found set.</param>
        [PublicAPI, NotNull]
        public static Component[] GetGenericComponentsInChildren(this GameObject gameObject, Type componentType,
            bool includeInactive = false)
        {
            return GenericObjectDatabase.TryGetValue(componentType, out Type concreteType)
                ? gameObject.GetComponentsInChildren(concreteType, includeInactive)
                : LogFailureArray(componentType);
        }

        /// <summary> Returns all components of type <typeparamref name="T"/> in the GameObject or any of its children. </summary>
        /// <param name="gameObject"></param>
        /// <param name="includeInactive">Whether components on inactive GameObjects should be included in the found set.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [PublicAPI, NotNull]
        public static T[] GetGenericComponentsInChildren<T>(this GameObject gameObject, bool includeInactive = false)
            where T : MonoBehaviour
        {
            return (T[]) gameObject.GetGenericComponentsInChildren(typeof(T), includeInactive);
        }

        /// <summary>
        /// Returns all components of type <paramref name="componentType"/> in the GameObject or any of its parents.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="componentType">The type of Component to retrieve.</param>
        /// <param name="includeInactive">Whether components on inactive GameObjects should be included in the found set.</param>
        /// <returns></returns>
        [PublicAPI, NotNull]
        public static Component[] GetGenericComponentsInParent(this GameObject gameObject, Type componentType,
            bool includeInactive = false)
        {
            return GenericObjectDatabase.TryGetValue(componentType, out Type concreteType)
                ? gameObject.GetComponentsInParent(concreteType, includeInactive)
                : LogFailureArray(componentType);
        }

        /// <summary>
        /// Returns all components of type <typeparamref name="T"/> in the GameObject or any of its parents.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="includeInactive">Whether components on inactive GameObjects should be included in the found set.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [PublicAPI, NotNull]
        public static T[] GetGenericComponentsInParent<T>(this GameObject gameObject, bool includeInactive = false)
            where T : MonoBehaviour
        {
            return (T[]) gameObject.GetGenericComponentsInParent(typeof(T), includeInactive);
        }

        private static Component LogFailure(Type componentType)
        {
            Debug.LogWarning($"There is no {componentType.GetGenericTypeDefinition()} derivative with type parameters " +
                             $"{string.Join(", ", componentType.GetGenericArguments().Select(typeParam => typeParam.Name))}. " +
                             "Please add a component with such type parameters in inspector once to be able to use it in code.");

            return null;
        }

        private static Component[] LogFailureArray(Type componentType)
        {
            LogFailure(componentType);
            return new Component[] { };
        }
    }
}