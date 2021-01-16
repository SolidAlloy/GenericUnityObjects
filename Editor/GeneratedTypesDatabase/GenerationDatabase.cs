namespace GenericUnityObjects.Editor.GeneratedTypesDatabase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GenericUnityObjects.Util;
    using JetBrains.Annotations;
    using SolidUtilities.Helpers;
    using UnityEditor;
    using UnityEngine;
    using Util;
    using Object = UnityEngine.Object;

    internal abstract partial class GenerationDatabase<TUnityObject> :
        EditorOnlySingletonSO<GenerationDatabase<TUnityObject>>,
        ISerializationCallbackReceiver,
        ICanBeInitialized
        where TUnityObject : Object
    {
        private FastIterationDictionary<ArgumentInfo, List<GenericTypeInfo>> _argumentGenericTypesDict;
        private FastIterationDictionary<GenericTypeInfo, List<ConcreteClass>> _genericTypeArgumentsDict;
        private Pool<ArgumentInfo> _argumentsPool;
        private Pool<GenericTypeInfo> _genericTypesPool;

        [SerializeField] private ArgumentInfo[] _genericArgumentKeys;
        [SerializeField] private GenericTypeCollection[] _genericTypeValues;
        [SerializeField] private GenericTypeInfo[] _genericTypeKeys;
        [SerializeField] private ConcreteClassCollection[] _genericArgumentValues;

        private bool _shouldSetDirty;

        public static ArgumentInfo[] Arguments => Instance.InstanceArguments;

        public ArgumentInfo[] InstanceArguments => _argumentGenericTypesDict.KeysCollection;

        public static GenericTypeInfo[] GenericTypes => Instance.InstanceGenericTypes;

        public GenericTypeInfo[] InstanceGenericTypes => _genericTypeArgumentsDict.KeysCollection;

        public static void AddGenericType(GenericTypeInfo genericTypeInfo)
        {
            Instance.AddGenericTypeImpl(genericTypeInfo);
        }

        public void AddGenericTypeImpl(GenericTypeInfo genericTypeInfo)
        {
            genericTypeInfo = _genericTypesPool.GetOrAdd(genericTypeInfo);
            _genericTypeArgumentsDict.Add(genericTypeInfo, new List<ConcreteClass>());
            EditorUtility.SetDirty(this);
        }

        public static void AddConcreteClass(Type genericTypeWithoutArgs, Type[] genericArgs, string assemblyGUID)
        {
            var genericTypeInfo = new GenericTypeInfo(genericTypeWithoutArgs);

            int genericArgsLength = genericArgs.Length;
            var arguments = new ArgumentInfo[genericArgsLength];

            for (int i = 0; i < genericArgsLength; i++)
            {
                arguments[i] = new ArgumentInfo(genericArgs[i]);
            }

            Instance.AddConcreteClassImpl(genericTypeInfo, arguments, assemblyGUID);
        }

        public void AddConcreteClassImpl(GenericTypeInfo genericTypeInfo, ArgumentInfo[] arguments, string assemblyGUID)
        {
            if (!_genericTypeArgumentsDict.TryGetValue(genericTypeInfo, out List<ConcreteClass> concreteClasses))
            {
                throw new KeyNotFoundException($"Cannot add a concrete class to a generic Unity.Object '{genericTypeInfo}' because it is not present in the database.");
            }

            genericTypeInfo = _genericTypesPool.GetOrAdd(genericTypeInfo);

            int argumentsLength = arguments.Length;

            for (int i = 0; i < argumentsLength; i++ )
            {
                ArgumentInfo argument = arguments[i];
                argument = _argumentsPool.GetOrAdd(argument);

                if (_argumentGenericTypesDict.ContainsKey(argument))
                {
                    _argumentGenericTypesDict[argument].Add(genericTypeInfo);
                }
                else
                {
                    _argumentGenericTypesDict[argument] = new List<GenericTypeInfo> { genericTypeInfo };
                }
            }

            var classToAdd = new ConcreteClass(arguments, assemblyGUID);

            if (concreteClasses.Contains(classToAdd))
            {
                throw new ArgumentException($"The generic Unity.Object '{genericTypeInfo}' already " +
                                            "has the following concrete class in the database: " +
                                            $"{string.Join(", ", arguments.Select(arg => arg.TypeNameAndAssembly))}");
            }

            concreteClasses.Add(classToAdd);

            EditorUtility.SetDirty(this);
        }

        public static void RemoveArgument(ArgumentInfo argument, [CanBeNull] Action<string> removeAssembly)
        {
            Instance.RemoveArgumentImpl(argument, removeAssembly);
        }

        public void RemoveArgumentImpl(ArgumentInfo argument, [CanBeNull] Action<string> removeAssembly)
        {
            if ( ! _argumentGenericTypesDict.TryGetValue(argument, out List<GenericTypeInfo> genericTypeInfos))
                throw new KeyNotFoundException($"Argument '{argument}' was not found in the database.");

            _argumentGenericTypesDict.Remove(argument);

            foreach (GenericTypeInfo genericTypeInfo in genericTypeInfos)
            {
                if ( ! _genericTypeArgumentsDict.TryGetValue(genericTypeInfo, out List<ConcreteClass> concreteClasses))
                    continue;

                for (int i = concreteClasses.Count - 1; i >= 0; i--)
                {
                    ConcreteClass concreteClass = concreteClasses[i];

                    if (concreteClass.Arguments.Contains(argument))
                    {
                        concreteClasses.RemoveAt(i);
                        removeAssembly?.Invoke(concreteClass.AssemblyGUID);
                    }
                }
            }

            EditorUtility.SetDirty(this);
        }

        public static bool RemoveGenericType(GenericTypeInfo genericTypeInfo, [CanBeNull] Action<string> removeAssembly)
        {
            return Instance.RemoveGenericTypeImpl(genericTypeInfo, removeAssembly);
        }

        public bool RemoveGenericTypeImpl(GenericTypeInfo genericTypeInfo, [CanBeNull] Action<string> removeAssembly)
        {
            if ( ! _genericTypeArgumentsDict.TryGetValue(genericTypeInfo, out List<ConcreteClass> concreteClasses))
                throw new KeyNotFoundException($"Unity.Object '{genericTypeInfo}' was not found in the database.");

            removeAssembly?.Invoke(genericTypeInfo.AssemblyGUID);
            _genericTypeArgumentsDict.Remove(genericTypeInfo);

            foreach (ConcreteClass concreteClass in concreteClasses)
            {
                foreach (ArgumentInfo argument in concreteClass.Arguments)
                {
                    if ( ! _argumentGenericTypesDict.TryGetValue(argument, out List<GenericTypeInfo> genericTypeInfos))
                        continue;

                    genericTypeInfos.Remove(genericTypeInfo);

                    if (genericTypeInfos.Count == 0)
                        _argumentGenericTypesDict.Remove(argument);
                }

                removeAssembly?.Invoke(concreteClass.AssemblyGUID);
            }

            EditorUtility.SetDirty(this);
            return concreteClasses.Count != 0;
        }

        public static GenericTypeInfo[] GetReferencedGenericTypes(ArgumentInfo argument) =>
            Instance.GetReferencedGenericTypesImpl(argument);

        public GenericTypeInfo[] GetReferencedGenericTypesImpl(ArgumentInfo argument) =>
            _argumentGenericTypesDict[argument].ToArray();

        public static ConcreteClass[] GetConcreteClasses(GenericTypeInfo genericTypeInfo) =>
            Instance.GetConcreteClassesImpl(genericTypeInfo);

        public ConcreteClass[] GetConcreteClassesImpl(GenericTypeInfo genericTypeInfo) =>
            _genericTypeArgumentsDict[genericTypeInfo].ToArray();

        public static ConcreteClass[] GetConcreteClassesByArgument(GenericTypeInfo genericTypeInfo, ArgumentInfo argument) =>
            Instance.GetConcreteClassesByArgumentImpl(genericTypeInfo, argument);

        public ConcreteClass[] GetConcreteClassesByArgumentImpl(GenericTypeInfo genericTypeInfo, ArgumentInfo argument)
        {
            return _genericTypeArgumentsDict[genericTypeInfo]
                .Where(concreteClass => concreteClass.Arguments.Contains(argument))
                .ToArray();
        }

        public static void UpdateArgumentGUID(ArgumentInfo argument, string newGUID)
        {
            Instance.UpdateArgumentGUIDImpl(argument, newGUID);
        }

        public void UpdateArgumentGUIDImpl(ArgumentInfo argument, string newGUID)
        {
            TemporarilyRemovingArgument(argument, () =>
            {
                _argumentsPool.ChangeItem(ref argument, argumentToChange =>
                {
                    argumentToChange.UpdateGUID(newGUID);
                });
            });
        }

        public static void UpdateArgumentNameAndAssembly(ArgumentInfo argument, Type newType)
        {
            Instance.UpdateArgumentNameAndAssemblyImpl(argument, newType);
        }

        public void UpdateArgumentNameAndAssemblyImpl(ArgumentInfo argument, Type newType)
        {
            TemporarilyRemovingArgument(argument, () =>
            {
                _argumentsPool.ChangeItem(ref argument, argumentToChange =>
                {
                    argumentToChange.UpdateNameAndAssembly(newType);
                });
            });
        }

        public static void UpdateGenericTypeGUID(GenericTypeInfo genericTypeInfo, string newGUID)
        {
            Instance.UpdateGenericTypeGUIDImpl(genericTypeInfo, newGUID);
        }

        public void UpdateGenericTypeGUIDImpl(GenericTypeInfo genericTypeInfo, string newGUID)
        {
            TemporarilyRemovingGenericType(genericTypeInfo, () =>
            {
                _genericTypesPool.ChangeItem(ref genericTypeInfo, genericTypeToChange =>
                {
                    genericTypeToChange.UpdateGUID(newGUID);
                });
            });
        }

        public static void UpdateGenericTypeArgs(GenericTypeInfo genericTypeInfo, string[] newArgNames)
        {
            Instance.UpdateGenericTypeArgsImpl(genericTypeInfo, newArgNames);
        }

        public void UpdateGenericTypeArgsImpl(GenericTypeInfo genericTypeInfo, string[] newArgNames)
        {
            TemporarilyRemovingGenericType(genericTypeInfo, () =>
            {
                _genericTypesPool.ChangeItem(ref genericTypeInfo, genericTypeToChange =>
                {
                    genericTypeToChange.UpdateArgNames(newArgNames);
                });
            });
        }

        public static void UpdateGenericType(GenericTypeInfo genericTypeInfo, Type newType)
        {
            Instance.UpdateGenericTypeImpl(genericTypeInfo, newType);
        }

        public void UpdateGenericTypeImpl(GenericTypeInfo genericTypeInfo, Type newType)
        {
            TemporarilyRemovingGenericType(genericTypeInfo, () =>
            {
                _genericTypesPool.ChangeItem(ref genericTypeInfo, genericTypeToChange =>
                {
                    genericTypeToChange.UpdateNameAndAssembly(newType);
                    genericTypeToChange.UpdateArgNames(newType.GetGenericArguments());
                });
            });
        }

        private void TemporarilyRemovingGenericType(GenericTypeInfo genericTypeInfo, Action updateType)
        {
            _genericTypeArgumentsDict.UpdateKey(genericTypeInfo, updateType);
            EditorUtility.SetDirty(this);
        }

        private void TemporarilyRemovingArgument(ArgumentInfo argument, Action updateArgument)
        {
            _argumentGenericTypesDict.UpdateKey(argument, updateArgument);
            EditorUtility.SetDirty(this);
        }
    }
}