namespace GenericScriptableObjects.Editor.TypeSelectionWindows
{
    using System;
    using SolidUtilities.Editor.Extensions;
    using TypeReferences;
    using TypeReferences.Editor.Drawers;
    using UnityEngine;

    /// <summary>
    /// A window that shows the type selection dropdown immediately after the creation,
    /// and closes once the type is chosen.
    /// </summary>
    internal class OneTypeSelectionWindow : TypeSelectionWindow
    {
        private Action<Type[]> _onTypeSelected;
        private bool _guiWasSetUp;
        private Type[] _genericParamConstraints;

        protected override void OnCreate(Action<Type[]> onTypeSelected, Type[][] genericParamConstraints)
        {
            _onTypeSelected = onTypeSelected;
            _genericParamConstraints = genericParamConstraints[0];
            this.CenterOnMainWin();
            this.Resize(1f, 1f);
        }

        protected override void OnGUI()
        {
            if (_guiWasSetUp)
                return;

            // Vector2 windowPos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            // position = new Rect(windowPos, position.size);

            var inheritsAttribute = new InheritsAttribute(_genericParamConstraints)
                { ExcludeNone = true, SerializableOnly = true };

            var dropdownDrawer = new TypeDropdownDrawer(null, inheritsAttribute, null);
            dropdownDrawer.Draw(type => _onTypeSelected(new[] { type }));

            _guiWasSetUp = true;
        }
    }
}