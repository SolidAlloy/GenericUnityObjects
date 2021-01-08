namespace GenericUnityObjects.Editor
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using GenericUnityObjects.Util;

    internal static class BehaviourSelectorCreator
    {
        public static void CreateSelectorAssemblyImpl(string assemblyName, Type genericBehaviourWithoutArgs)
        {
            const string className = "ClassSelector";

            AssemblyBuilder assemblyBuilder = AssemblyCreatorHelper.GetAssemblyBuilder(assemblyName);
            ModuleBuilder moduleBuilder = AssemblyCreatorHelper.GetModuleBuilder(assemblyBuilder, assemblyName);

            TypeBuilder typeBuilder = moduleBuilder.DefineType(className, TypeAttributes.NotPublic, typeof(BehaviourSelector));

            CreateBehaviourTypeProperty(typeBuilder, genericBehaviourWithoutArgs);
            string componentName = "Scripts/" + TypeUtility.GetShortNameWithBrackets(genericBehaviourWithoutArgs);
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