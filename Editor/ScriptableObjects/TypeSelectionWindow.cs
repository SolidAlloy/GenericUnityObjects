namespace GenericUnityObjects.Editor.ScriptableObjects
{
    using System;
    using System.Linq;
    using SelectionWindow;
    using SolidUtilities;
    using UnityEngine;

    /// <summary>
    /// A window where you can choose the generic argument types for a <see cref="GenericScriptableObject"/> asset.
    /// </summary>
    internal static class TypeSelectionWindow
    {
        /// <summary>Creates and shows the <see cref="TypeSelectionWindow"/>.</summary>
        /// <param name="genericTypeWithoutArgs">Generic type definition (i.e. without concrete generic arguments).</param>
        /// <param name="onTypesSelected">The action to do when all the types are chosen.</param>
        public static void Create(Type genericTypeWithoutArgs, Action<Type[]> onTypesSelected)
        {
            var genericParamConstraints = genericTypeWithoutArgs.GetGenericArguments()
                .Select(type => type.GetGenericParameterConstraints())
                .ToArray();

            var genericArgNames = TypeHelper.GetNiceArgsOfGenericType(genericTypeWithoutArgs);

            ITypeSelectionWindow window;

            if (genericParamConstraints.Length == 1)
            {
                window = new OneTypeSelectionWindow();
            }
            else
            {
                window = ScriptableObject.CreateInstance<MultipleTypeSelectionWindow>();
            }

            window.OnCreate(onTypesSelected, genericArgNames, genericParamConstraints);
        }
    }
}