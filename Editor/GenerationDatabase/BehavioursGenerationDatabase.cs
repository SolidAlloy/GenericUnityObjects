﻿namespace GenericUnityObjects.Editor.GeneratedTypesDatabase
{
    using System.Collections.Generic;
    using SolidUtilities.Helpers;
    using UnityEngine;

    /// <summary>
    /// All the work is done in the parent class. This is implemented just to create a ScriptableObject asset.
    /// </summary>
    internal class BehavioursGenerationDatabase : GenerationDatabase<MonoBehaviour>
    {
        [SerializeField] private ArgumentInfo[] _genericArgumentKeys;
        [SerializeField] private Collection<BehaviourInfo>[] _genericTypeValues;
        [SerializeField] private BehaviourInfo[] _genericTypeKeys;
        [SerializeField] private Collection<ConcreteClass>[] _genericArgumentValues;

        public static void UpdateComponentName(BehaviourInfo behaviourInfo, string newComponentName)
        {
            ((BehavioursGenerationDatabase)Instance).UpdateComponentNameImpl(behaviourInfo, newComponentName);
        }

        public void UpdateComponentNameImpl(BehaviourInfo behaviourInfo, string newComponentName)
        {
            TemporarilyRemovingGenericType(behaviourInfo, () =>
            {
                _genericTypesPool.ChangeItem(ref behaviourInfo, genericTypeToChange =>
                {
                    genericTypeToChange.UpdateComponentName(newComponentName);
                });
            });
        }

        public static void UpdateOrder(BehaviourInfo behaviourInfo, int newOrder)
        {
            ((BehavioursGenerationDatabase)Instance).UpdateOrderImpl(behaviourInfo, newOrder);
        }

        public void UpdateOrderImpl(BehaviourInfo behaviourInfo, int newOrder)
        {
            TemporarilyRemovingGenericType(behaviourInfo, () =>
            {
                _genericTypesPool.ChangeItem(ref behaviourInfo, genericTypeToChange =>
                {
                    genericTypeToChange.UpdateOrder(newOrder);
                });
            });
        }

        protected override void InitializeArgumentGenericTypesDict()
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

        protected override void InitializeGenericTypeArgumentsDict()
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

        protected override void SerializeArgumentGenericTypesDict()
        {
            if (_argumentGenericTypesDict == null)
                return;

            int dictLength = _argumentGenericTypesDict.Count;

            _genericArgumentKeys = new ArgumentInfo[dictLength];
            _genericTypeValues = new Collection<BehaviourInfo>[dictLength];

            int keysIndex = 0;
            foreach (var pair in _argumentGenericTypesDict)
            {
                _genericArgumentKeys[keysIndex] = pair.Key;
                _genericTypeValues[keysIndex] = pair.Value.ConvertAll(typeInfo => (BehaviourInfo) typeInfo);
                ++keysIndex;
            }
        }

        protected override void SerializeGenericTypeArgumentsDict()
        {
            if (_genericTypeArgumentsDict == null)
                return;

            int dictLength = _genericTypeArgumentsDict.Count;

            _genericTypeKeys = new BehaviourInfo[dictLength];
            _genericArgumentValues = new Collection<ConcreteClass>[dictLength];

            int keysIndex = 0;
            foreach (var pair in _genericTypeArgumentsDict)
            {
                _genericTypeKeys[keysIndex] = (BehaviourInfo) pair.Key;
                _genericArgumentValues[keysIndex] = pair.Value;
                ++keysIndex;
            }
        }
    }
}