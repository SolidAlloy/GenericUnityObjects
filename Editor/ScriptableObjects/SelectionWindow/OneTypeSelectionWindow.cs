namespace GenericUnityObjects.Editor.ScriptableObjects.SelectionWindow
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
        public Action<Type[]> OnTypeSelected;

        private bool _guiWasSetUp;
        private Type[] _genericParamConstraints;
        private DropdownWindow _dropdownWindow;

        protected override void OnCreate(Action<Type[]> onTypeSelected, Type[][] genericParamConstraints)
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
            _dropdownWindow = dropdownDrawer.Draw(type => OnTypeSelected(new[] { type }));

            _dropdownWindow.OnClose += () =>
            {
                try { Close(); }
                catch (NullReferenceException) { }
            };
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // I caught a bug_ where _dropdownWindow was destroyed before OneTypeSelectionWindow for some reason,
            // hence the null check.
            if (_dropdownWindow != null)
                _dropdownWindow.OnClose -= Close;
        }
    }
}