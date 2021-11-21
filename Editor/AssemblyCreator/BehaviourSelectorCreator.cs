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
        public static string CreateSelectorAssemblyImpl(string assemblyName, Type genericBehaviourWithoutArgs, string assemblyGUID)
        {
            using var concreteClassAssembly = AssemblyCreatorHelper.CreateConcreteClassAssembly(assemblyName, $"ClassSelector_{assemblyGUID}", typeof(BehaviourSelector));
            CreateBehaviourTypeProperty(concreteClassAssembly.TypeBuilder, genericBehaviourWithoutArgs);
            AssemblyCreatorHelper.AddComponentMenuAttribute(concreteClassAssembly.TypeBuilder, genericBehaviourWithoutArgs);
            return concreteClassAssembly.Path;
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