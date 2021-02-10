namespace GenericUnityObjects.Editor.ScriptableObjects.SelectionWindow
{
    using System;
    using SolidUtilities.Editor.Extensions;
    using TypeReferences.Editor.TypeDropdown;
    using Util;

    /// <summary>
    /// A window that shows the type selection dropdown immediately after the creation,
    /// and closes once the type is chosen.
    /// </summary>
    internal class OneTypeSelectionWindow : TypeSelectionWindow
    {
        public Action<Type[]> OnTypeSelected;

        private bool _guiWasSetUp;
        private Type[] _genericParamConstraints;
        private DropdownWindow _dropdownWindow;

        protected override void OnCreate(Action<Type[]> onTypeSelected, string[] genericArgNames, Type[][] genericParamConstraints)
        {
            OnTypeSelected = onTypeSelected;
            _genericParamConstraints = genericParamConstraints[0];
            this.Resize(1f, 1f);
            this.MoveOutOfScreen();
        }

        protected override void OnGUI()
        {
            if (_guiWasSetUp)
                return;

            _guiWasSetUp = true;

            var typeOptionsAttribute = new NonGenericAttribute(_genericParamConstraints);
            var dropdownDrawer = new CenteredTypeDropdownDrawer(null, typeOptionsAttribute, null);
            _dropdownWindow = dropdownDrawer.Draw(type =>
            {
                _dropdownWindow.Close();
                Close();
                OnTypeSelected(new[] { type });
            });
        }
    }
}