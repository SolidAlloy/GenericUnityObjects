namespace GenericUnityObjects.Editor
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using GenericUnityObjects.Util;

    /// <summary>
    /// Emits and saves an assembly of a concrete class that inherits from <see cref="BehaviourSelector"/>.
    /// </summary>
    internal static class BehaviourSelectorCreator
    {
        /// <summary>
        /// Not supposed to be used directly. Instead, use <see cref="AssemblyCreator.CreateSelectorAssembly"/>.
        /// </summary>
        public static void CreateSelectorAssemblyImpl(string assemblyName, Type genericBehaviourWithoutArgs, string assemblyGUID)
        {
            string className = $"ClassSelector_{assemblyGUID}";

            AssemblyBuilder assemblyBuilder = AssemblyCreatorHelper.GetAssemblyBuilder(assemblyName);
            ModuleBuilder moduleBuilder = AssemblyCreatorHelper.GetModuleBuilder(assemblyBuilder, assemblyName);

            TypeBuilder typeBuilder = moduleBuilder.DefineType(className, TypeAttributes.NotPublic, typeof(BehaviourSelector));

            CreateBehaviourTypeProperty(typeBuilder, genericBehaviourWithoutArgs);
            string componentName = "Scripts/" + TypeUtility.GetNiceNameOfGenericTypeDefinition(genericBehaviourWithoutArgs, true);
            AssemblyCreatorHelper.AddComponentMenuAttribute(typeBuilder, componentName);

            typeBuilder.CreateType();

            assemblyBuilder.Save($"{assemblyName}.dll");
        }

        private static void CreateBehaviourTypeProperty(TypeBuilder typeBuilder, Type propertyValue)
        {
            PropertyBuilder property = typeBuilder.DefineProperty(
                "GenericBehaviourType",
                PropertyAttributes.None,
                typeof(Type),
                null);

            MethodBuilder pGet = typeBuilder.DefineMethod(
                "get_GenericBehaviourType",
                MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                typeof(Type),
                Type.EmptyTypes);

            ILGenerator pILGet = pGet.GetILGenerator();

            pILGet.Emit(OpCodes.Ldtoken, propertyValue);
            pILGet.EmitCall(OpCodes.Call, AssemblyCreatorHelper.GetTypeFromHandle, null);
            pILGet.Emit(OpCodes.Ret);

            property.SetGetMethod(pGet);
        }
    }
}