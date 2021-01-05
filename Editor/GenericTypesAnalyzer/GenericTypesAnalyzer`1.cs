namespace GenericUnityObjects.Editor
{
    using System;
    using System.Collections.Generic;
    using GeneratedTypesDatabase;
    using GenericUnityObjects.Util;
    using UnityEditor;
    using UnityEngine.Assertions;

    internal static partial class GenericTypesAnalyzer<TDatabase>
        where TDatabase : GenerationDatabase<TDatabase>
    {
        public static Dictionary<Type, Dictionary<Type[], Type>> GetDictForInitialization()
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

        public static bool CheckBehaviours() => BehavioursChecker.CheckBehavioursImpl();

        public static bool CheckArguments() => ArgumentsChecker.CheckArgumentsImpl();

        public static bool CheckScriptableObjects() => ScriptableObjectsChecker.CheckScriptableObjectsImpl();

        public static bool CheckMenuItems() => MenuItemsChecker.CheckMenuItemsImpl();
    }
}