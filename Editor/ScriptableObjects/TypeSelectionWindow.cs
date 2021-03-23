namespace GenericUnityObjects.Editor.ScriptableObjects
{
    using System;
    using System.Linq;
    using GenericUnityObjects.Util;
    using SelectionWindow;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// A window where you can choose the generic argument types for a <see cref="GenericScriptableObject"/> asset.
    /// </summary>
    internal abstract class TypeSelectionWindow
    {
        /// <summary>Creates and shows the <see cref="TypeSelectionWindow"/>.</summary>
        /// <param name="genericTypeWithoutArgs">Generic type definition (i.e. without concrete generic arguments).</param>
        /// <param name="onTypesSelected">The action to do when all the types are chosen.</param>
        public static void Create(Type genericTypeWithoutArgs, Action<Type[]> onTypesSelected)
        {
            var genericParamConstraints = genericTypeWithoutArgs.GetGenericArguments()
                .Select(type => type.GetGenericParameterConstraints())
                .ToArray();

            var genericArgNames = TypeUtility.GetNiceArgsOfGenericType(genericTypeWithoutArgs);

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