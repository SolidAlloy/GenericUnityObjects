namespace GenericUnityObjects.Editor.ScriptableObjects
{
    using System;
    using System.Linq;
    using GenericUnityObjects.Util;
    using SelectionWindow;
    using UnityEditor;

    /// <summary>
    /// A window where you can choose the generic argument types for a <see cref="GenericScriptableObject"/> asset.
    /// </summary>
    internal abstract class TypeSelectionWindow : EditorWindow
    {
        /// <summary>Creates and shows the <see cref="TypeSelectionWindow"/>.</summary>
        /// <param name="genericTypeWithoutArgs">Generic type definition (i.e. without concrete generic arguments).</param>
        /// <param name="onTypesSelected">The action to do when all the types are chosen.</param>
        public static void Create(Type genericTypeWithoutArgs, Action<Type[]> onTypesSelected)
        {
            TypeSelectionWindow window;

            var genericParamConstraints = genericTypeWithoutArgs.GetGenericArguments()
                .Select(type => type.GetGenericParameterConstraints())
                .ToArray();

            var genericArgNames = TypeUtility.GetNiceArgsOfGenericType(genericTypeWithoutArgs);

            if (genericParamConstraints.Length == 1)
            {
                window = CreateInstance<OneTypeSelectionWindow>();
            }
            else
            {
                window = CreateInstance<MultipleTypeSelectionWindow>();
            }

            window.OnCreate(onTypesSelected, genericArgNames, genericParamConstraints);
            SetupAndShow(window);
        }

        protected abstract void OnCreate(Action<Type[]> onTypesSelected, string[] genericArgNames, Type[][] genericParamConstraints);

        protected abstract void OnGUI();

        protected virtual void OnDestroy()
        {
            EditorApplication.projectChanged -= Close;
            EditorApplication.quitting -= Close;
            AssemblyReloadEvents.beforeAssemblyReload -= Close;
        }

        private static void SetupAndShow(EditorWindow window)
        {
            EditorApplication.projectChanged += window.Close;
            EditorApplication.quitting += window.Close;
            AssemblyReloadEvents.beforeAssemblyReload += window.Close;
            window.Show();
        }
    }
}