namespace GenericScriptableObjects
{
    using System;
    using System.Collections.Generic;
    using TypeReferences;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Assertions;

    public class GenericSODatabase :
        SingletonScriptableObject<GenericSODatabase>,
        ISerializationCallbackReceiver
    {
        private readonly Dictionary<TypeReference, TypeDictionary> _dict =
            new Dictionary<TypeReference, TypeDictionary>(new TypeReferenceComparer());

        [SerializeField]
        // [HideInInspector] // TODO: hide fields
        private TypeReference[] _keys;

        [SerializeField]
        // [HideInInspector]
        private TypeDictionary[] _values;

        private static TypeDictionary GetAssetDict(Type genericAssetType)
        {
            if (Instance._dict.TryGetValue(genericAssetType, out TypeDictionary assetDict))
                return assetDict;

            assetDict = new TypeDictionary();
            Instance._dict.Add(genericAssetType, assetDict);
            EditorUtility.SetDirty(Instance);
            return assetDict;
        }

        public static void Add(Type genericAssetType, Type[] key, Type value)
        {
            TypeDictionary assetDict = GetAssetDict(genericAssetType);
            assetDict.Add(key, new TypeReference(value));
            EditorUtility.SetDirty(Instance);
        }

        public static bool ContainsKey(Type genericAssetType, Type[] key)
        {
            TypeDictionary assetDict = GetAssetDict(genericAssetType);
            return assetDict.ContainsKey(key);
        }

        public static bool TryGetValue(Type genericAssetType, out Type value)
        {
            var paramTypes = genericAssetType.GetGenericParameterConstraints();
            Assert.IsFalse(paramTypes.Length == 0);
            Type genericSOTypeWithoutTypeParams = genericAssetType.GetGenericTypeDefinition();
            TypeDictionary assetDict = GetAssetDict(genericSOTypeWithoutTypeParams);
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
                _dict[_keys[i]] = _values[i];

            _keys = null;
            _values = null;
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
    }
}