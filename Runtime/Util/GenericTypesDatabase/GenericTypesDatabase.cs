namespace GenericUnityObjects.Util
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Object = UnityEngine.Object;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    /// <summary>
    /// A database that holds references to generated concrete classes and provides them on demand when a generic class
    /// instance needs to be created.
    /// </summary>
    /// <typeparam name="TObject">
    /// A class derived from <see cref="UnityEngine.Object"/>. Currently supports only
    /// <see cref="ScriptableObject"/> and <see cref="MonoBehaviour"/>.
    /// </typeparam>
    internal abstract class GenericTypesDatabase<TObject> : SingletonScriptableObject<GenericTypesDatabase<TObject>>, ISerializationCallbackReceiver, ICanBeInitialized
    where TObject : Object
    {
        // TODO: replace with FastIterationDictionary
        private Dictionary<Type, Dictionary<Type[], Type>> _dict;

        [SerializeField] private SerializableType[] _keys;
        [SerializeField] private SerializableTypeDictionary[] _values;

        public static event Action<Type, Type> FoundNewType;

        public static void Initialize(Dictionary<Type, Dictionary<Type[], Type>> dict)
        {
            Instance.InitializeImpl(dict);
        }

        private void InitializeImpl(Dictionary<Type, Dictionary<Type[], Type>> dict)
        {
            _dict = dict;
            SetDirty();
        }

        public void OnBeforeSerialize()
        {
            if (_dict == null)
                return;

            int dictLength = _dict.Count;

            _keys = new SerializableType[dictLength];
            _values = new SerializableTypeDictionary[dictLength];

            int index = 0;

            foreach (var kvp in _dict)
            {
                _keys[index] = kvp.Key;
                _values[index] = kvp.Value;
                index++;
            }
        }

        // In Editor, the database is supplied with different types after each recompilation. This occurs in the GenericTypesAnalyzer class.
        // However, in a build types don't change and GenericTypesAnalyzer doesn't work, so a deserialization needs to be done.
        public void OnAfterDeserialize()
        {
#if ! UNITY_EDITOR
            if (_keys == null)
                return;

            int keysLength = _keys.Length;
            _dict = new Dictionary<Type, Dictionary<Type[], Type>>(keysLength);

            for (int i = 0; i < keysLength; i++)
            {
                Type key = _keys[i];
                Dictionary<Type[], Type> value = _values[i];

                if (key == null)
                    continue;

                _dict.Add(key, value);
            }
#endif
        }

        /// <summary>
        /// Searches for a <paramref name="concreteType"/> derived from the specified <paramref name="genericType"/> if one was generated.
        /// </summary>
        /// <param name="genericType">A generic type that needs to be instantiated.</param>
        /// <param name="concreteType">A generated concrete type that inherits from <paramref name="genericType"/>.</param>
        /// <returns><c>true</c> if a <paramref name="concreteType"/> was found.</returns>
        public static bool TryGetConcreteType(Type genericType, out Type concreteType)
        {
            return Instance.TryGetConcreteTypeImpl(genericType, out concreteType);
        }

        /// <summary>
        /// Searches for a concrete type derived from the specified <paramref name="genericType"/> if one was generated.
        /// </summary>
        /// <param name="genericType">A generic type that needs to be instantiated.</param>
        /// <returns>A generated concrete type that inherits from <paramref name="genericType"/>.</returns>
        public static Type GetConcreteType(Type genericType)
        {
            if (Instance.TryGetConcreteTypeImpl(genericType, out Type concreteType))
                return concreteType;

            throw new KeyNotFoundException($"A concrete type was not found for the generic type {genericType}");
        }

        private bool TryGetConcreteTypeImpl(Type genericType, out Type concreteType)
        {
            Type genericTypeWithoutArgs = genericType.GetGenericTypeDefinition();
            Type[] genericArgs = genericType.GetGenericArguments();

            if (_dict.TryGetValue(genericTypeWithoutArgs, out var concreteClassesDict)
                && concreteClassesDict.TryGetValue(genericArgs, out concreteType))
            {
                return true;
            }

            return TryGetEmptyDerivedType(genericType, out concreteType);
        }

        private bool TryGetEmptyDerivedType(Type genericType, out Type derivedType)
        {
            derivedType = TypeUtility.GetEmptyTypeDerivedDirectlyFrom(genericType);

            if (derivedType == null)
                return false;

            Add(genericType, derivedType);
            FoundNewType?.Invoke(genericType, derivedType);
            return true;
        }

        private void Add(Type genericType, Type concreteType)
        {
            Type genericTypeWithoutArgs = genericType.GetGenericTypeDefinition();
            Type[] genericArgs = genericType.GetGenericArguments();

            if (_dict.TryGetValue(genericTypeWithoutArgs, out var concreteClassesDict))
            {
                concreteClassesDict.Add(genericArgs, concreteType);
            }
            else
            {
                _dict.Add(
                    genericTypeWithoutArgs,
                    new Dictionary<Type[], Type>(default(TypeArrayComparer)) { { genericArgs, concreteType } });
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        void ICanBeInitialized.Initialize() { }
    }
}