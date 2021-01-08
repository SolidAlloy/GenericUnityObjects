namespace GenericUnityObjects.Editor
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using SolidUtilities.Extensions;
    using SolidUtilities.Helpers;

    internal static class ConcreteClassCreator
    {
        public static void CreateBehaviourConcreteClass(string assemblyName, Type genericTypeWithArgs)
        {
            using var emptyType = new EmptyConcreteClassType(assemblyName, genericTypeWithArgs);
            string componentName = "Scripts/" + GetComponentName(genericTypeWithArgs);
            AssemblyCreatorHelper.AddComponentMenuAttribute(emptyType.TypeBuilder, componentName);
        }

        public static void CreateSOConcreteClass(string assemblyName, Type genericTypeWithArgs)
        {
            // do nothing additional, just create the empty type
            using var emptyType = new EmptyConcreteClassType(assemblyName, genericTypeWithArgs);
        }

        private static string GetComponentName(Type genericTypeWithArgs)
        {
            string typeNameWithoutSuffix = genericTypeWithArgs.Name.StripGenericSuffix();

            var argumentNames = genericTypeWithArgs.GetGenericArguments()
                .Select(argument => argument.FullName)
                .Select(fullName => fullName.ReplaceWithBuiltInName())
                .Select(fullName => fullName.GetSubstringAfterLast('.'));

            return $"{typeNameWithoutSuffix}<{string.Join(",", argumentNames)}>";
        }

        private struct EmptyConcreteClassType : IDisposable
        {
            public readonly TypeBuilder TypeBuilder;

            private readonly AssemblyBuilder _assemblyBuilder;
            private readonly string _assemblyName;

            private bool _disposed;

            public EmptyConcreteClassType(string assemblyName, Type genericTypeWithArgs)
            {
                const string concreteClassName = "ConcreteClass";

                _assemblyName = assemblyName;
                _assemblyBuilder = AssemblyCreatorHelper.GetAssemblyBuilder(assemblyName);
                ModuleBuilder moduleBuilder = AssemblyCreatorHelper.GetModuleBuilder(_assemblyBuilder, assemblyName);

                TypeBuilder = moduleBuilder.DefineType(concreteClassName, TypeAttributes.NotPublic, genericTypeWithArgs);
                _disposed = false;
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                TypeBuilder.CreateType();
                _assemblyBuilder.Save($"{_assemblyName}.dll");
                _disposed = true;
            }
        }
    }
}