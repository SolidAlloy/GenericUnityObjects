namespace GenericUnityObjects.Editor.ScriptableObjects
{
    using System;
    using System.Reflection;
    using GenericUnityObjects.Util;
    using SolidUtilities.Editor.Extensions;
    using SolidUtilities.Editor.Helpers;
    using UnityEditorInternals;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Assertions;

    /// <summary>
    /// The default object field drawn for types derived from <see cref="GenericScriptableObject"/> does not list
    /// available assets in the object picker window. This custom property drawer looks the same but lists the
    /// available assets.
    /// </summary>
    [CustomPropertyDrawer(typeof(GenericScriptableObject), true)]
    internal class GenericSODrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using var propertyLabel = new EditorDrawHelper.PropertyWrapper(position, label, property);
            Rect posWithoutLabel = EditorGUI.PrefixLabel(position, propertyLabel);

            if (property.type.Contains("`"))
            {
                Type filterType = GetFilterType(property);
                property.objectReferenceValue = EditorGUI.ObjectField(
                    posWithoutLabel, property.objectReferenceValue, filterType, true);
            }
            else
            {
                EditorGUI.PropertyField(posWithoutLabel, property);
            }
        }

        private static Type GetFilterType(SerializedProperty property)
        {
            Type objectType = property.GetObjectType();

            return ScriptableObjectsDatabase.TryGetConcreteType(objectType, out Type concreteType)
                ? concreteType
                : objectType;
        }
    }

    [CustomPropertyDrawer(typeof(MonoBehaviour), true)]
    internal class GenericBehaviourDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            if (property.type.Contains("`"))
            {
                // TODO: complete the method so that it works
                EditorGUIHelper.GenericObjectField(position, property);
                /*
                Type filterType = GetFilterType(property);
                property.objectReferenceValue = EditorGUI.ObjectField(
                    posWithoutLabel, property.objectReferenceValue, filterType, true);
                    */
            }
            else
            {
                EditorGUI.PropertyField(position, property);
            }
        }

        private static Type GetFilterType(SerializedProperty property)
        {
            Type objectType = property.GetObjectType();

            return BehavioursDatabase.TryGetConcreteType(objectType, out Type concreteType)
                ? concreteType
                : objectType;
        }

        private static void GetObjectType(SerializedProperty property)
        {
            // ScriptAttributeUtility.GetFieldInfoFromProperty(property, out Type type);
            Type scriptableAttributeUtility = Assembly.Load("UnityEditor.CoreModule").GetType("UnityEditor.ScriptAttributeUtility");
            MethodInfo method = scriptableAttributeUtility.GetMethod(
                "GetFieldInfoFromProperty",
                BindingFlags.NonPublic | BindingFlags.Static);

            Assert.IsNotNull(method);


            var parameters = new object[] { property, null };
            method.Invoke(null, parameters);

            Debug.Log((Type) parameters[1]);
        }
    }
}