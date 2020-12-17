namespace GenericUnityObjects.Editor.MonoBehaviour
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using Util;
    using Debug = UnityEngine.Debug;

    internal class GenericBehavioursDatabase : SingletonScriptableObject<GenericBehavioursDatabase>, ISerializationCallbackReceiver
    {
        private Dictionary<TypeInfo, List<TypeInfo>> _argumentBehavioursDict;
        private Dictionary<TypeInfo, List<ConcreteClass>> _behaviourArgumentsDict;
        private TypeInfoPool _typeInfoPool;

        [HideInInspector] [SerializeField] private TypeInfo[] _genericArgumentKeys;
        [HideInInspector] [SerializeField] private TypeInfoCollection[] _genericBehaviourValues;
        [HideInInspector] [SerializeField] private TypeInfo[] _genericBehaviourKeys;
        [HideInInspector] [SerializeField] private ConcreteClassCollection[] _genericArgumentValues;

        private bool _shouldSetDirty;

        public static TypeInfo[] Arguments => CreatedOnlyInstance._argumentBehavioursDict.Keys.ToArray();
        public static TypeInfo[] Behaviours => CreatedOnlyInstance._behaviourArgumentsDict.Keys.ToArray();

        public static void AddGenericBehaviour(TypeInfo genericBehaviour)
        {
            CreatedOnlyInstance._behaviourArgumentsDict.Add(genericBehaviour, new List<ConcreteClass>());
            EditorUtility.SetDirty(CreatedOnlyInstance);
        }

        public static void AddConcreteClass(TypeInfo genericBehaviour, TypeInfo[] arguments, string assemblyName)
        {
            CreatedOnlyInstance._behaviourArgumentsDict[genericBehaviour].Add(new ConcreteClass(arguments, assemblyName));

            foreach (TypeInfo argument in arguments)
            {
                if (CreatedOnlyInstance._argumentBehavioursDict.ContainsKey(argument))
                {
                    CreatedOnlyInstance._argumentBehavioursDict[argument].Add(genericBehaviour);
                }
                else
                {
                    CreatedOnlyInstance._argumentBehavioursDict[argument] = new List<TypeInfo> { genericBehaviour };
                }
            }

            EditorUtility.SetDirty(CreatedOnlyInstance);
        }

        public static void RemoveArgument(TypeInfo argument, Action<string> assemblyAction)
        {
            if ( ! CreatedOnlyInstance._argumentBehavioursDict.TryGetValue(argument, out List<TypeInfo> genericBehaviours))
                return;

            CreatedOnlyInstance._argumentBehavioursDict.Remove(argument);

            foreach (TypeInfo genericBehaviour in genericBehaviours)
            {
                if ( ! CreatedOnlyInstance._behaviourArgumentsDict.TryGetValue(genericBehaviour, out List<ConcreteClass> concreteClasses))
                    continue;

                foreach (ConcreteClass concreteClass in concreteClasses)
                {
                    if (concreteClass.Arguments.Contains(argument))
                    {
                        concreteClasses.Remove(concreteClass);
                        assemblyAction(concreteClass.AssemblyName);
                    }
                }
            }

            EditorUtility.SetDirty(CreatedOnlyInstance);
        }

        public static void RemoveGenericBehaviour(TypeInfo genericBehaviour)
        {
            if ( ! CreatedOnlyInstance._behaviourArgumentsDict.TryGetValue(genericBehaviour, out List<ConcreteClass> concreteClasses))
                return;

            CreatedOnlyInstance._behaviourArgumentsDict.Remove(genericBehaviour);

            foreach (ConcreteClass concreteClass in concreteClasses)
            {
                foreach (TypeInfo argument in concreteClass.Arguments)
                {
                    if ( ! CreatedOnlyInstance._argumentBehavioursDict.TryGetValue(argument, out List<TypeInfo> behaviours))
                        continue;

                    behaviours.Remove(genericBehaviour);

                    if (behaviours.Count == 0)
                        CreatedOnlyInstance._argumentBehavioursDict.Remove(argument);
                }
            }

            EditorUtility.SetDirty(CreatedOnlyInstance);
        }

        public void OnAfterDeserialize()
        {
            InitializeArgumentBehavioursDict();
            InitializeBehaviourArgumentsDict();
        }

        private void InitializeArgumentBehavioursDict()
        {
            int keysLength = _genericArgumentKeys.Length;
            int valuesLength = _genericBehaviourValues.Length;

            _argumentBehavioursDict = new Dictionary<TypeInfo, List<TypeInfo>>(keysLength);

            _typeInfoPool = new TypeInfoPool(keysLength + valuesLength);
            _typeInfoPool.AddRange(_genericArgumentKeys);

            if (keysLength != valuesLength)
            {
                Debug.LogError($"Something wrong happened in the database. Keys count ({keysLength}) does " +
                               $"not equal to values count ({valuesLength}). The database will be cleaned up.");
                _shouldSetDirty = true;
                return;
            }

            for (int keyIndex = 0; keyIndex < keysLength; ++keyIndex)
            {
                TypeInfo[] valuesArray = _genericBehaviourValues[keyIndex];
                int valuesArrayLength = valuesArray.Length;
                var valuesToAdd = new List<TypeInfo>(valuesArrayLength);

                for (int valueIndex = 0; valueIndex < valuesArrayLength; valueIndex++)
                    valuesToAdd[valueIndex] = _typeInfoPool.GetOrAdd(valuesArray[valueIndex]);

                _argumentBehavioursDict[_genericArgumentKeys[keyIndex]] = valuesToAdd;
            }
        }

        private void InitializeBehaviourArgumentsDict()
        {
            int keysLength = _genericBehaviourKeys.Length;
            int valuesLength = _genericArgumentValues.Length;

            _behaviourArgumentsDict = new Dictionary<TypeInfo, List<ConcreteClass>>(keysLength);

            if (keysLength != valuesLength)
            {
                Debug.LogError($"Something wrong happened in the database. Keys count ({keysLength}) does " +
                               $"not equal to values count ({valuesLength}). The database will be cleaned up.");
                _shouldSetDirty = true;
                return;
            }

            for (int keyIndex = 0; keyIndex < keysLength; keyIndex++)
            {
                var key = _typeInfoPool.GetOrAdd(_genericBehaviourKeys[keyIndex]);

                List<ConcreteClass> value = _genericArgumentValues[keyIndex];

                foreach (ConcreteClass concreteClass in value)
                {
                    for (int i = 0; i < concreteClass.Arguments.Length; i++)
                    {
                        concreteClass.Arguments[i] = _typeInfoPool.GetOrAdd(concreteClass.Arguments[i]);
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
            int dictLength = _argumentBehavioursDict.Count;

            _genericArgumentKeys = new TypeInfo[dictLength];
            _genericBehaviourValues = new TypeInfoCollection[dictLength];

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
            int dictLength = _behaviourArgumentsDict.Count;

            _genericBehaviourKeys = new TypeInfo[dictLength];
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
            if (_shouldSetDirty)
            {
                _shouldSetDirty = false;
                EditorUtility.SetDirty(this);
            }
        }

        [Serializable]
        private class TypeInfoCollection
        {
            [SerializeField] private TypeInfo[] _array;

            public TypeInfoCollection(List<TypeInfo> collection) => _array = collection.ToArray();

            public TypeInfoCollection(TypeInfo[] collection) => _array = collection;

            public TypeInfoCollection() : this((TypeInfo[]) null) { }

            public static implicit operator TypeInfoCollection(List<TypeInfo> typeInfoArray) =>
                new TypeInfoCollection(typeInfoArray);

            public static implicit operator TypeInfo[] (TypeInfoCollection typeCollection) =>
                typeCollection._array;
        }

        [Serializable]
        private class ConcreteClassCollection
        {
            [SerializeField] private ConcreteClass[] _array;

            public ConcreteClassCollection(List<ConcreteClass> concreteClasses)
            {
                _array = concreteClasses.ToArray();
            }

            public ConcreteClassCollection() : this(null) { }

            public static implicit operator List<ConcreteClass>(ConcreteClassCollection concreteClassCollection)
            {
                var concreteClasses = new List<ConcreteClass>(concreteClassCollection._array.Length);

                for (int i = 0; i < concreteClasses.Count; i++)
                {
                    concreteClasses[i] = concreteClassCollection._array[i];
                }

                return concreteClasses;
            }

            public static implicit operator ConcreteClassCollection(List<ConcreteClass> concreteClasses) =>
                new ConcreteClassCollection(concreteClasses);
        }

        private class TypeInfoPool
        {
            private readonly Dictionary<TypeInfo, TypeInfo> _dict;

            public TypeInfoPool(int capacity)
            {
                _dict = new Dictionary<TypeInfo, TypeInfo>(capacity);
            }

            public TypeInfo GetOrAdd(TypeInfo item)
            {
                if (_dict.TryGetValue(item, out TypeInfo existingItem))
                    return existingItem;

                _dict[item] = item;
                return item;
            }

            public void AddRange(TypeInfo[] items)
            {
                foreach (TypeInfo item in items)
                {
                    if (! _dict.ContainsKey(item))
                        _dict.Add(item, item);
                }
            }
        }
    }

    [Serializable]
    internal class ConcreteClass
    {
        public TypeInfo[] Arguments;
        public string AssemblyName;

        public ConcreteClass(TypeInfo[] arguments, string assemblyName)
        {
            Arguments = arguments;
            AssemblyName = assemblyName;
        }
    }
}