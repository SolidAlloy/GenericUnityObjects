namespace GenericScriptableObjects.Editor
{
    using System;
    using UnityEditor;

    internal abstract class TypeSelectionWindow : EditorWindow
    {
        public static void Create(int typesCount, Action<Type[]> onTypesSelected)
        {
            TypeSelectionWindow window;
            if (typesCount == 1)
            {
                window = CreateInstance<OneTypeSelectionWindow>();
            }
            else
            {
                window = CreateInstance<MultipleTypeSelectionWindow>();
            }

            window.OnCreate(onTypesSelected, typesCount);
            SetupAndShow(window);
        }

        protected abstract void OnCreate(Action<Type[]> onTypesSelected, int typesCount);

        protected abstract void OnGUI();

        protected void OnDestroy()
        {
            EditorApplication.projectChanged -= Close;
            AssemblyReloadEvents.beforeAssemblyReload -= Close;
        }

        private static void SetupAndShow(TypeSelectionWindow window)
        {
            EditorApplication.projectChanged += window.Close;
            AssemblyReloadEvents.beforeAssemblyReload += window.Close;
            window.Show();
        }
    }
}