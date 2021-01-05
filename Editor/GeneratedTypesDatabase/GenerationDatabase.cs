namespace GenericUnityObjects.Editor.GeneratedTypesDatabase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GenericUnityObjects.Util;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Assertions;
    using Util;

    internal abstract partial class GenerationDatabase<TDatabase> :
        EditorOnlySingletonSO<TDatabase>,
        ISerializationCallbackReceiver,
        ICanBeInitialized
        where TDatabase : GenerationDatabase<TDatabase>
    {
        private Dictionary<ArgumentInfo, List<GenericTypeInfo>> _argumentBehavioursDict;
        private Dictionary<GenericTypeInfo, List<ConcreteClass>> _behaviourArgumentsDict;
        private Pool<ArgumentInfo> _argumentsPool;
        private Pool<GenericTypeInfo> _behavioursPool;

        [HideInInspector] [SerializeField] private ArgumentInfo[] _genericArgumentKeys;
        [HideInInspector] [SerializeField] private BehaviourCollection[] _genericBehaviourValues;
        [HideInInspector] [SerializeField] private GenericTypeInfo[] _genericBehaviourKeys;
        [HideInInspector] [SerializeField] private ConcreteClassCollection[] _genericArgumentValues;

        private bool _shouldSetDirty;

        public static ArgumentInfo[] Arguments => Instance.InstanceArguments;

        public ArgumentInfo[] InstanceArguments => _argumentBehavioursDict.Keys.ToArray();

        public static GenericTypeInfo[] GenericUnityObjects => Instance.InstanceBehaviours;

        public GenericTypeInfo[] InstanceBehaviours => _behaviourArgumentsDict.Keys.ToArray();

        public static void AddGenericType(GenericTypeInfo genericBehaviour)
        {
            Instance.AddGenericBehaviourImpl(genericBehaviour, out List<ConcreteClass> _);
        }

        public void AddGenericBehaviourImpl(GenericTypeInfo genericBehaviour, out List<ConcreteClass> concreteClasses)
        {
            concreteClasses = new List<ConcreteClass>();
            genericBehaviour = _behavioursPool.GetOrAdd(genericBehaviour);
            _behaviourArgumentsDict.Add(genericBehaviour, concreteClasses);
            EditorUtility.SetDirty(this);
        }

        public static void AddConcreteClass(Type genericTypeWithoutArgs, Type[] genericArgs, string assemblyGUID)
        {
            var behaviour = new GenericTypeInfo(genericTypeWithoutArgs);

            int genericArgsLength = genericArgs.Length;
            var arguments = new ArgumentInfo[genericArgsLength];

            for (int i = 0; i < genericArgsLength; i++)
            {
                arguments[i] = new ArgumentInfo(genericArgs[i]);
            }

            Instance.AddConcreteClassImpl(behaviour, arguments, assemblyGUID);
        }

        public void AddConcreteClassImpl(GenericTypeInfo genericTypeInfo, ArgumentInfo[] arguments, string assemblyGUID)
        {
            if (!_behaviourArgumentsDict.TryGetValue(genericTypeInfo, out List<ConcreteClass> concreteClasses))
            {
                throw new KeyNotFoundException($"Cannot add a concrete class to a generic Unity.Object '{genericTypeInfo}' because it is not present in the database.");
            }

            genericTypeInfo = _behavioursPool.GetOrAdd(genericTypeInfo);

            int argumentsLength = arguments.Length;

            for (int i = 0; i < argumentsLength; i++ )
            {
                ArgumentInfo argument = arguments[i];
                argument = _argumentsPool.GetOrAdd(argument);

                if (_argumentBehavioursDict.ContainsKey(argument))
                {
                    _argumentBehavioursDict[argument].Add(genericTypeInfo);
                }
                else
                {
                    _argumentBehavioursDict[argument] = new List<GenericTypeInfo> { genericTypeInfo };
                }
            }

            var classToAdd = new ConcreteClass(arguments, assemblyGUID);

            if (concreteClasses.Contains(classToAdd))
            {
                throw new ArgumentException($"The generic behaviour '{genericTypeInfo}' already " +
                                            "has the following concrete class in the database: " +
                                            $"{string.Join(", ", arguments.Select(arg => arg.TypeNameAndAssembly))}");
            }

            concreteClasses.Add(classToAdd);

            EditorUtility.SetDirty(this);
        }

        public static void RemoveArgument(ArgumentInfo argument, Action<string> assemblyAction)
        {
            Instance.RemoveArgumentImpl(argument, assemblyAction);
        }

        public void RemoveArgumentImpl(ArgumentInfo argument, Action<string> assemblyAction)
        {
            if ( ! _argumentBehavioursDict.TryGetValue(argument, out List<GenericTypeInfo> genericBehaviours))
                throw new KeyNotFoundException($"Argument '{argument}' was not found in the database.");

            _argumentBehavioursDict.Remove(argument);

            foreach (GenericTypeInfo genericBehaviour in genericBehaviours)
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

        public static bool RemoveGenericType(GenericTypeInfo genericBehaviour, Action<string> removeAssembly)
        {
            return Instance.RemoveGenericBehaviourImpl(genericBehaviour, removeAssembly);
        }

        public bool RemoveGenericBehaviourImpl(GenericTypeInfo genericBehaviour, Action<string> removeAssembly)
        {
            if ( ! _behaviourArgumentsDict.TryGetValue(genericBehaviour, out List<ConcreteClass> concreteClasses))
                throw new KeyNotFoundException($"Behaviour '{genericBehaviour}' was not found in the database.");

            removeAssembly(genericBehaviour.AssemblyGUID);
            _behaviourArgumentsDict.Remove(genericBehaviour);

            foreach (ConcreteClass concreteClass in concreteClasses)
            {
                foreach (ArgumentInfo argument in concreteClass.Arguments)
                {
                    if ( ! _argumentBehavioursDict.TryGetValue(argument, out List<GenericTypeInfo> behaviours))
                        continue;

                    behaviours.Remove(genericBehaviour);

                    if (behaviours.Count == 0)
                        _argumentBehavioursDict.Remove(argument);
                }

                removeAssembly(concreteClass.AssemblyGUID);
            }

            EditorUtility.SetDirty(this);
            return concreteClasses.Count != 0;
        }

        public static bool TryGetReferencedBehaviours(ArgumentInfo argument, out GenericTypeInfo[] referencedBehaviours)
        {
            return Instance.TryGetReferencedBehavioursImpl(argument, out referencedBehaviours);
        }

        public bool TryGetReferencedBehavioursImpl(ArgumentInfo argument, out GenericTypeInfo[] referencedBehaviours)
        {
            bool success = _argumentBehavioursDict.TryGetValue(argument, out List<GenericTypeInfo> behavioursList);
            referencedBehaviours = success ? behavioursList.ToArray() : null;
            return success;
        }

        public static bool TryGetConcreteClasses(GenericTypeInfo behaviour, out ConcreteClass[] concreteClasses)
        {
            return Instance.TryGetConcreteClassesImpl(behaviour, out concreteClasses);
        }

        public bool TryGetConcreteClassesImpl(GenericTypeInfo behaviour, out ConcreteClass[] concreteClasses)
        {
            bool success = _behaviourArgumentsDict.TryGetValue(behaviour, out List<ConcreteClass> concreteClassesList);
            concreteClasses = success ? concreteClassesList.ToArray() : null;
            return success;
        }

        public static bool TryGetConcreteClassesByArgument(GenericTypeInfo behaviour, ArgumentInfo argument, out ConcreteClass[] concreteClasses)
        {
            return Instance.TryGetConcreteClassesByArgumentImpl(behaviour, argument, out concreteClasses);
        }

        public bool TryGetConcreteClassesByArgumentImpl(GenericTypeInfo behaviour, ArgumentInfo argument, out ConcreteClass[] concreteClasses)
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

        public static void UpdateArgumentGUID(ref ArgumentInfo argument, string newGUID)
        {
            Instance.UpdateArgumentGUIDImpl(ref argument, newGUID);
        }

        public void UpdateArgumentGUIDImpl(ref ArgumentInfo argument, string newGUID)
        {
            if (! _argumentBehavioursDict.TryGetValue(argument, out List<GenericTypeInfo> behaviours))
                throw new KeyNotFoundException($"Argument '{argument}' was not found in the database.");

            _argumentBehavioursDict.Remove(argument);

            _argumentsPool.ChangeItem(ref argument, argumentToChange =>
            {
                argumentToChange.UpdateGUID(newGUID);
            });

            _argumentBehavioursDict.Add(argument, behaviours);
            EditorUtility.SetDirty(this);
        }

        public static void UpdateArgumentNameAndAssembly(ref ArgumentInfo argument, Type newType)
        {
            Instance.UpdateArgumentNameAndAssemblyImpl(ref argument, newType);
        }

        public void UpdateArgumentNameAndAssemblyImpl(ref ArgumentInfo argument, Type newType)
        {
            if (! _argumentBehavioursDict.TryGetValue(argument, out List<GenericTypeInfo> behaviours))
                throw new KeyNotFoundException($"Argument '{argument}' was not found in the database.");

            _argumentBehavioursDict.Remove(argument);

            _argumentsPool.ChangeItem(ref argument, argumentToChange =>
            {
                argumentToChange.UpdateNameAndAssembly(newType);
            });

            _argumentBehavioursDict.Add(argument, behaviours);
            EditorUtility.SetDirty(this);
        }

        public static void UpdateGenericTypeGUID(ref GenericTypeInfo behaviour, string newGUID)
        {
            Instance.UpdateBehaviourGUIDImpl(ref behaviour, newGUID);
        }

        public void UpdateBehaviourGUIDImpl(ref GenericTypeInfo behaviour, string newGUID)
        {
            if (! _behaviourArgumentsDict.TryGetValue(behaviour, out List<ConcreteClass> concreteClasses))
                throw new KeyNotFoundException($"Behaviour '{behaviour}' was not found in the database.");

            _behaviourArgumentsDict.Remove(behaviour);

            _behavioursPool.ChangeItem(ref behaviour, behaviourToChange =>
            {
                behaviourToChange.UpdateGUID(newGUID);
            });

            _behaviourArgumentsDict.Add(behaviour, concreteClasses);
            EditorUtility.SetDirty(this);
        }

        public static void UpdateGenericTypeArgs(ref GenericTypeInfo behaviour, string[] newArgNames)
        {
            Instance.UpdateBehaviourArgsImpl(ref behaviour, newArgNames);
        }

        public void UpdateBehaviourArgsImpl(ref GenericTypeInfo behaviour, string[] newArgNames)
        {
            if (! _behaviourArgumentsDict.TryGetValue(behaviour, out List<ConcreteClass> concreteClasses))
                throw new KeyNotFoundException($"Behaviour '{behaviour}' was not found in the database.");

            _behaviourArgumentsDict.Remove(behaviour);

            _behavioursPool.ChangeItem(ref behaviour, behaviourToChange =>
            {
                behaviourToChange.UpdateArgNames(newArgNames);
            });

            _behaviourArgumentsDict.Add(behaviour, concreteClasses);
            EditorUtility.SetDirty(this);
        }

        public static void UpdateBehaviourNameAndAssembly(ref GenericTypeInfo behaviour, Type newType)
        {
            Instance.UpdateBehaviourNameAndAssemblyImpl(ref behaviour, newType);
        }

        public void UpdateBehaviourNameAndAssemblyImpl(ref GenericTypeInfo behaviour, Type newType)
        {
            if (! _behaviourArgumentsDict.TryGetValue(behaviour, out List<ConcreteClass> concreteClasses))
                throw new KeyNotFoundException($"Argument '{behaviour}' was not found in the database.");

            _behaviourArgumentsDict.Remove(behaviour);

            _behavioursPool.ChangeItem(ref behaviour, behaviourToChange =>
            {
                behaviourToChange.UpdateNameAndAssembly(newType);
            });

            _behaviourArgumentsDict.Add(behaviour, concreteClasses);
            EditorUtility.SetDirty(this);
        }
    }
}