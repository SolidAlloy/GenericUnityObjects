namespace GenericUnityObjects.UnityEditorInternals
{
    using System;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using Object = UnityEngine.Object;

    /// <summary>
    /// When drawing ObjectField, there are things that need to be done differently for <see cref="Type"/> and <see cref="SerializedProperty"/>.
    /// This helper encapsulates such parts of the code so that the main method contains the same logic for Type and SerializedProperty.
    /// </summary>
    internal readonly struct ObjectFieldHelper
    {
        public readonly Type ObjType;
        
        private static readonly GUIContent _cachedContent = new GUIContent();

        private readonly SerializedProperty _property;

        public ObjectFieldHelper(Type type)
        {
            ObjType = type;
            _property = null;
        }

        public ObjectFieldHelper(SerializedProperty property)
        {
            _property = property;
            ObjType = GenericTypeHelper.GetGenericType(property);
        }

        public Object ValidateObjectFieldAssignment(Object[] references, EditorGUI.ObjectFieldValidatorOptions options)
        {
            if (references.Length == 0)
                return null;

            return _property != null
                ? ValidateAssignmentForProperty(references, options)
                : ValidateAssignmentForType(references);
        }

        public GUIContent GetObjectFieldContent(Object obj, string niceTypeName)
        {
            if (EditorGUI.showMixedValue)
                return EditorGUI.s_MixedValueContent;

            if (obj == null)
                return EditorGUIUtility.TempContent($"None ({niceTypeName})");

            _cachedContent.text = $"{obj.name} ({niceTypeName})";
            _cachedContent.image = EditorGUIUtility.GetSkinnedIcon(AssetPreview.GetMiniThumbnail(obj));

            if (_property == null)
                return _cachedContent;

            if (ValidateAssignmentForProperty(new[] { obj }, EditorGUI.ObjectFieldValidatorOptions.ExactObjectTypeValidation) == null)
                return EditorGUI.s_TypeMismatch;

            if (EditorSceneManager.preventCrossSceneReferences
                && EditorGUI.CheckForCrossSceneReferencing(obj, _property.serializedObject.targetObject))
            {
                if (EditorApplication.isPlaying)
                {
                    _cachedContent.text += $" ({EditorGUI.GetGameObjectFromObject(obj).scene.name})";
                }
                else
                {
                    return EditorGUI.s_SceneMismatch;
                }
            }

            return _cachedContent;
        }

        public void ShowObjectSelector(bool allowSceneObjects, string niceTypeName, Object obj)
        {
            Object objectBeingEdited = null;

            if (_property != null)
            {
                if (_property.hasMultipleDifferentValues)
                    obj = null;

                objectBeingEdited = _property.serializedObject.targetObject;
                ObjectSelector.get.m_EditedProperty = _property;
            }

            ObjectSelector.get.ShowGeneric(obj, objectBeingEdited, ObjType, allowSceneObjects, niceTypeName);

        }

        private Object ValidateAssignmentForType(Object[] references)
        {
            if (references[0] != null && references[0] is GameObject go && typeof(Component).IsAssignableFrom(ObjType))
            {
                // ReSharper disable once CoVariantArrayConversion
                references = go.GetComponents<Component>();
            }

            foreach (Object obj in references)
            {
                if (ObjType.IsInstanceOfType(obj))
                {
                    return obj;
                }
            }

            return null;
        }

        private Object ValidateAssignmentForProperty(Object[] references, EditorGUI.ObjectFieldValidatorOptions options)
        {
            if (references[0] == null || ! EditorGUI.ValidateObjectReferenceValue(_property, references[0], options))
                return null;

            if (EditorSceneManager.preventCrossSceneReferences &&
                EditorGUI.CheckForCrossSceneReferencing(references[0], _property.serializedObject.targetObject))
            {
                return null;
            }

            return references[0];
        }
    }
}