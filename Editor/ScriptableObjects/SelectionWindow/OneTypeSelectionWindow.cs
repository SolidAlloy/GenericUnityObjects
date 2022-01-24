namespace GenericUnityObjects.Editor.ScriptableObjects.SelectionWindow
{
    using System;
    using System.Linq;
    using TypeReferences.Editor.Drawers;
    using UnityDropdown.Editor;
    using Util;

    /// <summary>
    /// A window that shows the type selection dropdown immediately after the creation,
    /// and closes once the type is chosen.
    /// </summary>
    internal class OneTypeSelectionWindow : ITypeSelectionWindow
    {
        private DropdownWindow _dropdownWindow;

        public void OnCreate(Action<Type[]> onTypeSelected, string[] genericArgNames, Type[][] genericParamConstraints)
        {
            var typeOptionsAttribute = new NonGenericAttribute(genericParamConstraints[0]);
            var dropdownTree = GetDropdownTree(typeOptionsAttribute, onTypeSelected);
            _dropdownWindow = dropdownTree.ShowAsContext(typeOptionsAttribute.DropdownHeight);
        }

        private DropdownMenu GetDropdownTree(NonGenericAttribute attribute, Action<Type[]> onTypeSelected)
        {
            var parentDrawer = new TypeDropdownDrawer(null, attribute, null);
            var dropdownItems = parentDrawer.GetDropdownItems().ToList();

            var dropdownMenu = new DropdownMenu<Type>(dropdownItems, type =>
                {
                    _dropdownWindow.Close();
                    onTypeSelected(new[] { type });
                },
                TypeReferences.Editor.ProjectSettings.SearchbarMinItemsCount, true, attribute.ShowNoneElement);

            if (attribute.ExpandAllFolders)
                dropdownMenu.ExpandAllFolders();

            return dropdownMenu;
        }
    }
}