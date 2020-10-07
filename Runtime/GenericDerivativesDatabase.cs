namespace GenericScriptableObjects
{
    using System;
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using TypeReferences;
    using UnityEditor;
    using UnityEngine;

    public class GenericDerivativesDatabase :
        SingletonScriptableObject<GenericDerivativesDatabase>,
        ISerializationCallbackReceiver
    {
        public const string Template = "namespace GenericScriptableObjectsTypes { public class " +
                                               "Generic_#TYPE_NAME : GenericScriptableObjects.Generic<#TYPE> { } }";

        private readonly Dictionary<TypeReference, TypeReference> _dict =
            new Dictionary<TypeReference, TypeReference>(new TypeReferenceComparer());

        [SerializeField]
        // [TypeOptions(ExcludeNone = true, ShortName = true)]
        // [HideInInspector] // TODO: hide fields
        private TypeReference[] _keys;

        [SerializeField]
        // [Inherits(typeof(Generic<>), ExcludeNone = true, ShortName = true, ExpandAllFolders = true)]
        // [HideInInspector]
        private TypeReference[] _values;

        public TypeReference this[TypeReference key]
        {
            get => _dict[key];
            set
            {
                _dict[key] = value;
                EditorUtility.SetDirty(this);
            }
        }

        public static void Add(Type key, Type value) =>
            Instance._dict.Add(new TypeReference(key), new TypeReference(value));

        public static bool ContainsKey(Type key)
        {
            bool containsKey = Instance._dict.ContainsKey(new TypeReference(key));
            EditorUtility.SetDirty(Instance);
            return containsKey;
        }

        public static bool TryGetValue(TypeReference key, out TypeReference value) =>
            Instance._dict.TryGetValue(key, out value);

        public static bool TryGetValue(Type key, out Type value)
        {
            var typeRef = new TypeReference();
            bool result = TryGetValue(new TypeReference(key), out typeRef);
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