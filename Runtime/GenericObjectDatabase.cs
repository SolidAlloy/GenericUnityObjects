namespace GenericScriptableObjects
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using TypeReferences;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;
    using UnityEngine.Assertions;
    using Util;
    using Debug = UnityEngine.Debug;

    /// <summary>
    /// A database of all the type parameters of generic scriptable objects and their matching concrete implementations.
    /// When a new GenericScriptableObject asset is created through Unity context menu, a concrete implementation is
    /// created and added to this dictionary.
    /// <example>
    /// CustomGeneric&lt;T>
    ///     bool --- CustomGeneric_Boolean
    ///     int  --- CustomGeneric_Int32
    /// CustomGeneric&lt;T1,T2>
    ///     bool, int --- CustomGeneric_Boolean_Int32
    ///     bool, float --- CustomGeneric_Boolean_Single
    /// </example>
    /// </summary>
    internal class GenericObjectDatabase :
        SingletonScriptableObject<GenericObjectDatabase>,
        ISerializationCallbackReceiver
    {
        private readonly Dictionary<TypeReference, TypeDictionary> _dict =
            new Dictionary<TypeReference, TypeDictionary>(new TypeReferenceComparer());

        [HideInInspector]
        [SerializeField] private TypeReference[] _keys;

        [HideInInspector]
        [SerializeField] private TypeDictionary[] _values;

        private bool _shouldSetDirty;

        public static void Add(Type genericType, Type value)
        {
            var paramTypes = genericType.GetGenericArguments();
            Assert.IsFalse(paramTypes.Length == 0);
            Type genericTypeWithoutParams = genericType.GetGenericTypeDefinition();
            Add(genericTypeWithoutParams, paramTypes, value);
        }

        public static void Add(Type genericTypeWithoutArgs, Type[] key, Type value)
        {
            Assert.IsTrue(genericTypeWithoutArgs.IsGenericTypeDefinition);
            TypeDictionary assetDict = GetAssetDict(genericTypeWithoutArgs);
            assetDict.Add(key, value);
            SetInstanceDirty();
        }

        public static bool ContainsKey(Type genericType, Type[] key)
        {
            Assert.IsTrue(genericType.IsGenericTypeDefinition);
            TypeDictionary assetDict = GetAssetDict(genericType);
            return assetDict.ContainsKey(key);
        }

        public static bool TryGetValue(Type genericType, out Type value)
        {
            var paramTypes = genericType.GetGenericArguments();
            Assert.IsFalse(paramTypes.Length == 0);
            Type genericTypeWithoutParams = genericType.GetGenericTypeDefinition();
            return TryGetValue(genericTypeWithoutParams, paramTypes, out value);
        }

        public static bool TryGetValue(Type genericTypeWithoutArgs, Type[] genericArgs, out Type value)
        {
            Assert.IsTrue(genericTypeWithoutArgs.IsGenericTypeDefinition);
            TypeDictionary assetDict = GetAssetDict(genericTypeWithoutArgs);
            bool result = assetDict.TryGetValue(genericArgs, out value);
            return result;
        }

        public void OnAfterDeserialize()
        {
            int keysLength = _keys.Length;
            int valuesLength = _values.Length;

            if (keysLength != valuesLength)
            {
                Debug.LogError($"Something wrong happened in the database. Keys count ({keysLength}) does " +
                               $"not equal to values count ({valuesLength}). The database will be cleaned up.");
                _shouldSetDirty = true;
                return;
            }

            Assert.IsTrue(_dict.Count == 0);

            for (int i = 0; i < keysLength; ++i)
            {
                TypeReference typeRef = _keys[i];

                if (typeRef.TypeIsMissing())
                    continue;

                _dict[typeRef] = _values[i];
            }

            if (_dict.Count != keysLength)
                _shouldSetDirty = true;
        }

        public void OnBeforeSerialize()
        {
            int dictLength = _dict.Count;

            _keys = new TypeReference[dictLength];
            _values = new TypeDictionary[dictLength];

            int keysIndex = 0;
            foreach (var pair in _dict)
            {
                _keys[keysIndex] = pair.Key;
                _values[keysIndex] = pair.Value;
                ++keysIndex;
            }
        }

        private static TypeDictionary GetAssetDict(Type genericType)
        {
            if (Instance._dict.TryGetValue(genericType, out TypeDictionary assetDict))
                return assetDict;

            assetDict = new TypeDictionary();
            Instance._dict.Add(new TypeReference(genericType, true), assetDict);
            SetInstanceDirty();
            return assetDict;
        }

        private void OnEnable()
        {
            if (_shouldSetDirty)
            {
                _shouldSetDirty = false;
                SetDirty();
            }
        }

        [Conditional("UNITY_EDITOR")]
        private new void SetDirty()
        {
            EditorUtility.SetDirty(this);
        }

        [Conditional("UNITY_EDITOR")]
        private static void SetInstanceDirty()
        {
            EditorUtility.SetDirty(Instance);
        }
    }
}