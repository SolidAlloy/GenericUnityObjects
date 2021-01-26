namespace GenericUnityObjects.UnityEditorInternals
{
    using System;
    using JetBrains.Annotations;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public static class EditorGUILayoutHelper
    {
        public static Object GenericObjectField(string label, Object oldTarget, Type objType, bool allowSceneObjects)
        {
            EditorGUILayout.s_LastRect = EditorGUILayout.GetControlRect(
                true,
                EditorGUI.kSingleLineHeight,
                (GUILayoutOption[]) null);

            return EditorGUIHelper.GenericObjectField(
                EditorGUILayout.s_LastRect,
                EditorGUIUtility.TempContent(label),
                oldTarget,
                objType,
                allowSceneObjects);
        }
    }

    public static class EditorGUIHelper
    {
        public static Object GenericObjectField(Rect position, GUIContent label, Object oldTarget, Type objType, bool allowSceneObjects)
        {
            return null;
        }

        [PublicAPI]
        public static void GenericObjectField(Rect position, SerializedProperty property)
        {
            GUIContent label = EditorGUI.BeginProperty(position, null, property);

            ObjectFieldInternal(position, property, label);

            EditorGUI.EndProperty();
        }

        private static Object ObjectFieldInternal(Rect position, GUIContent label, Type objType, bool allowSceneObjects)
        {
            const EditorGUI.ObjectFieldVisualType visualType = EditorGUI.ObjectFieldVisualType.IconAndText;

            int id = GUIUtility.GetControlID(EditorGUI.s_PPtrHash, FocusType.Keyboard, position);
            position = EditorGUI.PrefixLabel(position, id, label);

            return null;
        }

        private static void ObjectFieldInternal(Rect position, SerializedProperty property, GUIContent label)
        {
            const EditorGUI.ObjectFieldVisualType visualType = EditorGUI.ObjectFieldVisualType.IconAndText;

            int id = GUIUtility.GetControlID(EditorGUI.s_PPtrHash, FocusType.Keyboard, position);
            position = EditorGUI.PrefixLabel(position, id, label);

            Object objectBeingEdited = property.serializedObject.targetObject;

            // Allow scene objects if the object being edited is NOT persistent
            bool allowSceneObjects = ! (objectBeingEdited == null || EditorUtility.IsPersistent(objectBeingEdited));



            Object obj = property.objectReferenceValue;
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

            string niceTypeName = GenericTypeHelper.GetNiceTypeName(property);

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
                    Object validatedObject = ValidateObjectFieldAssignment(references, property,
                        EditorGUI.ObjectFieldValidatorOptions.None);

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
                        property.objectReferenceValue = validatedObject;

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
                        Object actualObject = property.objectReferenceValue;
                        var contextMenu = new GenericMenu();
                        contextMenu.AddItem(GUIContent.Temp("Properties..."), false,
                            () => PropertyEditor.OpenPropertyEditor(actualObject));
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
                        ObjectSelector.get.ShowGeneric(property, allowSceneObjects, niceTypeName);
                        ObjectSelector.get.objectSelectorID = id;

                        evt.Use();
                        GUIUtility.ExitGUI();

                        break;
                    }

                    Object actualTargetObject = property.objectReferenceValue;

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
                        if (actualTargetObject)
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
                        AssignSelectedObject(property, evt);
                    }

                    break;
                case EventType.KeyDown:
                    if (GUIUtility.keyboardControl != id)
                        break;

                    if (evt.keyCode == KeyCode.Backspace ||
                        (evt.keyCode == KeyCode.Delete && (evt.modifiers & EventModifiers.Shift) == 0))
                    {
                        property.objectReferenceValue = null;
                        GUI.changed = true;
                        evt.Use();
                    }

                    // Apparently we have to check for the character being space instead of the keyCode,
                    // otherwise the Inspector will maximize upon pressing space.
                    if (evt.MainActionKeyForControl(id))
                    {
                        ObjectSelector.get.ShowGeneric(property, allowSceneObjects, niceTypeName);
                        ObjectSelector.get.objectSelectorID = id;
                        evt.Use();
                        GUIUtility.ExitGUI();
                    }

                    break;
                case EventType.Repaint:
                    GUIContent objectFieldContent = GetObjectFieldContent(obj, property, niceTypeName);
                    EditorGUI.BeginHandleMixedValueContentColor();
                    EditorStyles.objectField.Draw(position, objectFieldContent, id, DragAndDrop.activeControlID == id, position.Contains(Event.current.mousePosition));

                    Rect buttonRect2 = EditorStyles.objectFieldButton.margin.Remove(EditorGUI.GetButtonRect(visualType, position));
                    EditorStyles.objectFieldButton.Draw(buttonRect2, GUIContent.none, id, DragAndDrop.activeControlID == id, buttonRect2.Contains(Event.current.mousePosition));
                    EditorGUI.EndHandleMixedValueContentColor();
                    break;
            }
        }

        private static GUIContent GetObjectFieldContent(Object obj, SerializedProperty property, string niceTypeName)
        {
            if (EditorGUI.showMixedValue)
            {
                return EditorGUI.s_MixedValueContent;
            }

            // If obj is null, we have to rely on
            // property.objectReferenceStringValue to display None/Missing and the
            // correct type. But if not, EditorGUIUtility.ObjectContent is more reliable.
            // It can take a more specific object type specified as argument into account,
            // and it gets the icon at the same time.
            if (obj == null)
            {
                return EditorGUIUtility.TempContent($"None ({niceTypeName})");
            }

            if (ValidateObjectFieldAssignment(new[] { obj }, property, EditorGUI.ObjectFieldValidatorOptions.ExactObjectTypeValidation) == null)
                return EditorGUI.s_TypeMismatch;

            EditorGUIUtility.s_ObjectContent.text = $"{obj.name} ({niceTypeName})";
            EditorGUIUtility.s_ObjectContent.image = EditorGUIUtility.GetSkinnedIcon(AssetPreview.GetMiniThumbnail(obj));
            GUIContent temp = EditorGUIUtility.s_ObjectContent;

            if (EditorSceneManager.preventCrossSceneReferences && EditorGUI.CheckForCrossSceneReferencing(obj, property.serializedObject.targetObject))
            {
                if (EditorApplication.isPlaying)
                {
                    temp.text += $" ({EditorGUI.GetGameObjectFromObject(obj).scene.name})";
                }
                else
                {
                    return EditorGUI.s_SceneMismatch;
                }
            }

            return temp;
        }

        private static Object ValidateObjectFieldAssignment(Object[] references, SerializedProperty property, EditorGUI.ObjectFieldValidatorOptions options)
        {
            if (references.Length == 0)
                return null;

            if (references[0] == null || ! EditorGUI.ValidateObjectReferenceValue(property, references[0], options))
                return null;

            if (EditorSceneManager.preventCrossSceneReferences &&
                EditorGUI.CheckForCrossSceneReferencing(references[0], property.serializedObject.targetObject))
            {
                return null;
            }

            return references[0];
        }

        private static void AssignSelectedObject(SerializedProperty property, Event evt)
        {
            Object[] references = { ObjectSelector.GetCurrentObject() };
            Object assigned = ValidateObjectFieldAssignment(references, property, EditorGUI.ObjectFieldValidatorOptions.None);

            // Assign the value
            property.objectReferenceValue = assigned;

            GUI.changed = true;
            evt.Use();
        }
    }
}