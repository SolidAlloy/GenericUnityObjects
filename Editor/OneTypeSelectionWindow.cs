namespace GenericScriptableObjects.Editor
{
    using System;
    using SolidUtilities.Editor.Extensions;
    using TypeReferences;
    using TypeReferences.Editor.Drawers;
    using UnityEngine;

    internal class OneTypeSelectionWindow : TypeSelectionWindow
    {
        private Action<Type[]> _onTypeSelected;
        private bool _guiWasSetUp;

        protected override void OnCreate(Action<Type[]> onTypeSelected, int typesCount)
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

            var dropdownDrawer = new TypeDropdownDrawer(null, new TypeOptionsAttribute { ExcludeNone = true, SerializableOnly = true }, null);
            dropdownDrawer.Draw(type => _onTypeSelected(new[] { type }));

            _guiWasSetUp = true;
        }
    }
}