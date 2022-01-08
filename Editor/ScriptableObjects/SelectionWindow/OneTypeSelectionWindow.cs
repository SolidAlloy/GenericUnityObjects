namespace GenericUnityObjects.Editor.ScriptableObjects.SelectionWindow
{
    using System;
    using SolidUtilities.Editor;
    using SolidUtilities;
    using TypeReferences.Editor.Drawers;
    using UnityDropdown.Editor;
    using UnityEngine;
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
            var selectionTree = GetSelectionTree(typeOptionsAttribute, onTypeSelected);
            _dropdownWindow = DropdownWindow.Create(selectionTree, DropdownWindowType.Popup, windowHeight: typeOptionsAttribute.DropdownHeight);
        }

        private DropdownTree GetSelectionTree(NonGenericAttribute attribute, Action<Type[]> onTypeSelected)
        {
            var parentDrawer = new TypeDropdownDrawer(null, attribute, null);
            var dropdownItems = parentDrawer.GetDropdownItems();

            var selectionTree = new DropdownTree<Type>(dropdownItems, null, type =>
                {
                    _dropdownWindow.Close();
                    onTypeSelected(new[] { type });
                },
                TypeReferences.Editor.ProjectSettings.SearchbarMinItemsCount, true, attribute.ExcludeNone);

            if (attribute.ExpandAllFolders)
                selectionTree.ExpandAllFolders();

            return selectionTree;
        }
    }
}