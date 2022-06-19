namespace GenericUnityObjects.Editor.GeneratedTypesDatabase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GenericUnityObjects.Util;
    using JetBrains.Annotations;
    using SolidUtilities;
    using UnityEditor;
    using UnityEngine;
    using Util;
    using Object = UnityEngine.Object;

    /// <summary>
    /// A database that holds references to generic types in the project and concrete classes generated for them.
    /// </summary>
    /// <typeparam name="TUnityObject">
    /// Type derived from <see cref="UnityEngine.Object"/>. Currently supports only
    /// <see cref="ScriptableObject"/> and <see cref="MonoBehaviour"/>.
    /// </typeparam>
    /// <remarks>
    /// The database utilizes two dictionaries: [Argument, List{GenericType}] and [GenericType, List{Argument[]}].
    /// This is done for the fast lookup of both argument and generic type. Dictionaries are interconnected with help of pools.
    /// It means that the same Argument object is located in both databases. If it is updated in one database,
    /// it is automatically updated in the second one.
    /// Both ArgumentInfo and GenericTypeInfo fields are not readonly and can be changed. To avoid losing reference to
    /// the object when its field is changed, the class removes the object from the database, changes it, then puts it back.
    /// This allows changing objects while keeping them in the database.
    /// </remarks>
    internal abstract partial class GenerationDatabase<TUnityObject> :
        EditorOnlySingletonSO<GenerationDatabase<TUnityObject>>
        where TUnityObject : Object
    {
        protected FastIterationDictionary<ArgumentInfo, List<GenericTypeInfo>> _argumentGenericTypesDict;
        protected FastIterationDictionary<GenericTypeInfo, List<ConcreteClass>> _genericTypeArgumentsDict;
        protected Pool<ArgumentInfo> _argumentsPool;
        protected Pool<GenericTypeInfo> _genericTypesPool;

        public static IKeysValuesHolder<ArgumentInfo, List<GenericTypeInfo>> ArgumentGenericTypes => Instance._argumentGenericTypesDict;

        public static IKeysValuesHolder<GenericTypeInfo, List<ConcreteClass>> GenericTypeArguments => Instance._genericTypeArgumentsDict;

        public IKeysValuesHolder<ArgumentInfo, List<GenericTypeInfo>> InstanceArgumentGenericTypes => _argumentGenericTypesDict;

        public IKeysValuesHolder<GenericTypeInfo, List<ConcreteClass>> InstanceGenericTypeArguments => _genericTypeArgumentsDict;

        [CanBeNull]
        public static string GetCachedGenericTypeGUID(string typeNameAndAssembly)
            => Instance._genericTypesPool.GetGUID(typeNameAndAssembly);

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

        public static void AddConcreteClass(GenericTypeInfo genericTypeInfo, Type[] genericArgs, string assemblyGUID)
        {
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
            if ( ! _genericTypeArgumentsDict.TryGetValue(genericTypeInfo, out List<ConcreteClass> concreteClasses))
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

        public static void RemoveConcreteClass(GenericTypeInfo genericTypeInfo, ConcreteClass concreteClass)
        {
            Instance.RemoveConcreteClassImpl(genericTypeInfo, concreteClass);
        }

        public void RemoveConcreteClassImpl(GenericTypeInfo genericTypeInfo, ConcreteClass concreteClass)
        {
            if ( ! _genericTypeArgumentsDict.TryGetValue(genericTypeInfo, out List<ConcreteClass> concreteClasses))
            {
                throw new KeyNotFoundException($"Failed to find generic Unity.Object '{genericTypeInfo}' in the database.");
            }

            concreteClasses.Remove(concreteClass);
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

        public static void UpdateGenericType(GenericTypeInfo genericTypeInfo, Action<GenericTypeInfo> updateAction)
        {
            Instance.UpdateGenericTypeImpl(genericTypeInfo, updateAction);
        }

        public void UpdateGenericTypeImpl(GenericTypeInfo genericTypeInfo, Action<GenericTypeInfo> updateAction)
        {
            TemporarilyRemovingGenericType(genericTypeInfo, () => _genericTypesPool.ChangeItem(ref genericTypeInfo, updateAction));
        }

        public static void UpdateGenericType(GenericTypeInfo genericTypeInfo, Type newType)
        {
            Instance.UpdateGenericTypeImpl(genericTypeInfo, info =>
            {
                info.UpdateNameAndAssembly(newType);
                info.UpdateArgNames(newType.GetGenericArguments());
            });
        }

        protected void TemporarilyRemovingGenericType(GenericTypeInfo genericTypeInfo, Action updateType)
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