namespace GenericScriptableObjects.Editor
{
    using System;
    using SolidUtilities.Editor.Extensions;
    using TypeReferences;
    using TypeReferences.Editor.Drawers;
    using UnityEditor;
    using UnityEngine;

    public class TypeSelectionWindow : EditorWindow
    {
        private Action<Type> _onTypeSelected;
        private bool _guiWasSetUp;

        public static void Create(Action<Type> onTypeSelected)
        {
            var window = CreateInstance<TypeSelectionWindow>();
            window.OnCreate(onTypeSelected);
        }

        private void OnCreate(Action<Type> onTypeSelected)
        {
            _onTypeSelected = onTypeSelected;
            this.Resize(1f, 1f);
            Show();

            EditorApplication.projectChanged += Close;
            AssemblyReloadEvents.beforeAssemblyReload += Close;
        }

        private void OnGUI()
        {
            if (_guiWasSetUp)
                return;

            var windowPos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            position = new Rect(windowPos, position.size);

            // TODO : check edge cases: type is null, etc.
            var dropdownDrawer = new TypeDropdownDrawer(null, new TypeOptionsAttribute { ExcludeNone = true }, null);
            dropdownDrawer.Draw(type => _onTypeSelected(type));

            _guiWasSetUp = true;
        }

        private void OnDestroy()
        {
            EditorApplication.projectChanged -= Close;
            AssemblyReloadEvents.beforeAssemblyReload -= Close;
        }
    }
}