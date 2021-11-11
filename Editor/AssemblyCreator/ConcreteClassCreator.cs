namespace GenericUnityObjects.Editor
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using GenericUnityObjects.Util;
    using UnityEngine;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Emits and saves an assembly of a concrete class that inherits from a specific generic UnityEngine.Object
    /// </summary>
    internal static class ConcreteClassCreator
    {
        /// <summary>
        /// Not supposed to be used directly. Instead, use <see cref="AssemblyCreator.CreateConcreteClass{TObject}"/>.
        /// </summary>
        public static string CreateConcreteClass<TObject>(string assemblyName, Type genericTypeWithArgs, string assemblyGUID)
            where TObject : Object
        {
            using var concreteClassAssembly = AssemblyCreatorHelper.CreateConcreteClassAssembly(Config.AssembliesDirPath, assemblyName, $"ConcreteClass_{assemblyGUID}", genericTypeWithArgs);

            AssemblyCreatorHelper.AddChildrenAttributes(concreteClassAssembly.TypeBuilder, genericTypeWithArgs);

            if (typeof(TObject) == typeof(MonoBehaviour))
            {
                AssemblyCreatorHelper.AddComponentMenuAttribute(concreteClassAssembly.TypeBuilder, genericTypeWithArgs);
            }

            return concreteClassAssembly.Path;
        }
    }
}