namespace GenericUnityObjects.Editor.GeneratedTypesDatabase
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using UnityEditor;
    using UnityEngine;

    internal abstract partial class GenerationDatabase<TDatabase>
        where TDatabase : GenerationDatabase<TDatabase>
    {
        public void OnAfterDeserialize()
        {
            InitializeArgumentBehavioursDict();
            InitializeBehaviourArgumentsDict();
        }

        public void Initialize()
        {
            if (_genericArgumentKeys != null)
                throw new InvalidOperationException("The asset is already initialized.");

            if (! typeof(GenericTypeInfo).IsSerializable)
                throw new SerializationException($"Cannot initialize a database with {typeof(GenericTypeInfo)} because the type is not serializable.");

            _argumentBehavioursDict = new Dictionary<ArgumentInfo, List<GenericTypeInfo>>();
            _argumentsPool = new Pool<ArgumentInfo>();
            _behaviourArgumentsDict = new Dictionary<GenericTypeInfo, List<ConcreteClass>>();
            _behavioursPool = new Pool<GenericTypeInfo>();
        }

        private void InitializeArgumentBehavioursDict()
        {
            if (_genericArgumentKeys == null)
            {
                _argumentBehavioursDict = new Dictionary<ArgumentInfo, List<GenericTypeInfo>>();
                _argumentsPool = new Pool<ArgumentInfo>();
                return;
            }

            int keysLength = _genericArgumentKeys.Length;
            int valuesLength = _genericBehaviourValues.Length;

            _argumentBehavioursDict = new Dictionary<ArgumentInfo, List<GenericTypeInfo>>(keysLength);

            _argumentsPool = new Pool<ArgumentInfo>(keysLength);
            _argumentsPool.AddRange(_genericArgumentKeys);

            _behavioursPool = new Pool<GenericTypeInfo>(valuesLength);

            if (keysLength != valuesLength)
            {
                Debug.LogError($"Something wrong happened in the database. Keys count ({keysLength}) does " +
                               $"not equal to values count ({valuesLength}). The database will be cleaned up.");
                _shouldSetDirty = true;
                return;
            }

            for (int keyIndex = 0; keyIndex < keysLength; ++keyIndex)
            {
                GenericTypeInfo[] valuesArray = _genericBehaviourValues[keyIndex];
                int valuesArrayLength = valuesArray.Length;
                var valuesToAdd = new List<GenericTypeInfo>(valuesArrayLength);

                for (int valueIndex = 0; valueIndex < valuesArrayLength; valueIndex++)
                    valuesToAdd.Add(_behavioursPool.GetOrAdd(valuesArray[valueIndex]));

                _argumentBehavioursDict[_genericArgumentKeys[keyIndex]] = valuesToAdd;
            }
        }

        private void InitializeBehaviourArgumentsDict()
        {
            int keysLength = _genericBehaviourKeys.Length;
            int valuesLength = _genericArgumentValues.Length;

            _behaviourArgumentsDict = new Dictionary<GenericTypeInfo, List<ConcreteClass>>(keysLength);

            if (keysLength != valuesLength)
            {
                Debug.LogError($"Something wrong happened in the database. Keys count ({keysLength}) does " +
                               $"not equal to values count ({valuesLength}). The database will be cleaned up.");
                _shouldSetDirty = true;
                return;
            }

            for (int keyIndex = 0; keyIndex < keysLength; keyIndex++)
            {
                GenericTypeInfo key = _behavioursPool.GetOrAdd(_genericBehaviourKeys[keyIndex]);

                List<ConcreteClass> value = _genericArgumentValues[keyIndex];

                foreach (ConcreteClass concreteClass in value)
                {
                    for (int i = 0; i < concreteClass.Arguments.Length; i++)
                    {
                        concreteClass.Arguments[i] = _argumentsPool.GetOrAdd(concreteClass.Arguments[i]);
                    }
                }

                _behaviourArgumentsDict[key] = value;
            }
        }

        public void OnBeforeSerialize()
        {
            SerializeArgumentBehavioursDict();
            SerializeBehaviourArgumentsDict();
        }

        private void SerializeArgumentBehavioursDict()
        {
            // If OnAfterDeserialize() has never been called yet for this new asset.
            if (_argumentBehavioursDict == null)
                return;

            int dictLength = _argumentBehavioursDict.Count;

            _genericArgumentKeys = new ArgumentInfo[dictLength];
            _genericBehaviourValues = new BehaviourCollection[dictLength];

            int keysIndex = 0;
            foreach (var pair in _argumentBehavioursDict)
            {
                _genericArgumentKeys[keysIndex] = pair.Key;
                _genericBehaviourValues[keysIndex] = pair.Value;
                ++keysIndex;
            }
        }

        private void SerializeBehaviourArgumentsDict()
        {
            // If OnAfterDeserialize() has never been called yet for this new asset.
            if (_behaviourArgumentsDict == null)
                return;

            int dictLength = _behaviourArgumentsDict.Count;

            _genericBehaviourKeys = new GenericTypeInfo[dictLength];
            _genericArgumentValues = new ConcreteClassCollection[dictLength];

            int keysIndex = 0;
            foreach (var pair in _behaviourArgumentsDict)
            {
                _genericBehaviourKeys[keysIndex] = pair.Key;
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