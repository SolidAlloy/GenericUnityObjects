namespace GenericUnityObjects.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using GeneratedTypesDatabase;
    using GenericUnityObjects.Util;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Assertions;
    using Object = UnityEngine.Object;

    /// <summary>
    /// A class that gathers data from <see cref="GenerationDatabase{TUnityObject}"/> and
    /// fills <see cref="GenericTypesDatabase{TObject}"/> with items to be used at runtime.
    /// </summary>
    /// <typeparam name="TObject"> A type derived from <see cref="UnityEngine.Object"/>. </typeparam>
    internal static class DictInitializer<TObject>
        where TObject : Object
    {
        public static void Initialize()
        {
            var genericTypes = GenerationDatabase<TObject>.GenericTypeArguments.Keys;
            var dict = new Dictionary<Type, Dictionary<Type[], Type>>(genericTypes.Count);

            foreach (GenericTypeInfo genericTypeInfo in genericTypes)
            {
                CheckSelectorAssembly(genericTypeInfo);

                Type genericType = genericTypeInfo.RetrieveType<TObject>(false);
                var concreteClassesDict = CreateConcreteClassesDict(genericTypeInfo);
                dict.Add(genericType, concreteClassesDict);
            }

            GenericTypesDatabase<TObject>.Initialize(dict);

            // Sometimes, GenericTypesDatabase can find a generated assembly that was not added to the database for some reason.
            GenericTypesDatabase<TObject>.FoundNewType += OnFoundNewType;
        }

        private static void OnFoundNewType(Type genericType, Type derivedType)
        {
            string assemblyName = derivedType.Assembly.GetName().Name;

            var assetGuids = AssetDatabase.FindAssets(assemblyName, new[] { Config.GetAssemblyPathForType(genericType) });

            string assemblyGUID = assetGuids.FirstOrDefault(assetGuid =>
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);

                var fileName = Path.GetFileNameWithoutExtension(assetPath);
                var extension = Path.GetExtension(assetPath);
                return assemblyName == fileName && extension == ".dll";
            });

            if (assemblyGUID == null)
                return;

            Type genericTypeWithoutArgs = genericType.GetGenericTypeDefinition();
            Type[] genericArgs = genericType.GenericTypeArguments;
            var genericTypeInfo = GenericTypeInfo.Instantiate<TObject>(genericTypeWithoutArgs);

            GenerationDatabase<TObject>.AddConcreteClass(genericTypeInfo, genericArgs, assemblyGUID);
        }

        private static void CheckSelectorAssembly(GenericTypeInfo genericTypeInfo)
        {
            if (string.IsNullOrEmpty(genericTypeInfo.AssemblyGUID))
                return;

            string assemblyPath = AssetDatabase.GUIDToAssetPath(genericTypeInfo.AssemblyGUID);

            if (!File.Exists(assemblyPath))
            {
                DebugUtility.Log($"Detected a missing selector assembly for type {genericTypeInfo.TypeFullName}, adding it back.");
                FailedAssembliesChecker.MissingSelectors.Add(genericTypeInfo);
                return;
            }

            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(assemblyPath);

            if (script == null)
                FailedAssembliesChecker.FailedAssemblyPaths.Add(assemblyPath);
        }

        private static Dictionary<Type[], Type> CreateConcreteClassesDict(GenericTypeInfo genericTypeInfo)
        {
            var concreteClasses = GenerationDatabase<TObject>.GetConcreteClasses(genericTypeInfo);
            var concreteClassesDict = new Dictionary<Type[], Type>(concreteClasses.Length, default(TypeArrayComparer));

            foreach (ConcreteClass concreteClass in concreteClasses)
            {
                if (!TryGetConcreteClassType(genericTypeInfo, concreteClass, out Type value))
                    continue;

                var key = GetConcreteClassArguments(concreteClass);
                concreteClassesDict.Add(key, value);
            }

            return concreteClassesDict;
        }

        private static Type[] GetConcreteClassArguments(ConcreteClass concreteClass)
        {
            int argsLength = concreteClass.Arguments.Length;

            Type[] arguments = new Type[argsLength];

            for (int i = 0; i < argsLength; i++)
            {
                var type = concreteClass.Arguments[i].RetrieveType<TObject>();
                Assert.IsNotNull(type);
                arguments[i] = type;
            }

            return arguments;
        }

        private static bool TryGetConcreteClassType(GenericTypeInfo genericTypeInfo, ConcreteClass concreteClass, out Type type)
        {
            type = null;
            string assemblyPath = AssetDatabase.GUIDToAssetPath(concreteClass.AssemblyGUID);

            // This means the assembly was physically removed, so it shouldn't be in the database anymore.
            if ( ! File.Exists(assemblyPath))
            {
                GenerationDatabase<TObject>.RemoveConcreteClass(genericTypeInfo, concreteClass);
                return false;
            }

            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(assemblyPath);

            if (script == null)
            {
                FailedAssembliesChecker.FailedAssemblyPaths.Add(assemblyPath);
                return false;
            }

            type = script.GetClass();
            return true;
        }
    }
}