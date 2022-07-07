namespace GenericUnityObjects.UnityEditorInternals
{
    using System;
    using JetBrains.Annotations;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public static class EditorGUIHelper
    {
        /// <summary>
        /// Version of <see cref="EditorGUI.ObjectField"/> that accepts only a generic type and draws it correctly.
        /// </summary>
        [PublicAPI]
        public static Object GenericObjectField(Rect position, GUIContent label, Object currentTarget, Type objType, bool allowSceneObjects)
        {
            return GenericObjectField(position, label, currentTarget, allowSceneObjects, new ObjectFieldHelper(currentTarget, objType));
        }

        /// <summary>
        /// Version of <see cref="EditorGUI.ObjectField"/> that accepts only a generic type and draws it correctly.
        /// </summary>
        [PublicAPI]
        public static void GenericObjectField(Rect position, SerializedProperty property, GUIContent label = null)
        {
            GUIContent propertyLabel = EditorGUI.BeginProperty(position, label, property);

            Object objectBeingEdited = property.serializedObject.targetObject;

            // Allow scene objects if the object being edited is NOT persistent
            bool allowSceneObjects = ! (objectBeingEdited == null || EditorUtility.IsPersistent(objectBeingEdited));

            property.objectReferenceValue = GenericObjectField(position, label == null ? null : propertyLabel, property.objectReferenceValue,
                allowSceneObjects, new ObjectFieldHelper(property));

            EditorGUI.EndProperty();
        }

        [PublicAPI]
        public static Object GenericObjectField(Rect position, [CanBeNull] GUIContent label, Object currentTarget,
            bool allowSceneObjects, in ObjectFieldHelper helper)
        {
            const EditorGUI.ObjectFieldVisualType visualType = EditorGUI.ObjectFieldVisualType.IconAndText;

            int id = GUIUtility.GetControlID(EditorGUI.s_PPtrHash, FocusType.Keyboard, position);

            if (label != null)
                position = EditorGUI.PrefixLabel(position, id, label);

            Event evt = Event.current;
            EventType eventType;

            // special case test, so we continue to ping/select objects with the object field disabled
            if ( ! GUI.enabled && GUIClip.enabled && Event.current.rawType == EventType.MouseDown)
            {
                eventType = Event.current.rawType;
            }
            else
            {
                eventType = evt.type;
            }

            // Has to be this small to fit inside a single line height ObjectField
            using var _ = new EditorGUIUtility.IconSizeScope(new Vector2(12f, 12f));

            string niceTypeName = GenericTypeHelper.GetNiceTypeName(helper);

            Object newTarget = currentTarget;

            switch (eventType)
            {
                case EventType.DragExited:
                    if (GUI.enabled)
                        HandleUtility.Repaint();

                    break;
                case EventType.DragUpdated:
                case EventType.DragPerform:

                    if (eventType == EventType.DragPerform
                        && ! EditorGUI.ValidDroppedObject(DragAndDrop.objectReferences, null, out string errorString))
                    {
                        EditorUtility.DisplayDialog("Can't assign script", errorString, "OK");
                        break;
                    }

                    if ( ! position.Contains(Event.current.mousePosition) && GUI.enabled)
                        break;

                    Object[] references = DragAndDrop.objectReferences;
                    Object validatedObject = helper.ValidateObjectFieldAssignment(references, EditorGUI.ObjectFieldValidatorOptions.None);

                    if (validatedObject != null)
                    {
                        // If scene objects are not allowed and object is a scene object then clear
                        if (!allowSceneObjects && !EditorUtility.IsPersistent(validatedObject))
                            validatedObject = null;
                    }

                    if (validatedObject == null)
                        break;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                    if (eventType == EventType.DragPerform)
                    {
                        newTarget = validatedObject;

                        GUI.changed = true;
                        DragAndDrop.AcceptDrag();
                        DragAndDrop.activeControlID = 0;
                    }
                    else
                    {
                        DragAndDrop.activeControlID = id;
                    }

                    Event.current.Use();

                    break;
                case EventType.MouseDown:
                    if ( ! position.Contains(Event.current.mousePosition))
                        break;

                    if (Event.current.button == 1)
                    {
                        var contextMenu = new GenericMenu();
                        contextMenu.AddItem(GUIContent.Temp("Properties..."), false,
                            () => PropertyEditor.OpenPropertyEditor(currentTarget));
                        contextMenu.DropDown(position);
                    }

                    if (Event.current.button != 0)
                        break;

                    // Get button rect for Object Selector
                    Rect buttonRect = EditorGUI.GetButtonRect(visualType, position);

                    EditorGUIUtility.editingTextField = false;

                    if (buttonRect.Contains(Event.current.mousePosition) && GUI.enabled)
                    {
                        GUIUtility.keyboardControl = id;
                        helper.ShowObjectSelector(allowSceneObjects, niceTypeName, currentTarget);
                        ObjectSelector.get.objectSelectorID = id;

                        evt.Use();
                        GUIUtility.ExitGUI();

                        break;
                    }

                    Object actualTargetObject = currentTarget;

                    if (EditorGUI.showMixedValue)
                    {
                        actualTargetObject = null;
                    }
                    else if (actualTargetObject is Component component)
                    {
                        actualTargetObject = component.gameObject;
                    }

                    // One click shows where the referenced object is, or pops up a preview
                    if (Event.current.clickCount == 1)
                    {
                        GUIUtility.keyboardControl = id;

                        EditorGUI.PingObjectOrShowPreviewOnClick(actualTargetObject, position);
                        evt.Use();
                    }
                    // Double click opens the asset in external app or changes selection to referenced object
                    else if (Event.current.clickCount == 2)
                    {
                        if (actualTargetObject != null)
                        {
                            AssetDatabase.OpenAsset(actualTargetObject);
                            GUIUtility.ExitGUI();
                        }

                        evt.Use();
                    }

                    break;
                case EventType.ExecuteCommand:
                    if (evt.commandName == ObjectSelector.ObjectSelectorUpdatedCommand &&
                        ObjectSelector.get.objectSelectorID == id && GUIUtility.keyboardControl == id)
                    {
                        newTarget = helper.ValidateObjectFieldAssignment(
                            new[] { ObjectSelector.GetCurrentObject() },
                            EditorGUI.ObjectFieldValidatorOptions.None);
                        GUI.changed = true;
                        evt.Use();
                    }

                    break;
                case EventType.KeyDown:
                    if (GUIUtility.keyboardControl != id)
                        break;

                    if (evt.keyCode == KeyCode.Backspace ||
                        (evt.keyCode == KeyCode.Delete && (evt.modifiers & EventModifiers.Shift) == 0))
                    {
                        newTarget = null;
                        GUI.changed = true;
                        evt.Use();
                    }

                    // Apparently we have to check for the character being space instead of the keyCode,
                    // otherwise the Inspector will maximize upon pressing space.
                    if (evt.MainActionKeyForControl(id))
                    {
                        helper.ShowObjectSelector(allowSceneObjects, niceTypeName, currentTarget);
                        ObjectSelector.get.objectSelectorID = id;
                        evt.Use();
                        GUIUtility.ExitGUI();
                    }

                    break;
                case EventType.Repaint:
                    GUIContent objectFieldContent = helper.GetObjectFieldContent(currentTarget, niceTypeName);
                    EditorGUI.BeginHandleMixedValueContentColor();
                    EditorStyles.objectField.Draw(position, objectFieldContent, id, DragAndDrop.activeControlID == id, position.Contains(Event.current.mousePosition));

                    Rect buttonRect2 = EditorStyles.objectFieldButton.margin.Remove(EditorGUI.GetButtonRect(visualType, position));
                    EditorStyles.objectFieldButton.Draw(buttonRect2, GUIContent.none, id, DragAndDrop.activeControlID == id, buttonRect2.Contains(Event.current.mousePosition));
                    EditorGUI.EndHandleMixedValueContentColor();
                    break;
            }

            return newTarget;
        }
    }
}