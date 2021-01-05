namespace GenericUnityObjects.Editor
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    internal static partial class AssemblyCreator
    {
        private static class ConcreteClassCreator
        {
            public static Type CreateConcreteClassImpl(string assemblyName, Type genericBehaviourWithArgs, string componentName)
            {
                const string concreteClassName = "ConcreteClass";

                AssemblyBuilder assemblyBuilder = GetAssemblyBuilder(assemblyName);
                ModuleBuilder moduleBuilder = GetModuleBuilder(assemblyBuilder, assemblyName);

                TypeBuilder typeBuilder = moduleBuilder.DefineType(concreteClassName, TypeAttributes.NotPublic, genericBehaviourWithArgs);

                AddComponentMenuAttribute(typeBuilder, componentName);

                Type type = typeBuilder.CreateType();

                assemblyBuilder.Save($"{assemblyName}.dll");

                return type;
            }
        }
    }
}