namespace GenericUnityObjects.Editor.MonoBehaviour
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using GenericUnityObjects.Util;
    using ScriptableObject;
    using SolidUtilities.Editor.Helpers;
    using SolidUtilities.Extensions;
    using SolidUtilities.Helpers;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Assertions;
    using Util;

    internal class GenericTypesAnalyzer<TDatabase>
        where TDatabase : GenerationDatabase<TDatabase>
    {
        private bool _needsAssetDatabaseRefresh;

        public void RefreshDatabaseIfNeeded(Action doStuff)
        {
            _needsAssetDatabaseRefresh = false;

            doStuff();

            if (_needsAssetDatabaseRefresh)
                AssetDatabase.Refresh();
        }

        public void CheckArguments()
        {
            foreach (ArgumentInfo argument in GenerationDatabase<TDatabase>.Arguments)
            {
                if (argument.RetrieveType<TDatabase>(out Type type, out bool retrievedFromGUID))
                {
                    if (retrievedFromGUID)
                    {
                        UpdateArgumentTypeName(argument, type);
                    }
                }
                else
                {
                    GenerationDatabase<TDatabase>.RemoveArgument(argument, RemoveAssembly);
                }
            }
        }

        public void CheckBehaviours()
        {
            var oldBehaviours = GenerationDatabase<TDatabase>.GenericUnityObjects;
            var newBehaviours = TypeCache.GetTypesDerivedFrom<MonoBehaviour>()
                .Where(type => type.IsGenericType && ! type.IsAbstract)
                .Select(type => new GenericTypeInfo(type))
                .ToArray();

            int oldBehavioursLength = oldBehaviours.Length;
            int newBehavioursLength = newBehaviours.Length;

            // Optimizations for common cases.
            if (oldBehavioursLength == 0 && newBehavioursLength == 0)
            {
                return;
            }

            if (oldBehavioursLength == 0)
            {
                newBehaviours.ForEach(AddNewBehaviour);
                return;
            }

            if (newBehavioursLength == 0)
            {
                oldBehaviours.ForEach(RemoveBehaviour);
                return;
            }

            var oldTypesSet = new HashSet<GenericTypeInfo>(oldBehaviours);
            var newTypesSet = new HashSet<GenericTypeInfo>(newBehaviours);

            var oldBehavioursOnly = oldTypesSet.ExceptWithAndCreateNew(newTypesSet).ToList();
            var newBehavioursOnly = newTypesSet.ExceptWithAndCreateNew(oldTypesSet).ToArray();

            foreach (GenericTypeInfo newBehaviour in newBehavioursOnly)
            {
                bool foundMatching = false;

                for (int i = 0; i < oldBehavioursOnly.Count; i++)
                {
                    GenericTypeInfo oldBehaviour = oldBehavioursOnly[i];

                    if (newBehaviour.GUID == oldBehaviour.GUID)
                    {
                        if (newBehaviour.TypeNameAndAssembly == oldBehaviour.TypeNameAndAssembly)
                        {
                            UpdateBehaviourArgNames(oldBehaviour, newBehaviour.ArgNames, newBehaviour.Type);
                        }
                        else
                        {
                            UpdateBehaviourTypeName(oldBehaviour, newBehaviour.Type);
                        }

                        oldBehavioursOnly.Remove(oldBehaviour);
                        foundMatching = true;
                        break;
                    }

                    if (newBehaviour.TypeNameAndAssembly == oldBehaviour.TypeNameAndAssembly)
                    {
                        // new type GUID is empty -> leave the old GUID
                        if (!string.IsNullOrEmpty(newBehaviour.GUID))
                        {
                            // new type GUID is not empty -> update old GUID
                            UpdateBehaviourGUID(oldBehaviour, newBehaviour.GUID);
                        }

                        if (newBehaviour.ArgNames.SequenceEqual(oldBehaviour.ArgNames))
                        {
                            UpdateBehaviourArgNames(oldBehaviour, newBehaviour.ArgNames, newBehaviour.Type);
                        }

                        oldBehavioursOnly.Remove(oldBehaviour);
                        foundMatching = true;
                        break;
                    }
                }

                // There is no matching old type info, so a completely new assembly must be added for this type
                if ( ! foundMatching)
                {
                    AddNewBehaviour(newBehaviour);
                }
            }

            oldBehavioursOnly.ForEach(RemoveBehaviour);
        }

        public void CheckScriptableObjects()
        {
            var newScriptableObjects = TypeCache.GetTypesDerivedFrom<GenericScriptableObject>()
                .Where(type => type.IsGenericType && ! type.IsAbstract)
                .ToArray();

            int typesCount = newScriptableObjects.Length;

            if (newScriptableObjects.Length == 0)
            {
                if (PersistentStorage.MenuItemMethods.Length == 0)
                {
                    // delete assembly and stop
                    return;
                }
                else
                {
                    // nothing to delete or create
                    return;
                }
            }

            var newMenuItemMethods = new List<MenuItemMethod>(typesCount);

            for (int i = 0; i < typesCount; ++i)
            {
                Type type = newScriptableObjects[i];

                var assetMenuAttribute = type.GetCustomAttribute<CreateGenericAssetMenuAttribute>();
                if (assetMenuAttribute == null)
                    continue;

                newMenuItemMethods.Add(new MenuItemMethod(
                    assetMenuAttribute.FileName,
                    assetMenuAttribute.MenuName,
                    assetMenuAttribute.Order,
                    type));
            }

            if (newMenuItemMethods.Count == 0)
            {
                // delete assembly and stop
                return;
            }

            var oldTypesSet = new HashSet<MenuItemMethod>(PersistentStorage.MenuItemMethods);
            var newTypesSet = new HashSet<MenuItemMethod>(newMenuItemMethods);

            if (oldTypesSet.SetEquals(newTypesSet))
                return;

            // re-generate assembly
        }

        public Dictionary<Type, Dictionary<Type[], Type>> GetDictForInitialization()
        {
            var behaviours = GenerationDatabase<TDatabase>.GenericUnityObjects;
            var dict = new Dictionary<Type, Dictionary<Type[], Type>>(behaviours.Length);

            foreach (GenericTypeInfo behaviourInfo in behaviours)
            {
                GenerationDatabase<TDatabase>.TryGetConcreteClasses(behaviourInfo, out var concreteClasses);

                Type behaviourType = behaviourInfo.RetrieveType<TDatabase>();

                var concreteClassesDict = new Dictionary<Type[], Type>(concreteClasses.Length, default(TypeArrayComparer));

                foreach (ConcreteClass concreteClass in concreteClasses)
                {
                    int argsLength = concreteClass.Arguments.Length;

                    Type[] key = new Type[argsLength];

                    for (int i = 0; i < argsLength; i++)
                    {
                        var type = concreteClass.Arguments[i].RetrieveType<TDatabase>();
                        Assert.IsNotNull(type);
                        key[i] = type;
                    }

                    string assemblyPath = AssetDatabase.GUIDToAssetPath(concreteClass.AssemblyGUID);
                    var script = AssetDatabase.LoadAssetAtPath<MonoScript>(assemblyPath);

                    // There was once NullReferenceReference here because Unity lost a MonoScript asset connected to
                    // the concrete class assembly. Would be great to find a consistent reproduction of the issue.
                    Type value = script.GetClass();

                    concreteClassesDict.Add(key, value);
                }

                dict.Add(behaviourType, concreteClassesDict);
            }

            return dict;
        }

        private void UpdateBehaviourGUID(GenericTypeInfo behaviour, string newGUID)
        {
            DebugUtil.Log($"Behaviour GUID updated: {behaviour.GUID} => {newGUID}");
            GenerationDatabase<TDatabase>.UpdateBehaviourGUID(ref behaviour, newGUID);
        }

        private void RemoveBehaviour(GenericTypeInfo behaviour)
        {
            DebugUtil.Log($"Behaviour removed: {behaviour.TypeFullName}");
            GenerationDatabase<TDatabase>.RemoveGenericBehaviour(behaviour, RemoveAssembly);
        }

        private void UpdateArgumentTypeName(ArgumentInfo argument, Type newType)
        {
            // Retrieve the array of generic arguments where the old argument was listed
            bool behavioursSuccess = GenerationDatabase<TDatabase>.TryGetReferencedBehaviours(argument, out GenericTypeInfo[] referencedBehaviours);

            // update argument typename in database before updating assemblies and trying to find behaviour because behaviour might also need to be updated, and the argument should already be new
            GenerationDatabase<TDatabase>.UpdateArgumentNameAndAssembly(ref argument, newType);

            Assert.IsTrue(behavioursSuccess);

            foreach (GenericTypeInfo behaviour in referencedBehaviours)
            {
                // Retrieve type of the behaviour
                // If type cannot be retrieved, try finding by GUID
                // If GUID didn't help, remove the whole collection of assemblies
                if (behaviour.RetrieveType<TDatabase>(out Type behaviourType, out bool retrievedFromGUID))
                {
                    if (retrievedFromGUID)
                    {
                        UpdateBehaviourTypeName(behaviour, behaviourType);
                    }
                }
                else
                {
                    GenerationDatabase<TDatabase>.RemoveGenericBehaviour(behaviour, RemoveAssembly);
                    continue; // concrete classes are already removed, no need to iterate through them.
                }

                bool concreteClassesSuccess = GenerationDatabase<TDatabase>.TryGetConcreteClassesByArgument(behaviour, argument, out ConcreteClass[] concreteClasses);

                Assert.IsTrue(concreteClassesSuccess);

                foreach (ConcreteClass concreteClass in concreteClasses)
                {
                    UpdateConcreteClassAssembly(behaviourType, concreteClass);
                }
            }
        }

        private void UpdateBehaviourArgNames(GenericTypeInfo behaviour, string[] newArgNames, Type newType)
        {
            DebugUtil.Log($"Behaviour args updated: '{string.Join(", ", behaviour.ArgNames)}' => '{string.Join(", ", behaviour.ArgNames)}'");

            GenerationDatabase<TDatabase>.UpdateBehaviourArgs(ref behaviour, newArgNames);

            UpdateSelectorAssembly(behaviour.AssemblyGUID, newType);
        }

        private void UpdateBehaviourTypeName(GenericTypeInfo behaviour, Type newType)
        {
            DebugUtil.Log($"Behaviour typename updated: {behaviour.TypeFullName} => {newType.FullName}");

            bool success = GenerationDatabase<TDatabase>.TryGetConcreteClasses(behaviour, out ConcreteClass[] concreteClasses);

            Assert.IsTrue(success);

            // Update database before operating on assemblies
            GenerationDatabase<TDatabase>.UpdateBehaviourNameAndAssembly(ref behaviour, newType);

            // update selector assembly
            UpdateSelectorAssembly(behaviour.AssemblyGUID, newType);

            // update concrete classes assemblies
            foreach (ConcreteClass concreteClass in concreteClasses)
            {
                UpdateConcreteClassAssembly(newType, concreteClass);
            }
        }

        private string GetSelectorAssemblyName(Type genericTypeWithoutArgs) => genericTypeWithoutArgs.FullName.MakeClassFriendly();

        private void UpdateSelectorAssembly(string selectorAssemblyGUID, Type newType)
        {
            string newAssemblyName = GetSelectorAssemblyName(newType);

            ReplaceAssembly(selectorAssemblyGUID, newAssemblyName, () =>
            {
                CreateSelectorAssembly(newType, newAssemblyName);
            });
        }

        private void CreateSelectorAssembly(Type genericTypeWithoutArgs, string assemblyName)
        {
            Type[] genericArgs = genericTypeWithoutArgs.GetGenericArguments();
            string componentName = "Scripts/" + CreatorUtil.GetShortNameWithBrackets(genericTypeWithoutArgs, genericArgs);
            AssemblyCreator.CreateSelectorAssembly(assemblyName, genericTypeWithoutArgs, componentName);
        }

        private void AddNewBehaviour(GenericTypeInfo behaviour)
        {
            DebugUtil.Log($"Behaviour added: {behaviour.TypeFullName}");

            Type behaviourType = behaviour.Type;
            Assert.IsNotNull(behaviourType);

            string assemblyName = GetSelectorAssemblyName(behaviourType);
            CreateSelectorAssembly(behaviourType, assemblyName);

            string assemblyPath = $"{Config.AssembliesDirPath}/{assemblyName}.dll";
            behaviour.AssemblyGUID = AssemblyGeneration.ImportAssemblyAsset(assemblyPath);
            _needsAssetDatabaseRefresh = true;

            GenerationDatabase<TDatabase>.AddGenericBehaviour(behaviour);
        }

        private void RemoveAssembly(string assemblyGUID)
        {
            string assemblyPath = AssetDatabase.GUIDToAssetPath(assemblyGUID);
            string mdbPath = $"{assemblyPath}.mdb";
            AssetDatabase.DeleteAsset(assemblyPath);
            AssetDatabase.DeleteAsset(mdbPath);
            _needsAssetDatabaseRefresh = true;
        }

        private void UpdateConcreteClassAssembly(Type behaviourType, ConcreteClass concreteClass)
        {
            if ( ! GetArgumentTypes(concreteClass, out Type[] argumentTypes))
                return;

            UpdateConcreteClassAssembly(behaviourType, argumentTypes, concreteClass);
            LogHelper.RemoveLogEntriesByMode(LogModes.EditorErrors);
            LogHelper.RemoveLogEntriesByMode(LogModes.UserAndEditorWarnings);
        }

        private bool GetArgumentTypes(ConcreteClass concreteClass, out Type[] argumentTypes)
        {
            var arguments = concreteClass.Arguments;
            int argumentsLength = arguments.Length;

            argumentTypes = new Type[argumentsLength];

            for (int i = 0; i < argumentsLength; i++)
            {
                ArgumentInfo argument = arguments[i];

                if (argument.RetrieveType<TDatabase>(out Type type, out bool retrievedFromGUID))
                {
                    if (retrievedFromGUID)
                    {
                        UpdateArgumentTypeName(argument, type);
                    }

                    argumentTypes[i] = type;
                }
                else
                {
                    GenerationDatabase<TDatabase>.RemoveArgument(argument, RemoveAssembly);

                    // Since one of the arguments was not found, the assembly associated with the concrete class
                    // already has been removed, and there is no need to try updating it.
                    return false;
                }
            }

            return true;
        }

        private void UpdateConcreteClassAssembly(Type behaviourType, Type[] argumentTypes, ConcreteClass concreteClass)
        {
            string newAssemblyName;

            try
            {
                newAssemblyName = ConcreteClassCreator.GetConcreteClassAssemblyName(behaviourType, argumentTypes);
            }
            catch (TypeLoadException)
            {
                return;
            }

            ReplaceAssembly(concreteClass.AssemblyGUID, newAssemblyName, () =>
            {
                ConcreteClassCreator.CreateConcreteClassAssembly(behaviourType, argumentTypes, newAssemblyName);
            });
        }

        private void ReplaceAssembly(string assemblyGUID, string newAssemblyName, Action createAssembly)
        {
            string oldAssemblyPath = AssetDatabase.GUIDToAssetPath(assemblyGUID);
            string oldAssemblyPathWithoutExtension = RemoveDLLExtension(oldAssemblyPath);
            File.Delete(oldAssemblyPath);
            File.Delete($"{oldAssemblyPathWithoutExtension}.dll.mdb");

            createAssembly();

            string newAssemblyPathWithoutExtension = $"{Config.AssembliesDirPath}/{newAssemblyName}";
            File.Move($"{oldAssemblyPathWithoutExtension}.dll.meta", $"{newAssemblyPathWithoutExtension}.dll.meta");
            File.Move($"{oldAssemblyPathWithoutExtension}.dll.mdb.meta", $"{newAssemblyPathWithoutExtension}.dll.mdb.meta");
            _needsAssetDatabaseRefresh = true;
        }

        private string RemoveDLLExtension(string assemblyPath)
        {
            return assemblyPath.Substring(0, assemblyPath.Length - 4);
        }
    }
}