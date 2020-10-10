namespace GenericScriptableObjects.Editor
{
    using System;
    using SolidUtilities.Editor.Extensions;
    using TypeReferences;
    using TypeReferences.Editor.Drawers;
    using UnityEditor;
    using UnityEngine;

    public abstract class TypeSelectionWindow : EditorWindow
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

            window.OnCreate(onTypesSelected);
            SetupAndShow(window);
        }

        protected abstract void OnCreate(Action<Type[]> onTypesSelected);

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

    public class OneTypeSelectionWindow : TypeSelectionWindow
    {
        private Action<Type[]> _onTypeSelected;
        private bool _guiWasSetUp;

        protected override void OnCreate(Action<Type[]> onTypeSelected)
        {
            _onTypeSelected = onTypeSelected;
            this.Resize(1f, 1f);
        }

        protected override void OnGUI()
        {
            if (_guiWasSetUp)
                return;

            var windowPos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            position = new Rect(windowPos, position.size);

            var dropdownDrawer = new TypeDropdownDrawer(null, new TypeOptionsAttribute { ExcludeNone = true }, null);
            dropdownDrawer.Draw(type => _onTypeSelected(new[] { type }));

            _guiWasSetUp = true;
        }
    }

    public class MultipleTypeSelectionWindow : TypeSelectionWindow
    {
        private Action<Type[]> _onTypesSelected;

        protected override void OnCreate(Action<Type[]> onTypesSelected)
        {
            _onTypesSelected = onTypesSelected;
            this.Resize(300f, 300f);
        }

        protected override void OnGUI()
        {
            throw new NotImplementedException();
        }
    }
}