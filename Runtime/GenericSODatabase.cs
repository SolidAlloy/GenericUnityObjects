namespace GenericScriptableObjects
{
    using System;
    using System.Collections.Generic;
    using SolidUtilities.Extensions;
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

        public static void Add(Type genericAssetType, Type key, Type value)
        {
            TypeDictionary assetDict = GetAssetDict(genericAssetType);
            assetDict.Add(new TypeReference(key), new TypeReference(value));
            EditorUtility.SetDirty(Instance);
        }

        public static bool ContainsKey(Type genericAssetType, Type key)
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
            bool result = assetDict.TryGetValue(paramTypes[0], out TypeReference typeRef);  // TODO: This will be changed later to array.
            value = typeRef;
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

    [Serializable]
    public class TypeDictionary : ISerializationCallbackReceiver
    {
        private readonly Dictionary<TypeReference, TypeReference> _dict =
            new Dictionary<TypeReference, TypeReference>(new TypeReferenceComparer());

        [SerializeField]
        // [HideInInspector] // TODO: hide fields
        private TypeReference[] _keys;

        [SerializeField]
        // [HideInInspector]
        private TypeReference[] _values;

        public void Add(Type key, Type value) => _dict.Add(key, value);

        public bool ContainsKey(Type key)
        {
            return _dict.ContainsKey(new TypeReference(key));
        }

        public bool TryGetValue(TypeReference key, out TypeReference value) => _dict.TryGetValue(key, out value);

        public bool TryGetValue(Type key, out Type value)
        {
            bool result = TryGetValue(key, out TypeReference typeRef);
            value = typeRef;
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
                if (_values[i].Type != null)
                    _dict[_keys[i]] = _values[i];
            }

            _keys = null;
            _values = null;
        }

        public void OnBeforeSerialize()
        {
            int dictLength = _dict.Count;
            _keys = new TypeReference[dictLength];
            _values = new TypeReference[dictLength];

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