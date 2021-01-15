namespace GenericUnityObjects.Editor
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using GenericUnityObjects.Util;
    using UnityEngine;
    using Object = UnityEngine.Object;

    internal static class ConcreteClassCreator
    {
        public static void CreateConcreteClass<TObject>(string assemblyName, Type genericTypeWithArgs, string assemblyGUID)
            where TObject : Object
        {
            string concreteClassName = $"ConcreteClass_{assemblyGUID}";

            AssemblyBuilder assemblyBuilder = AssemblyCreatorHelper.GetAssemblyBuilder(assemblyName);
            ModuleBuilder moduleBuilder = AssemblyCreatorHelper.GetModuleBuilder(assemblyBuilder, assemblyName);

            TypeBuilder typeBuilder = moduleBuilder.DefineType(concreteClassName, TypeAttributes.NotPublic, genericTypeWithArgs);

            if (typeof(TObject) == typeof(MonoBehaviour))
            {
                string componentName = "Scripts/" + TypeUtility.GetGenericTypeNameWithBrackets(genericTypeWithArgs);
                AssemblyCreatorHelper.AddComponentMenuAttribute(typeBuilder, componentName);
            }

            typeBuilder.CreateType();
            assemblyBuilder.Save($"{assemblyName}.dll");
        }
    }
}