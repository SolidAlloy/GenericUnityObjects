namespace GenericUnityObjects.Editor.GeneratedTypesDatabase
{
    using System.Collections.Generic;
    using GenericUnityObjects.Util;
    using SolidUtilities;
    using UnityEditor;
    using UnityEngine;
    using Util;
    using Object = UnityEngine.Object;

    internal abstract partial class GenerationDatabase<TUnityObject> :
        ISerializationCallbackReceiver,
        ICanBeInitialized
        where TUnityObject : Object
    {
        [SerializeField] private ArgumentInfo[] _genericArgumentKeys;
        [SerializeField] private Collection<ConcreteClass>[] _genericArgumentValues;

        private bool _shouldSetDirty;

        public void Initialize()
        {
            _argumentGenericTypesDict = new FastIterationDictionary<ArgumentInfo, List<GenericTypeInfo>>();
            _argumentsPool = new Pool<ArgumentInfo>();
            _genericTypeArgumentsDict = new FastIterationDictionary<GenericTypeInfo, List<ConcreteClass>>();
            _genericTypesPool = new Pool<GenericTypeInfo>();
        }

        public void OnAfterDeserialize()
        {
            InitializeArgumentGenericTypesDict();
            InitializeGenericTypeArgumentsDict();
        }

        public void OnBeforeSerialize()
        {
            SerializeArgumentGenericTypesDict();
            SerializeGenericTypeArgumentsDict();
        }

        protected abstract int GenericTypeKeysLength { get; }

        protected abstract int GenericTypeValuesLength { get; }

        protected abstract GenericTypeInfo[] GetTypeValueAtIndex(int index);

        protected abstract GenericTypeInfo GetTypeKeyAtIndex(int index);

        protected abstract void SetTypeValueAtIndex(int index, List<GenericTypeInfo> value);

        protected abstract void SetTypeKeyAtIndex(int index, GenericTypeInfo value);

        protected abstract void ResetTypeValuesToLength(int length);

        protected abstract void ResetTypeKeysToLength(int length);

        private void InitializeArgumentGenericTypesDict()
        {
            int keysLength = _genericArgumentKeys.Length;
            int valuesLength = GenericTypeValuesLength;

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
                GenericTypeInfo[] valuesArray = GetTypeValueAtIndex(keyIndex);
                int valuesArrayLength = valuesArray.Length;
                var valuesToAdd = new List<GenericTypeInfo>(valuesArrayLength);

                for (int valueIndex = 0; valueIndex < valuesArrayLength; valueIndex++)
                    valuesToAdd.Add(_genericTypesPool.GetOrAdd(valuesArray[valueIndex]));

                _argumentGenericTypesDict.Add(_genericArgumentKeys[keyIndex], valuesToAdd);
            }
        }

        private void InitializeGenericTypeArgumentsDict()
        {
            int keysLength = GenericTypeKeysLength;
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
                GenericTypeInfo key = _genericTypesPool.GetOrAdd(GetTypeKeyAtIndex(keyIndex));

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

        private void SerializeArgumentGenericTypesDict()
        {
            if (_argumentGenericTypesDict == null)
                return;

            int dictLength = _argumentGenericTypesDict.Count;

            _genericArgumentKeys = new ArgumentInfo[dictLength];
            ResetTypeValuesToLength(dictLength);

            int keysIndex = 0;
            foreach (var pair in _argumentGenericTypesDict)
            {
                _genericArgumentKeys[keysIndex] = pair.Key;
                SetTypeValueAtIndex(keysIndex, pair.Value);
                ++keysIndex;
            }
        }

        private void SerializeGenericTypeArgumentsDict()
        {
            if (_genericTypeArgumentsDict == null)
                return;

            int dictLength = _genericTypeArgumentsDict.Count;

            ResetTypeKeysToLength(dictLength);
            _genericArgumentValues = new Collection<ConcreteClass>[dictLength];

            int keysIndex = 0;
            foreach (var pair in _genericTypeArgumentsDict)
            {
                SetTypeKeyAtIndex(keysIndex, pair.Key);
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