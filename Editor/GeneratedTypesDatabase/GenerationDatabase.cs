namespace GenericUnityObjects.Editor.GeneratedTypesDatabase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GenericUnityObjects.Util;
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
        private Dictionary<ArgumentInfo, List<GenericTypeInfo>> _argumentGenericTypesDict;
        private Dictionary<GenericTypeInfo, List<ConcreteClass>> _genericTypeArgumentsDict;
        private Pool<ArgumentInfo> _argumentsPool;
        private Pool<GenericTypeInfo> _genericTypesPool;

        [SerializeField] private ArgumentInfo[] _genericArgumentKeys;
        [SerializeField] private GenericTypeCollection[] _genericTypeValues;
        [SerializeField] private GenericTypeInfo[] _genericTypeKeys;
        [SerializeField] private ConcreteClassCollection[] _genericArgumentValues;

        private bool _shouldSetDirty;

        public static ArgumentInfo[] Arguments => Instance.InstanceArguments;

        public ArgumentInfo[] InstanceArguments => _argumentGenericTypesDict.Keys.ToArray();

        public static GenericTypeInfo[] GenericTypes => Instance.InstanceGenericTypes;

        public GenericTypeInfo[] InstanceGenericTypes => _genericTypeArgumentsDict.Keys.ToArray();

        public static void AddGenericType(GenericTypeInfo genericTypeInfo)
        {
            Instance.AddGenericTypeImpl(genericTypeInfo, out List<ConcreteClass> _);
        }

        public void AddGenericTypeImpl(GenericTypeInfo genericTypeInfo, out List<ConcreteClass> concreteClasses)
        {
            concreteClasses = new List<ConcreteClass>();
            genericTypeInfo = _genericTypesPool.GetOrAdd(genericTypeInfo);
            _genericTypeArgumentsDict.Add(genericTypeInfo, concreteClasses);
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

        public static void RemoveArgument(ArgumentInfo argument, Action<string> assemblyAction)
        {
            Instance.RemoveArgumentImpl(argument, assemblyAction);
        }

        public void RemoveArgumentImpl(ArgumentInfo argument, Action<string> assemblyAction)
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
                        assemblyAction(concreteClass.AssemblyGUID);
                    }
                }
            }

            EditorUtility.SetDirty(this);
        }

        public static bool RemoveGenericType(GenericTypeInfo genericTypeInfo, Action<string> removeAssembly)
        {
            return Instance.RemoveGenericTypeImpl(genericTypeInfo, removeAssembly);
        }

        public bool RemoveGenericTypeImpl(GenericTypeInfo genericTypeInfo, Action<string> removeAssembly)
        {
            if ( ! _genericTypeArgumentsDict.TryGetValue(genericTypeInfo, out List<ConcreteClass> concreteClasses))
                throw new KeyNotFoundException($"Unity.Object '{genericTypeInfo}' was not found in the database.");

            removeAssembly(genericTypeInfo.AssemblyGUID);
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

                removeAssembly(concreteClass.AssemblyGUID);
            }

            EditorUtility.SetDirty(this);
            return concreteClasses.Count != 0;
        }

        // TODO: replace with GetReferencedGenericTypes in tests
        public static bool TryGetReferencedGenericTypes(ArgumentInfo argument, out GenericTypeInfo[] referencedGenericTypeInfos)
        {
            return Instance.TryGetReferencedGenericTypesImpl(argument, out referencedGenericTypeInfos);
        }

        public bool TryGetReferencedGenericTypesImpl(ArgumentInfo argument, out GenericTypeInfo[] referencedGenericTypeInfos)
        {
            bool success = _argumentGenericTypesDict.TryGetValue(argument, out List<GenericTypeInfo> genericTypeInfos);
            referencedGenericTypeInfos = success ? genericTypeInfos.ToArray() : null;
            return success;
        }

        public static GenericTypeInfo[] GetReferencedGenericTypes(ArgumentInfo argument) =>
            Instance.GetReferencedGenericTypesImpl(argument);

        public GenericTypeInfo[] GetReferencedGenericTypesImpl(ArgumentInfo argument) =>
            _argumentGenericTypesDict[argument].ToArray();

        // TODO: replace with GetConcreteClasses in tests
        public static bool TryGetConcreteClasses(GenericTypeInfo genericTypeInfo, out ConcreteClass[] concreteClasses)
        {
            return Instance.TryGetConcreteClassesImpl(genericTypeInfo, out concreteClasses);
        }

        public bool TryGetConcreteClassesImpl(GenericTypeInfo genericTypeInfo, out ConcreteClass[] concreteClasses)
        {
            bool success = _genericTypeArgumentsDict.TryGetValue(genericTypeInfo, out List<ConcreteClass> concreteClassesList);
            concreteClasses = success ? concreteClassesList.ToArray() : null;
            return success;
        }

        public static ConcreteClass[] GetConcreteClasses(GenericTypeInfo genericTypeInfo) =>
            Instance.GetConcreteClassesImpl(genericTypeInfo);

        public ConcreteClass[] GetConcreteClassesImpl(GenericTypeInfo genericTypeInfo) =>
            _genericTypeArgumentsDict[genericTypeInfo].ToArray();

        public static bool TryGetConcreteClassesByArgument(GenericTypeInfo genericTypeInfo, ArgumentInfo argument, out ConcreteClass[] concreteClasses)
        {
            return Instance.TryGetConcreteClassesByArgumentImpl(genericTypeInfo, argument, out concreteClasses);
        }

        // TODO: replace with GetConcreteClassesByArgument in tests
        public bool TryGetConcreteClassesByArgumentImpl(GenericTypeInfo genericTypeInfo, ArgumentInfo argument, out ConcreteClass[] concreteClasses)
        {
            if ( ! _genericTypeArgumentsDict.TryGetValue(genericTypeInfo, out List<ConcreteClass> concreteClassesList))
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

        public static ConcreteClass[] GetConcreteClassesByArgument(GenericTypeInfo genericTypeInfo, ArgumentInfo argument) =>
            Instance.GetConcreteClassesByArgumentImpl(genericTypeInfo, argument);

        public ConcreteClass[] GetConcreteClassesByArgumentImpl(GenericTypeInfo genericTypeInfo, ArgumentInfo argument)
        {
            return _genericTypeArgumentsDict[genericTypeInfo]
                .Where(concreteClass => concreteClass.Arguments.Contains(argument))
                .ToArray();
        }

        public static void UpdateArgumentGUID(ref ArgumentInfo argument, string newGUID)
        {
            Instance.UpdateArgumentGUIDImpl(ref argument, newGUID);
        }

        public void UpdateArgumentGUIDImpl(ref ArgumentInfo argument, string newGUID)
        {
            if (! _argumentGenericTypesDict.TryGetValue(argument, out List<GenericTypeInfo> genericTypeInfos))
                throw new KeyNotFoundException($"Argument '{argument}' was not found in the database.");

            _argumentGenericTypesDict.Remove(argument);

            _argumentsPool.ChangeItem(ref argument, argumentToChange =>
            {
                argumentToChange.UpdateGUID(newGUID);
            });

            _argumentGenericTypesDict.Add(argument, genericTypeInfos);
            EditorUtility.SetDirty(this);
        }

        public static void UpdateArgumentNameAndAssembly(ref ArgumentInfo argument, Type newType)
        {
            Instance.UpdateArgumentNameAndAssemblyImpl(ref argument, newType);
        }

        public void UpdateArgumentNameAndAssemblyImpl(ref ArgumentInfo argument, Type newType)
        {
            if (! _argumentGenericTypesDict.TryGetValue(argument, out List<GenericTypeInfo> behaviours))
                throw new KeyNotFoundException($"Argument '{argument}' was not found in the database.");

            _argumentGenericTypesDict.Remove(argument);

            _argumentsPool.ChangeItem(ref argument, argumentToChange =>
            {
                argumentToChange.UpdateNameAndAssembly(newType);
            });

            _argumentGenericTypesDict.Add(argument, behaviours);
            EditorUtility.SetDirty(this);
        }

        public static void UpdateGenericTypeGUID(ref GenericTypeInfo genericTypeInfo, string newGUID)
        {
            Instance.UpdateGenericTypeGUIDImpl(ref genericTypeInfo, newGUID);
        }

        public void UpdateGenericTypeGUIDImpl(ref GenericTypeInfo genericTypeInfo, string newGUID)
        {
            if (! _genericTypeArgumentsDict.TryGetValue(genericTypeInfo, out List<ConcreteClass> concreteClasses))
                throw new KeyNotFoundException($"Unity.Object '{genericTypeInfo}' was not found in the database.");

            _genericTypeArgumentsDict.Remove(genericTypeInfo);

            _genericTypesPool.ChangeItem(ref genericTypeInfo, genericTypeToChange =>
            {
                genericTypeToChange.UpdateGUID(newGUID);
            });

            _genericTypeArgumentsDict.Add(genericTypeInfo, concreteClasses);
            EditorUtility.SetDirty(this);
        }

        public static void UpdateGenericTypeArgs(ref GenericTypeInfo genericTypeInfo, string[] newArgNames)
        {
            Instance.UpdateGenericTypeArgsImpl(ref genericTypeInfo, newArgNames);
        }

        public void UpdateGenericTypeArgsImpl(ref GenericTypeInfo genericTypeInfo, string[] newArgNames)
        {
            if (! _genericTypeArgumentsDict.TryGetValue(genericTypeInfo, out List<ConcreteClass> concreteClasses))
                throw new KeyNotFoundException($"Unity.Object '{genericTypeInfo}' was not found in the database.");

            _genericTypeArgumentsDict.Remove(genericTypeInfo);

            _genericTypesPool.ChangeItem(ref genericTypeInfo, genericTypeToChange =>
            {
                genericTypeToChange.UpdateArgNames(newArgNames);
            });

            _genericTypeArgumentsDict.Add(genericTypeInfo, concreteClasses);
            EditorUtility.SetDirty(this);
        }

        public static void UpdateGenericTypeNameAndAssembly(ref GenericTypeInfo genericTypeInfo, Type newType)
        {
            Instance.UpdateGenericTypeNameAndAssemblyImpl(ref genericTypeInfo, newType);
        }

        public void UpdateGenericTypeNameAndAssemblyImpl(ref GenericTypeInfo genericTypeInfo, Type newType)
        {
            if (! _genericTypeArgumentsDict.TryGetValue(genericTypeInfo, out List<ConcreteClass> concreteClasses))
                throw new KeyNotFoundException($"Unity.Object '{genericTypeInfo}' was not found in the database.");

            _genericTypeArgumentsDict.Remove(genericTypeInfo);

            _genericTypesPool.ChangeItem(ref genericTypeInfo, genericTypeToChange =>
            {
                genericTypeToChange.UpdateNameAndAssembly(newType);
            });

            _genericTypeArgumentsDict.Add(genericTypeInfo, concreteClasses);
            EditorUtility.SetDirty(this);
        }
    }
}