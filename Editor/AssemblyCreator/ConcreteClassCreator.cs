namespace GenericUnityObjects.Editor
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using GenericUnityObjects.Util;
    using SolidUtilities.Extensions;
    using UnityEngine;
    using UnityEngine.Assertions;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Emits and saves an assembly of a concrete class that inherits from a specific generic UnityEngine.Object
    /// </summary>
    internal static class ConcreteClassCreator
    {
        /// <summary>
        /// Not supposed to be used directly. Instead, use <see cref="AssemblyCreator.CreateConcreteClass{TObject}"/>.
        /// </summary>
        public static void CreateConcreteClass<TObject>(string assemblyName, Type genericTypeWithArgs, string assemblyGUID)
            where TObject : Object
        {
            string concreteClassName = $"ConcreteClass_{assemblyGUID}";

            AssemblyBuilder assemblyBuilder = AssemblyCreatorHelper.GetAssemblyBuilder(assemblyName);
            ModuleBuilder moduleBuilder = AssemblyCreatorHelper.GetModuleBuilder(assemblyBuilder, assemblyName);

            // IgnoresAccessChecksTo-related code was taken from https://github.com/microsoft/vs-streamjsonrpc/blob/main/src/StreamJsonRpc/SkipClrVisibilityChecks.cs
            string genericTypeAssemblyName = genericTypeWithArgs.GetShortAssemblyName();
            SkipVisibilityChecksFor(assemblyBuilder, moduleBuilder, genericTypeAssemblyName);

            TypeBuilder typeBuilder = moduleBuilder.DefineType(concreteClassName, TypeAttributes.NotPublic, genericTypeWithArgs);

            if (typeof(TObject) == typeof(MonoBehaviour))
            {
                string componentName = $"Scripts/{TypeUtility.GetNiceNameOfGenericType(genericTypeWithArgs)}";
                AssemblyCreatorHelper.AddComponentMenuAttribute(typeBuilder, componentName);
            }

            typeBuilder.CreateType();
            assemblyBuilder.Save($"{assemblyName}.dll");
        }

        /// <summary>
        /// The <see cref="AttributeUsageAttribute(AttributeTargets)"/> constructor.
        /// </summary>
        private static readonly ConstructorInfo _attributeUsageCtor = typeof(AttributeUsageAttribute).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(AttributeTargets) }, null)!;

        /// <summary>
        /// The <see cref="AttributeUsageAttribute.AllowMultiple"/> property.
        /// </summary>
        private static readonly PropertyInfo _attributeUsageAllowMultipleProperty = typeof(AttributeUsageAttribute).GetProperty(nameof(AttributeUsageAttribute.AllowMultiple), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;

        /// <summary>
        /// The <see cref="Attribute"/> constructor.
        /// </summary>
        private static readonly ConstructorInfo _attributeBaseClassCtor = typeof(Attribute).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).Single(ctor => ctor.GetParameters().Length == 0);

        /// <summary>
        /// Add an attribute to a dynamic assembly so that the CLR will skip visibility checks
        /// for the assembly with the specified name.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly to skip visibility checks for.</param>
        private static void SkipVisibilityChecksFor(AssemblyBuilder assemblyBuilder, ModuleBuilder moduleBuilder, string assemblyName)
        {
            var cab = new CustomAttributeBuilder(GetMagicAttributeCtor(moduleBuilder), new object[] { assemblyName });
            assemblyBuilder.SetCustomAttribute(cab);
        }

        private static ConstructorInfo _magicAttributeCtor;

        /// <summary>
        /// Gets the constructor to the IgnoresAccessChecksToAttribute, generating the attribute if necessary.
        /// </summary>
        /// <returns>The constructor to the IgnoresAccessChecksToAttribute.</returns>
        private static ConstructorInfo GetMagicAttributeCtor(ModuleBuilder moduleBuilder)
        {
            if (_magicAttributeCtor == null)
            {
                TypeInfo magicAttribute = EmitMagicAttribute(moduleBuilder);
                _magicAttributeCtor = magicAttribute.GetConstructor(new[] { typeof(string) });
            }

            return _magicAttributeCtor!;
        }

        /// <summary>
        /// Defines the special IgnoresAccessChecksToAttribute type in the <see cref="moduleBuilder"/>.
        /// </summary>
        /// <returns>The generated attribute type.</returns>
        private static TypeInfo EmitMagicAttribute(ModuleBuilder moduleBuilder)
        {
            TypeBuilder tb = moduleBuilder.DefineType(
                "System.Runtime.CompilerServices.IgnoresAccessChecksToAttribute",
                TypeAttributes.NotPublic,
                typeof(Attribute));

            var attributeUsage = new CustomAttributeBuilder(
                _attributeUsageCtor,
                new object[] { AttributeTargets.Assembly },
                new[] { _attributeUsageAllowMultipleProperty },
                new object[] { false });
            tb.SetCustomAttribute(attributeUsage);

            ConstructorBuilder cb = tb.DefineConstructor(
                MethodAttributes.Public |
                MethodAttributes.HideBySig |
                MethodAttributes.SpecialName |
                MethodAttributes.RTSpecialName,
                CallingConventions.Standard,
                new Type[] { typeof(string) });
            cb.DefineParameter(1, ParameterAttributes.None, "assemblyName");

            ILGenerator il = cb.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, _attributeBaseClassCtor);
            il.Emit(OpCodes.Ret);

            return tb.CreateTypeInfo()!;
        }
    }
}