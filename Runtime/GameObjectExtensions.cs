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
        private static bool TryGetConcreteType(Type genericTypeWithArgs, out Type concreteType)
        {
            if (BehavioursDatabase.TryGetConcreteType(genericTypeWithArgs, out concreteType))
                return true;

            concreteType = BehaviourCache.GetClass(genericTypeWithArgs);
            return concreteType != null;
        }

        /// <summary>
        /// Adds a component class of the generic type <paramref name="componentType"/> to <paramref name="gameObject"/>.
        /// </summary>
        /// <param name="gameObject">The game object that needs to obtain the component.</param>
        /// <param name="componentType">The type of component to add.</param>
        /// <returns>The added component.</returns>
        [PublicAPI, CanBeNull]
        public static Component AddGenericComponent(this GameObject gameObject, Type componentType)
        {
            return TryGetConcreteType(componentType, out Type concreteType)
                ? gameObject.AddComponent(concreteType)
                : LogFailure(componentType);
        }

        /// <summary>
        /// Adds a component class of the generic type <typeparamref name="T"/> to <paramref name="gameObject"/>.
        /// </summary>
        /// <param name="gameObject">The game object that needs to obtain the component.</param>
        /// <typeparam name="T">The type of component to add.</typeparam>
        /// <returns>The added component.</returns>
        [PublicAPI, CanBeNull]
        public static T AddGenericComponent<T>(this GameObject gameObject)
            where T : MonoBehaviour
        {
            return (T) gameObject.AddGenericComponent(typeof(T));
        }

        /// <summary>
        /// Returns the component of type <paramref name="componentType"/> if <paramref name="gameObject"/> has one attached,
        /// <c>null</c> if it doesn't.
        /// </summary>
        /// <param name="gameObject">The game object to check.</param>
        /// <param name="componentType">The type of component to retrieve.</param>
        /// <returns>The component of type <paramref name="componentType"/> or <c>null</c>.</returns>
        [PublicAPI, CanBeNull]
        public static Component GetGenericComponent(this GameObject gameObject, Type componentType)
        {
            return TryGetConcreteType(componentType, out Type concreteType)
                ? gameObject.GetComponent(concreteType)
                : LogFailure(componentType);
        }

        /// <summary>
        /// Returns the component of type <typeparamref name="T"/> if <paramref name="gameObject"/> has one attached,
        /// null if it doesn't.
        /// </summary>
        /// <param name="gameObject">The game object to check.</param>
        /// <typeparam name="T">The type of component to retrieve.</typeparam>
        /// <returns>The found component or <c>null</c>.</returns>
        [PublicAPI, CanBeNull]
        public static T GetGenericComponent<T>(this GameObject gameObject)
            where T : MonoBehaviour
        {
            return (T) gameObject.GetGenericComponent(typeof(T));
        }

        /// <summary>
        /// Returns the component of type <paramref name="componentType"/> in <paramref name="gameObject"/> or any of its children
        /// using depth first search.
        /// </summary>
        /// <param name="gameObject">The game object to check.</param>
        /// <param name="componentType">The type of Component to retrieve.</param>
        /// <param name="includeInactive">The type of component to retrieve.</param>
        /// <returns>A component of the matching type, if found.</returns>
        [PublicAPI, CanBeNull]
        public static Component GetGenericComponentInChildren(this GameObject gameObject, Type componentType,
            bool includeInactive = false)
        {
            return TryGetConcreteType(componentType, out Type concreteType)
                ? gameObject.GetComponentInChildren(concreteType, includeInactive)
                : LogFailure(componentType);
        }

        /// <summary>
        /// Returns the component of type <typeparamref name="T"/> in <paramref name="gameObject"/> or any of its children using
        /// depth first search.
        /// </summary>
        /// <param name="gameObject">The game object to check.</param>
        /// <param name="includeInactive">Whether to include inactive children in the search.</param>
        /// <typeparam name="T">The type of component to retrieve.</typeparam>
        /// <returns>A component of the matching type, if found.</returns>
        [PublicAPI, CanBeNull]
        public static T GetGenericComponentInChildren<T>(this GameObject gameObject, bool includeInactive = false)
            where T : MonoBehaviour
        {
            return (T) gameObject.GetGenericComponentInChildren(typeof(T), includeInactive);
        }

        /// <summary>
        /// Retrieves the component of type <paramref name="componentType"/> in <paramref name="gameObject"/> or any of its parents.
        /// </summary>
        /// <param name="gameObject">The game object to check.</param>
        /// <param name="componentType">Type of the component to find.</param>
        /// <param name="includeInactive">Whether to include inactive parents in the search.</param>
        /// <returns>A component if a component matching the type is found. Returns null otherwise.</returns>
        [PublicAPI, CanBeNull]
        public static Component GetGenericComponentInParent(this GameObject gameObject, Type componentType, bool includeInactive = false)
        {
            return TryGetConcreteType(componentType, out Type concreteType)
                ? gameObject.GetComponentInParent(concreteType, includeInactive)
                : LogFailure(componentType);
        }

        /// <summary>
        /// Retrieves the component of type <typeparamref name="T"/> in <paramref name="gameObject"/> or any of its parents.
        /// </summary>
        /// <param name="gameObject">The game object to check.</param>
        /// <param name="includeInactive">Whether to include inactive parents in the search.</param>
        /// <typeparam name="T">The type of component to retrieve.</typeparam>
        /// <returns>A component if a component matching the type is found. Returns null otherwise.</returns>
        [PublicAPI, CanBeNull]
        public static T GetGenericComponentInParent<T>(this GameObject gameObject, bool includeInactive)
            where T : MonoBehaviour
        {
            return (T) gameObject.GetGenericComponentInParent(typeof(T), includeInactive);
        }

        /// <summary> Returns all components of type <paramref name="componentType"/> in <paramref name="gameObject"/>. </summary>
        /// <param name="gameObject">The game object to check.</param>
        /// <param name="componentType">The type of components to retrieve.</param>
        /// <returns>Found components of type <paramref name="componentType"/>>.</returns>
        [PublicAPI, NotNull]
        public static Component[] GetGenericComponents(this GameObject gameObject, Type componentType)
        {
            return TryGetConcreteType(componentType, out Type concreteType)
                ? gameObject.GetComponents(concreteType)
                : LogFailureArray(componentType);
        }

        /// <summary> Returns all components of type <typeparamref name="T"/> in <paramref name="gameObject"/>. </summary>
        /// <param name="gameObject">The game object to check.</param>
        /// <typeparam name="T">The type of components to retrieve.</typeparam>
        /// <returns>Found components.</returns>
        [PublicAPI, NotNull]
        public static T[] GetGenericComponents<T>(this GameObject gameObject)
            where T : MonoBehaviour
        {
            return (T[]) gameObject.GetGenericComponents(typeof(T));
        }

        /// <summary>
        /// Returns all components of type <paramref name="componentType"/> in <paramref name="gameObject"/> or any of its children.
        /// </summary>
        /// <param name="gameObject">The game object to check.</param>
        /// <param name="componentType">The type of components to retrieve.</param>
        /// <param name="includeInactive">Whether components on inactive GameObjects should be included in the found set.</param>
        /// <returns>Found components of type <paramref name="componentType"/>>.</returns>
        [PublicAPI, NotNull]
        public static Component[] GetGenericComponentsInChildren(this GameObject gameObject, Type componentType,
            bool includeInactive = false)
        {
            return TryGetConcreteType(componentType, out Type concreteType)
                ? gameObject.GetComponentsInChildren(concreteType, includeInactive)
                : LogFailureArray(componentType);
        }

        /// <summary>
        /// Returns all components of type <typeparamref name="T"/> in <paramref name="gameObject"/> or any of its children.
        /// </summary>
        /// <param name="gameObject">The game object to check.</param>
        /// <param name="includeInactive">Whether components on inactive GameObjects should be included in the found set.</param>
        /// <typeparam name="T">The type of components to retrieve.</typeparam>
        /// <returns>Found components.</returns>
        [PublicAPI, NotNull]
        public static T[] GetGenericComponentsInChildren<T>(this GameObject gameObject, bool includeInactive = false)
            where T : MonoBehaviour
        {
            return (T[]) gameObject.GetGenericComponentsInChildren(typeof(T), includeInactive);
        }

        /// <summary>
        /// Returns all components of type <paramref name="componentType"/> in <paramref name="gameObject"/> or any of its parents.
        /// </summary>
        /// <param name="gameObject">The game object to check.</param>
        /// <param name="componentType">The type of components to retrieve.</param>
        /// <param name="includeInactive">Whether components on inactive GameObjects should be included in the found set.</param>
        /// <returns>Found components of type <paramref name="componentType"/>>.</returns>
        [PublicAPI, NotNull]
        public static Component[] GetGenericComponentsInParent(this GameObject gameObject, Type componentType,
            bool includeInactive = false)
        {
            return TryGetConcreteType(componentType, out Type concreteType)
                ? gameObject.GetComponentsInParent(concreteType, includeInactive)
                : LogFailureArray(componentType);
        }

        /// <summary>
        /// Returns all components of type <typeparamref name="T"/> in <paramref name="gameObject"/> or any of its parents.
        /// </summary>
        /// <param name="gameObject">The game object to check.</param>
        /// <param name="includeInactive">Whether components on inactive GameObjects should be included in the found set.</param>
        /// <typeparam name="T">The type of components to retrieve.</typeparam>
        /// <returns>Found components.</returns>
        [PublicAPI, NotNull]
        public static T[] GetGenericComponentsInParent<T>(this GameObject gameObject, bool includeInactive = false)
            where T : MonoBehaviour
        {
            return (T[]) gameObject.GetGenericComponentsInParent(typeof(T), includeInactive);
        }

        private static Component LogFailure(Type componentType)
        {
            Debug.LogWarning($"There is no {componentType.GetGenericTypeDefinition()} derivative with type parameters " +
                             $"{string.Join(", ", componentType.GetGenericArguments().Select(typeParam => typeParam.Name))} " +
                             "and a type cannot be created dynamically in an IL2CPP build. " +
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