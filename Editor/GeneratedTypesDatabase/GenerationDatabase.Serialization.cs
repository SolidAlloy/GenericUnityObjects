namespace GenericUnityObjects.Editor.GeneratedTypesDatabase
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using SolidUtilities.Helpers;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    internal abstract partial class GenerationDatabase<TUnityObject>
        where TUnityObject : Object
    {
        [SerializeField] private ArgumentInfo[] _genericArgumentKeys;
        [SerializeField] private GenericTypeCollection[] _genericTypeValues;
        [SerializeField] private GenericTypeInfo[] _genericTypeKeys;
        [SerializeField] private ConcreteClassCollection[] _genericArgumentValues;

        private bool _shouldSetDirty;

        public void OnAfterDeserialize()
        {
            InitializeArgumentGenericTypesDict();
            InitializeGenericTypeArgumentsDict();
        }

        public void Initialize()
        {
            if (_genericArgumentKeys != null)
                throw new InvalidOperationException("The asset is already initialized.");

            if (! typeof(GenericTypeInfo).IsSerializable)
                throw new SerializationException($"Cannot initialize a database with {typeof(GenericTypeInfo)} because the type is not serializable.");

            _argumentGenericTypesDict = new FastIterationDictionary<ArgumentInfo, List<GenericTypeInfo>>();
            _argumentsPool = new Pool<ArgumentInfo>();
            _genericTypeArgumentsDict = new FastIterationDictionary<GenericTypeInfo, List<ConcreteClass>>();
            _genericTypesPool = new Pool<GenericTypeInfo>();
        }

        private void InitializeArgumentGenericTypesDict()
        {
            int keysLength = _genericArgumentKeys.Length;
            int valuesLength = _genericTypeValues.Length;

            _argumentGenericTypesDict = new FastIterationDictionary<ArgumentInfo, List<GenericTypeInfo>>(keysLength);

            _argumentsPool = new Pool<ArgumentInfo>(keysLength);
            _argumentsPool.AddRange(_genericArgumentKeys);

            _genericTypesPool = new Pool<GenericTypeInfo>(valuesLength);

            if (keysLength != valuesLength)
            {
                Debug.LogError($"Something wrong happened in the database. Keys count ({keysLength}) does " +
                               $"not equal to values count ({valuesLength}). The database will be cleaned up.");
                _shouldSetDirty = true;
                return;
            }

            for (int keyIndex = 0; keyIndex < keysLength; ++keyIndex)
            {
                GenericTypeInfo[] valuesArray = _genericTypeValues[keyIndex];
                int valuesArrayLength = valuesArray.Length;
                var valuesToAdd = new List<GenericTypeInfo>(valuesArrayLength);

                for (int valueIndex = 0; valueIndex < valuesArrayLength; valueIndex++)
                    valuesToAdd.Add(_genericTypesPool.GetOrAdd(valuesArray[valueIndex]));

                _argumentGenericTypesDict.Add(_genericArgumentKeys[keyIndex], valuesToAdd);
            }
        }

        private void InitializeGenericTypeArgumentsDict()
        {
            int keysLength = _genericTypeKeys.Length;
            int valuesLength = _genericArgumentValues.Length;

            _genericTypeArgumentsDict = new FastIterationDictionary<GenericTypeInfo, List<ConcreteClass>>(keysLength);

            if (keysLength != valuesLength)
            {
                Debug.LogError($"Something wrong happened in the database. Keys count ({keysLength}) does " +
                               $"not equal to values count ({valuesLength}). The database will be cleaned up.");
                _shouldSetDirty = true;
                return;
            }

            for (int keyIndex = 0; keyIndex < keysLength; keyIndex++)
            {
                GenericTypeInfo key = _genericTypesPool.GetOrAdd(_genericTypeKeys[keyIndex]);

                List<ConcreteClass> value = _genericArgumentValues[keyIndex];

                foreach (ConcreteClass concreteClass in value)
                {
                    for (int i = 0; i < concreteClass.Arguments.Length; i++)
                    {
                        concreteClass.Arguments[i] = _argumentsPool.GetOrAdd(concreteClass.Arguments[i]);
                    }
                }

                _genericTypeArgumentsDict[key] = value;
            }
        }

        public void OnBeforeSerialize()
        {
            SerializeArgumentGenericTypesDict();
            SerializeGenericTypeArgumentsDict();
        }

        private void SerializeArgumentGenericTypesDict()
        {
            if (_argumentGenericTypesDict == null)
                return;

            int dictLength = _argumentGenericTypesDict.Count;

            _genericArgumentKeys = new ArgumentInfo[dictLength];
            _genericTypeValues = new GenericTypeCollection[dictLength];

            int keysIndex = 0;
            foreach (var pair in _argumentGenericTypesDict)
            {
                _genericArgumentKeys[keysIndex] = pair.Key;
                _genericTypeValues[keysIndex] = pair.Value;
                ++keysIndex;
            }
        }

        private void SerializeGenericTypeArgumentsDict()
        {
            if (_genericTypeArgumentsDict == null)
                return;

            int dictLength = _genericTypeArgumentsDict.Count;

            _genericTypeKeys = new GenericTypeInfo[dictLength];
            _genericArgumentValues = new ConcreteClassCollection[dictLength];

            int keysIndex = 0;
            foreach (var pair in _genericTypeArgumentsDict)
            {
                _genericTypeKeys[keysIndex] = pair.Key;
                _genericArgumentValues[keysIndex] = pair.Value;
                ++keysIndex;
            }
        }

        private void OnEnable()
        {
            if ( ! _shouldSetDirty)
                return;

            _shouldSetDirty = false;
            EditorUtility.SetDirty(this);
        }
    }
}