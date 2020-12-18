namespace GenericUnityObjects.Editor.MonoBehaviour
{
    using System;
    using System.Collections.Generic;
    using System.Data;
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

        public static TypeInfo[] Arguments
        {
            get
            {
                if (CreatedOnlyInstance == null)
                    throw new NoNullAllowedException("Check CreatedOnlyInstance for null before using the property");

                return CreatedOnlyInstance.InstanceArguments;
            }
        }

        public TypeInfo[] InstanceArguments => _argumentBehavioursDict.Keys.ToArray();

        public static TypeInfo[] Behaviours
        {
            get
            {
                if (CreatedOnlyInstance == null)
                    throw new NoNullAllowedException("Check CreatedOnlyInstance for null before using the property");

                return CreatedOnlyInstance.InstanceBehaviours;
            }
        }

        public TypeInfo[] InstanceBehaviours => _behaviourArgumentsDict.Keys.ToArray();

        public static void AddGenericBehaviour(TypeInfo genericBehaviour)
        {
            if (CreatedOnlyInstance == null)
                throw new NoNullAllowedException("Check CreatedOnlyInstance for null before calling the method");

            CreatedOnlyInstance.InstanceAddGenericBehaviour(genericBehaviour);
        }

        public void InstanceAddGenericBehaviour(TypeInfo genericBehaviour)
        {
            genericBehaviour = _typeInfoPool.GetOrAdd(genericBehaviour);
            _behaviourArgumentsDict.Add(genericBehaviour, new List<ConcreteClass>());
            EditorUtility.SetDirty(this);
        }

        public static void AddConcreteClass(TypeInfo genericBehaviour, TypeInfo[] arguments, string assemblyGUID)
        {
            if (CreatedOnlyInstance == null)
                throw new NoNullAllowedException("Check CreatedOnlyInstance for null before calling the method");

            CreatedOnlyInstance.InstanceAddConcreteClass(genericBehaviour, arguments, assemblyGUID);
        }

        public void InstanceAddConcreteClass(TypeInfo genericBehaviour, TypeInfo[] arguments, string assemblyGUID)
        {
            if (!_behaviourArgumentsDict.TryGetValue(genericBehaviour, out List<ConcreteClass> concreteClasses))
            {
                throw new KeyNotFoundException($"Cannot add a concrete class to a generic behaviour '{genericBehaviour}' not present in the database.");
            }

            genericBehaviour = _typeInfoPool.GetOrAdd(genericBehaviour);

            int argumentsLength = arguments.Length;

            for (int i = 0; i < argumentsLength; i++ )
            {
                var argument = arguments[i];
                argument = _typeInfoPool.GetOrAdd(argument);

                if (_argumentBehavioursDict.ContainsKey(argument))
                {
                    _argumentBehavioursDict[argument].Add(genericBehaviour);
                }
                else
                {
                    _argumentBehavioursDict[argument] = new List<TypeInfo> { genericBehaviour };
                }
            }

            var classToAdd = new ConcreteClass(arguments, assemblyGUID);

            if (concreteClasses.Contains(classToAdd))
            {
                throw new ArgumentException($"The generic behaviour '{genericBehaviour}' already " +
                                            "has the following concrete class in the database: " +
                                            $"{string.Join(", ", arguments.Select(arg => arg.TypeFullName))}");
            }

            concreteClasses.Add(classToAdd);

            EditorUtility.SetDirty(this);
        }

        public static void RemoveArgument(TypeInfo argument, Action<string> assemblyAction)
        {
            if (CreatedOnlyInstance == null)
                throw new NoNullAllowedException("Check CreatedOnlyInstance for null before calling the method");

            CreatedOnlyInstance.InstanceRemoveArgument(argument, assemblyAction);
        }

        public void InstanceRemoveArgument(TypeInfo argument, Action<string> assemblyAction)
        {
            if ( ! _argumentBehavioursDict.TryGetValue(argument, out List<TypeInfo> genericBehaviours))
                throw new KeyNotFoundException($"Argument '{argument}' was not found in the database.");

            _argumentBehavioursDict.Remove(argument);

            foreach (TypeInfo genericBehaviour in genericBehaviours)
            {
                if ( ! _behaviourArgumentsDict.TryGetValue(genericBehaviour, out List<ConcreteClass> concreteClasses))
                    continue;

                for (int i = concreteClasses.Count - 1; i >= 0; i--)
                {
                    ConcreteClass concreteClass = concreteClasses[i];

                    if (concreteClass.Arguments.Contains(argument))
                    {
                        concreteClasses.RemoveAt(i);
                        assemblyAction(concreteClass.AssemblyGUID);
                    }
                }
            }

            EditorUtility.SetDirty(this);
        }

        public static void RemoveGenericBehaviour(TypeInfo genericBehaviour)
        {
            if (CreatedOnlyInstance == null)
                throw new NoNullAllowedException("Check CreatedOnlyInstance for null before calling the method");

            CreatedOnlyInstance.InstanceRemoveGenericBehaviour(genericBehaviour);
        }

        public void InstanceRemoveGenericBehaviour(TypeInfo genericBehaviour)
        {
            if ( ! _behaviourArgumentsDict.TryGetValue(genericBehaviour, out List<ConcreteClass> concreteClasses))
                throw new KeyNotFoundException($"Behaviour '{genericBehaviour}' was not found in the database.");

            _behaviourArgumentsDict.Remove(genericBehaviour);

            foreach (ConcreteClass concreteClass in concreteClasses)
            {
                foreach (TypeInfo argument in concreteClass.Arguments)
                {
                    if ( ! _argumentBehavioursDict.TryGetValue(argument, out List<TypeInfo> behaviours))
                        continue;

                    behaviours.Remove(genericBehaviour);

                    if (behaviours.Count == 0)
                        _argumentBehavioursDict.Remove(argument);
                }
            }

            EditorUtility.SetDirty(this);
        }

        public static bool TryGetReferencedBehaviours(TypeInfo argument, out TypeInfo[] referencedBehaviours)
        {
            if (CreatedOnlyInstance == null)
                throw new NoNullAllowedException("Check CreatedOnlyInstance for null before calling the method");

            return CreatedOnlyInstance.InstanceTryGetReferencedBehaviours(argument, out referencedBehaviours);
        }

        public bool InstanceTryGetReferencedBehaviours(TypeInfo argument, out TypeInfo[] referencedBehaviours)
        {
            bool success = _argumentBehavioursDict.TryGetValue(argument, out List<TypeInfo> behavioursList);
            referencedBehaviours = success ? behavioursList.ToArray() : null;
            return success;
        }

        public static bool TryGetConcreteClasses(TypeInfo behaviour, out ConcreteClass[] concreteClasses)
        {
            if (CreatedOnlyInstance == null)
                throw new NoNullAllowedException("Check CreatedOnlyInstance for null before calling the method");

            return CreatedOnlyInstance.InstanceTryGetConcreteClasses(behaviour, out concreteClasses);
        }

        public bool InstanceTryGetConcreteClasses(TypeInfo behaviour, out ConcreteClass[] concreteClasses)
        {
            bool success = _behaviourArgumentsDict.TryGetValue(behaviour, out List<ConcreteClass> concreteClassesList);
            concreteClasses = success ? concreteClassesList.ToArray() : null;
            return success;
        }

        public static bool TryGetConcreteClassesByArgument(TypeInfo behaviour, TypeInfo argument, out ConcreteClass[] concreteClasses)
        {
            if (CreatedOnlyInstance == null)
                throw new NoNullAllowedException("Check CreatedOnlyInstance for null before calling the method");

            return CreatedOnlyInstance.InstanceTryGetConcreteClassesByArgument(behaviour, argument, out concreteClasses);
        }

        public bool InstanceTryGetConcreteClassesByArgument(TypeInfo behaviour, TypeInfo argument, out ConcreteClass[] concreteClasses)
        {
            if ( ! _behaviourArgumentsDict.TryGetValue(behaviour, out List<ConcreteClass> concreteClassesList))
            {
                concreteClasses = null;
                return false;
            }

            concreteClasses = concreteClassesList
                .Where(concreteClass => concreteClass.Arguments.Contains(argument))
                .ToArray();

            if (concreteClasses.Length != 0)
                return true;

            concreteClasses = null;
            return false;
        }

        public static TypeInfo UpdateArgumentGUID(TypeInfo argument, string newGUID)
        {
            if (CreatedOnlyInstance == null)
                throw new NoNullAllowedException("Check CreatedOnlyInstance for null before calling the method");

            return CreatedOnlyInstance.InstanceUpdateArgumentGUID(argument, newGUID);
        }

        public TypeInfo InstanceUpdateArgumentGUID(TypeInfo argument, string newGUID)
        {
            if (! _argumentBehavioursDict.TryGetValue(argument, out List<TypeInfo> behaviours))
                throw new KeyNotFoundException($"Argument '{argument}' was not found in the database.");

            _argumentBehavioursDict.Remove(argument);
            argument = _typeInfoPool.GetOrAdd(argument);
            argument.UpdateGUID(newGUID);
            _argumentBehavioursDict.Add(argument, behaviours);
            EditorUtility.SetDirty(this);
            return argument;
        }

        public static TypeInfo UpdateArgumentFullName(TypeInfo argument, string newName)
        {
            if (CreatedOnlyInstance == null)
                throw new NoNullAllowedException("Check CreatedOnlyInstance for null before calling the method");

            return CreatedOnlyInstance.InstanceUpdateArgumentFullName(argument, newName);
        }

        public TypeInfo InstanceUpdateArgumentFullName(TypeInfo argument, string newName)
        {
            if (! _argumentBehavioursDict.TryGetValue(argument, out List<TypeInfo> behaviours))
                throw new KeyNotFoundException($"Argument '{argument}' was not found in the database.");

            _argumentBehavioursDict.Remove(argument);
            argument = _typeInfoPool.GetOrAdd(argument);
            argument.UpdateFullName(newName);
            _argumentBehavioursDict.Add(argument, behaviours);
            EditorUtility.SetDirty(this);
            return argument;
        }

        public static TypeInfo UpdateBehaviourGUID(TypeInfo behaviour, string newGUID)
        {
            if (CreatedOnlyInstance == null)
                throw new NoNullAllowedException("Check CreatedOnlyInstance for null before calling the method");

            return CreatedOnlyInstance.InstanceUpdateBehaviourGUID(behaviour, newGUID);
        }

        public TypeInfo InstanceUpdateBehaviourGUID(TypeInfo behaviour, string newGUID)
        {
            if (! _behaviourArgumentsDict.TryGetValue(behaviour, out List<ConcreteClass> concreteClasses))
                throw new KeyNotFoundException($"Behaviour '{behaviour}' was not found in the database.");

            _behaviourArgumentsDict.Remove(behaviour);
            behaviour = _typeInfoPool.GetOrAdd(behaviour);
            behaviour.UpdateGUID(newGUID);
            _behaviourArgumentsDict.Add(behaviour, concreteClasses);
            EditorUtility.SetDirty(this);
            return behaviour;
        }

        public static TypeInfo UpdateBehaviourFullName(TypeInfo behaviour, string newName)
        {
            if (CreatedOnlyInstance == null)
                throw new NoNullAllowedException("Check CreatedOnlyInstance for null before calling the method");

            return CreatedOnlyInstance.InstanceUpdateBehaviourFullName(behaviour, newName);
        }

        public TypeInfo InstanceUpdateBehaviourFullName(TypeInfo behaviour, string newName)
        {
            if (! _behaviourArgumentsDict.TryGetValue(behaviour, out List<ConcreteClass> concreteClasses))
                throw new KeyNotFoundException($"Argument '{behaviour}' was not found in the database.");

            _behaviourArgumentsDict.Remove(behaviour);
            behaviour = _typeInfoPool.GetOrAdd(behaviour);
            behaviour.UpdateFullName(newName);
            _behaviourArgumentsDict.Add(behaviour, concreteClasses);
            EditorUtility.SetDirty(this);
            return behaviour;
        }

        public void OnAfterDeserialize()
        {
            InitializeArgumentBehavioursDict();
            InitializeBehaviourArgumentsDict();
        }

        private void InitializeArgumentBehavioursDict()
        {
            // If it is a runtime-created asset and OnBeforeSerialize has never been called for it yet.
            if (_genericArgumentKeys == null)
            {
                _argumentBehavioursDict = new Dictionary<TypeInfo, List<TypeInfo>>();
                _typeInfoPool = new TypeInfoPool();
                return;
            }

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
                    valuesToAdd.Add(_typeInfoPool.GetOrAdd(valuesArray[valueIndex]));

                _argumentBehavioursDict[_genericArgumentKeys[keyIndex]] = valuesToAdd;
            }
        }

        private void InitializeBehaviourArgumentsDict()
        {
            // If it is a runtime-created asset and OnBeforeSerialize has never been called for it yet.
            if (_genericBehaviourKeys == null)
            {
                _behaviourArgumentsDict = new Dictionary<TypeInfo, List<ConcreteClass>>();
                return;
            }

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
            // If OnAfterDeserialize() has never been called yet for this new asset.
            if (_argumentBehavioursDict == null)
                return;

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
            // If OnAfterDeserialize() has never been called yet for this new asset.
            if (_behaviourArgumentsDict == null)
                return;

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
                return new List<ConcreteClass>(concreteClassCollection._array);
            }

            public static implicit operator ConcreteClassCollection(List<ConcreteClass> concreteClasses) =>
                new ConcreteClassCollection(concreteClasses);
        }

        private class TypeInfoPool
        {
            private readonly Dictionary<TypeInfo, TypeInfo> _dict;

            public TypeInfoPool(int capacity = 0)
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
}