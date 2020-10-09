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
        public static void Create(Action<Type> onTypeSelected)
        {
            var window = CreateInstance<OneTypeSelectionWindow>();
            window.OnCreate(onTypeSelected);
            SetupAndShow(window);
        }

        public static void Create(Action<Type, Type> onTwoTypesSelected)
        {
            var window = CreateInstance<TwoTypeSelectionWindow>();
            window.OnCreate(onTwoTypesSelected);
            SetupAndShow(window);
        }

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
        private Action<Type> _onTypeSelected;
        private bool _guiWasSetUp;

        public void OnCreate(Action<Type> onTypeSelected)
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
            dropdownDrawer.Draw(type => _onTypeSelected(type));

            _guiWasSetUp = true;
        }
    }

    public class TwoTypeSelectionWindow : TypeSelectionWindow
    {
        private Action<Type, Type> _onTypesSelected;

        public void OnCreate(Action<Type, Type> onTypesSelected)
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