namespace GenericUnityObjects.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using GeneratedTypesDatabase;
    using GenericUnityObjects.Util;
    using UnityEditor;
    using UnityEngine.Assertions;
    using Object = UnityEngine.Object;

    internal static class DictInitializer<TObject>
        where TObject : Object
    {
        public static void Initialize()
        {
            var behaviours = GenerationDatabase<TObject>.GenericTypes;
            var dict = new Dictionary<Type, Dictionary<Type[], Type>>(behaviours.Length);

            foreach (GenericTypeInfo behaviourInfo in behaviours)
            {
                var concreteClasses = GenerationDatabase<TObject>.GetConcreteClasses(behaviourInfo);

                Type behaviourType = behaviourInfo.RetrieveType<TObject>();

                var concreteClassesDict = new Dictionary<Type[], Type>(concreteClasses.Length, default(TypeArrayComparer));

                foreach (ConcreteClass concreteClass in concreteClasses)
                {
                    int argsLength = concreteClass.Arguments.Length;

                    Type[] key = new Type[argsLength];

                    for (int i = 0; i < argsLength; i++)
                    {
                        var type = concreteClass.Arguments[i].RetrieveType<TObject>();
                        Assert.IsNotNull(type);
                        key[i] = type;
                    }

                    string assemblyPath = AssetDatabase.GUIDToAssetPath(concreteClass.AssemblyGUID);

                    // This means the assembly was physically removed, so it shouldn't be in the database anymore.
                    if ( ! File.Exists(assemblyPath))
                    {
                        GenerationDatabase<TObject>.RemoveConcreteClass(behaviourInfo, concreteClass);
                        continue;
                    }

                    var script = AssetDatabase.LoadAssetAtPath<MonoScript>(assemblyPath);

                    // There was once NullReferenceReference here because Unity lost a MonoScript asset connected to
                    // the concrete class assembly. Would be great to find a consistent reproduction of the issue.
                    Type value = script.GetClass();

                    concreteClassesDict.Add(key, value);
                }

                dict.Add(behaviourType, concreteClassesDict);
            }

            GenericTypesDatabase<TObject>.Initialize(dict);
        }
    }
}