namespace GenericUnityObjects.Editor.ScriptableObjects.SelectionWindow
{
    using System;
    using SolidUtilities.Editor.Helpers;
    using SolidUtilities.Extensions;
    using TypeReferences.Editor.Drawers;
    using TypeReferences.Editor.TypeDropdown;
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
            var dropdownPosition = GetCenteredPosition(selectionTree);
            _dropdownWindow = DropdownWindow.Create(selectionTree, typeOptionsAttribute.DropdownHeight, dropdownPosition, DropdownWindowType.Popup);
        }

        private SelectionTree GetSelectionTree(NonGenericAttribute attribute, Action<Type[]> onTypeSelected)
        {
            var parentDrawer = new TypeDropdownDrawer(null, attribute, null);
            var dropdownItems = parentDrawer.GetDropdownItems();

            var selectionTree = new SelectionTree(dropdownItems, null, type =>
                {
                    _dropdownWindow.Close();
                    onTypeSelected(new[] { type });
                },
                TypeReferences.Editor.ProjectSettings.SearchbarMinItemsCount, attribute.ExcludeNone);

            if (attribute.ExpandAllFolders)
                selectionTree.ExpandAllFolders();

            return selectionTree;
        }

        private Vector2 GetCenteredPosition(SelectionTree selectionTree)
        {
            Vector2 dropdownPosition = EditorGUIUtilityHelper.GetMainWindowPosition().center;
            dropdownPosition.x -= DropdownWindow.CalculateOptimalWidth(selectionTree.SelectionPaths) / 2f;
            return dropdownPosition.RoundUp();
        }
    }
}