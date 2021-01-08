namespace GenericUnityObjects.Editor
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using SolidUtilities.Extensions;
    using SolidUtilities.Helpers;
    using Object = UnityEngine.Object;

    internal static class ConcreteClassCreator
    {
        public static void CreateConcreteClass<TObject>(string assemblyName, Type genericTypeWithArgs)
            where TObject : Object
        {
            const string concreteClassName = "ConcreteClass";

            AssemblyBuilder assemblyBuilder = AssemblyCreatorHelper.GetAssemblyBuilder(assemblyName);
            ModuleBuilder moduleBuilder = AssemblyCreatorHelper.GetModuleBuilder(assemblyBuilder, assemblyName);

            TypeBuilder typeBuilder = moduleBuilder.DefineType(concreteClassName, TypeAttributes.NotPublic, genericTypeWithArgs);

            if (typeof(TObject) == typeof(UnityEngine.MonoBehaviour))
            {
                string componentName = "Scripts/" + GetComponentName(genericTypeWithArgs);
                AssemblyCreatorHelper.AddComponentMenuAttribute(typeBuilder, componentName);
            }

            typeBuilder.CreateType();
            assemblyBuilder.Save($"{assemblyName}.dll");
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
    }
}