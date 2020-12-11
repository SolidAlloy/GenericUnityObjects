namespace GenericScriptableObjects
{
    using System;
    using System.Linq;
    using JetBrains.Annotations;
    using UnityEngine;

    /// <summary>
    /// Extensions that allow adding generic components to game objects.
    /// </summary>
    public static class GameObjectExtensions
    {
        [PublicAPI]
        public static Component AddGenericComponent(this GameObject gameObject, Type componentType)
        {
            return GenericObjectDatabase.TryGetValue(componentType, out Type concreteType)
                ? gameObject.AddComponent(concreteType)
                : LogFailure(componentType);
        }

        [PublicAPI]
        public static T AddGenericComponent<T>(this GameObject gameObject)
            where T : MonoBehaviour
        {
            return (T) gameObject.AddGenericComponent(typeof(T));
        }

        [PublicAPI]
        public static Component GetGenericComponent(this GameObject gameObject, Type componentType)
        {
            return GenericObjectDatabase.TryGetValue(componentType, out Type concreteType)
                ? gameObject.GetComponent(concreteType)
                : LogFailure(componentType);
        }

        [PublicAPI]
        public static T GetGenericComponent<T>(this GameObject gameObject)
            where T : MonoBehaviour
        {
            return (T) gameObject.GetGenericComponent(typeof(T));
        }

        [PublicAPI]
        public static Component GetGenericComponentInChildren(this GameObject gameObject, Type componentType,
            bool includeInactive = false)
        {
            return GenericObjectDatabase.TryGetValue(componentType, out Type concreteType)
                ? gameObject.GetComponentInChildren(concreteType, includeInactive)
                : LogFailure(componentType);
        }

        [PublicAPI]
        public static T GetGenericComponentInChildren<T>(this GameObject gameObject, bool includeInactive = false)
            where T : MonoBehaviour
        {
            return (T) gameObject.GetGenericComponentInChildren(typeof(T), includeInactive);
        }

        [PublicAPI]
        public static Component GetGenericComponentInParent(this GameObject gameObject, Type componentType)
        {
            return GenericObjectDatabase.TryGetValue(componentType, out Type concreteType)
                ? gameObject.GetComponentInParent(concreteType)
                : LogFailure(componentType);
        }

        [PublicAPI]
        public static T GetGenericComponentInParent<T>(this GameObject gameObject)
            where T : MonoBehaviour
        {
            return (T) gameObject.GetGenericComponentInParent(typeof(T));
        }

        [PublicAPI]
        public static Component[] GetGenericComponents(this GameObject gameObject, Type componentType)
        {
            return GenericObjectDatabase.TryGetValue(componentType, out Type concreteType)
                ? gameObject.GetComponents(concreteType)
                : LogFailureArray(componentType);
        }

        [PublicAPI]
        public static T[] GetGenericComponents<T>(this GameObject gameObject)
            where T : MonoBehaviour
        {
            return (T[]) gameObject.GetGenericComponents(typeof(T));
        }

        [PublicAPI]
        public static Component[] GetGenericComponentsInChildren(this GameObject gameObject, Type componentType,
            bool includeInactive = false)
        {
            return GenericObjectDatabase.TryGetValue(componentType, out Type concreteType)
                ? gameObject.GetComponentsInChildren(concreteType, includeInactive)
                : LogFailureArray(componentType);
        }

        [PublicAPI]
        public static T[] GetGenericComponentsInChildren<T>(this GameObject gameObject, bool includeInactive = false)
            where T : MonoBehaviour
        {
            return (T[]) gameObject.GetGenericComponentsInChildren(typeof(T), includeInactive);
        }

        [PublicAPI]
        public static Component[] GetGenericComponentsInParent(this GameObject gameObject, Type componentType,
            bool includeInactive = false)
        {
            return GenericObjectDatabase.TryGetValue(componentType, out Type concreteType)
                ? gameObject.GetComponentsInParent(concreteType, includeInactive)
                : LogFailureArray(componentType);
        }

        [PublicAPI]
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
            return null;
        }
    }
}