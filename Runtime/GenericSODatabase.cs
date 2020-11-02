namespace GenericScriptableObjects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TypeReferences;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;
    using UnityEngine.Assertions;
    using Util;

    /// <summary>
    /// A database of all the type parameters of generic scriptable objects and their matching concrete implementations.
    /// When a new GenericScriptableObject asset is created through Unity context menu, a concrete implementation is
    /// created and added to this dictionary.
    ///
    /// For example:
    /// CustomGeneric&lt;T>
    ///     bool --- CustomGeneric_1_System_Boolean
    ///     int  --- CustomGeneric_1_System_Int32
    /// CustomGeneric&lt;T1,T2>
    ///     bool, int --- CustomGeneric_2_System_Boolean_System_Int32
    ///     bool, float --- CustomGeneric_2_System_Boolean_System_Single
    /// </summary>
    public class GenericSODatabase :
        SingletonScriptableObject<GenericSODatabase>,
        ISerializationCallbackReceiver
    {
        private readonly Dictionary<TypeReference, TypeDictionary> _dict =
            new Dictionary<TypeReference, TypeDictionary>(new TypeReferenceComparer());

        [HideInInspector]
        [SerializeField] private TypeReference[] _keys;

        [HideInInspector]
        [SerializeField] private TypeDictionary[] _values;

        public static void Add(Type genericType, Type[] key, Type value)
        {
            Assert.IsTrue(genericType.IsGenericTypeDefinition);
            TypeDictionary assetDict = GetAssetDict(genericType);
            assetDict.Add(key, value);
#if UNITY_EDITOR
            EditorUtility.SetDirty(Instance);
#endif
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
            TypeDictionary assetDict = GetAssetDict(genericTypeWithoutParams);
            bool result = assetDict.TryGetValue(paramTypes, out value);
            return result;
        }

        public void OnAfterDeserialize()
        {
            if (_keys == null || _values == null || _keys.Length != _values.Length)
                return;

            _dict.Clear();
            int keysLength = _keys.Length;

            for (int i = 0; i < keysLength; ++i)
            {
                TypeReference typeRef = _keys[i];

                if (typeRef.TypeIsMissing())
                    continue;

                _dict[typeRef] = _values[i];
            }
        }

        public void OnBeforeSerialize()
        {
            int dictLength = _dict.Count;

            var prevKeys = _keys;
            var prevValues = _values;

            _keys = new TypeReference[dictLength];
            _values = new TypeDictionary[dictLength];

            int keysIndex = 0;
            foreach (var pair in _dict)
            {
                _keys[keysIndex] = pair.Key;
                _values[keysIndex] = pair.Value;
                ++keysIndex;
            }

            if ( ! _keys.SequenceEqual(prevKeys) || ! _values.SequenceEqual(prevValues))
            {
                Debug.Log("not equal, setting dirty");
                EditorUtility.SetDirty(this);
            }
        }

        private static TypeDictionary GetAssetDict(Type genericType)
        {
            if (Instance._dict.TryGetValue(genericType, out TypeDictionary assetDict))
                return assetDict;

            assetDict = new TypeDictionary();
            Instance._dict.Add(genericType, assetDict);
#if UNITY_EDITOR
            EditorUtility.SetDirty(Instance);
#endif
            return assetDict;
        }
    }
}