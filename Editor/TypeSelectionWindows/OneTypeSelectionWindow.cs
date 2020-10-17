namespace GenericScriptableObjects.Editor.TypeSelectionWindows
{
    using System;
    using SolidUtilities.Editor.Extensions;
    using TypeReferences;
    using TypeReferences.Editor.TypeDropdown;
    using Util;

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
            this.Resize(1f, 1f);
            this.MoveOutOfScreen();
        }

        protected override void OnGUI()
        {
            if (_guiWasSetUp)
                return;

            _guiWasSetUp = true;

            TypeOptionsAttribute typeOptionsAttribute = _genericParamConstraints.Length == 0
                ? new TypeOptionsAttribute()
                : new InheritsAttribute(_genericParamConstraints) { ExpandAllFolders = true };

            typeOptionsAttribute.ExcludeNone = true;
            typeOptionsAttribute.SerializableOnly = true;

            var dropdownDrawer = new CenteredTypeDropdownDrawer(null, typeOptionsAttribute, null);
            DropdownWindow dropdownWindow = dropdownDrawer.Draw(type => _onTypeSelected(new[] { type }));
            dropdownWindow.OnClose += Close;
        }
    }
}