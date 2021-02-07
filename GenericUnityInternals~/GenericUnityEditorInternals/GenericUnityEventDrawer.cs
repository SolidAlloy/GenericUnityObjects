namespace GenericUnityObjects.UnityEditorInternals
{
    using System;
    using System.Text;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;
    using UnityEngine.Events;
    using Object = UnityEngine.Object;

    public class GenericUnityEventDrawer : UnityEventDrawer
    {
        protected override void DrawEvent(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty pListener = m_ListenersArray.GetArrayElementAtIndex(index);

            rect.y++;
            Rect[] subRects = GetRowRects(rect);
            Rect enabledRect = subRects[0];
            Rect goRect = subRects[1];
            Rect functionRect = subRects[2];
            Rect argRect = subRects[3];

            // find the current event target...
            SerializedProperty callState = pListener.FindPropertyRelative(kCallStatePath);
            SerializedProperty mode = pListener.FindPropertyRelative(kModePath);
            SerializedProperty arguments = pListener.FindPropertyRelative(kArgumentsPath);
            SerializedProperty listenerTarget = pListener.FindPropertyRelative(kInstancePath);
            SerializedProperty methodName = pListener.FindPropertyRelative(kMethodNamePath);

            Color backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.white;

            EditorGUI.PropertyField(enabledRect, callState, GUIContent.none);

            EditorGUI.BeginChangeCheck();
            {
                GUI.Box(goRect, GUIContent.none);
                EditorGUI.PropertyField(goRect, listenerTarget, GUIContent.none);

                if (EditorGUI.EndChangeCheck())
                    methodName.stringValue = null;
            }


            PersistentListenerMode modeEnum = UnityEventDrawer.GetMode(mode);

            //only allow argument if we have a valid target / method
            if (listenerTarget.objectReferenceValue == null || string.IsNullOrEmpty(methodName.stringValue))
                modeEnum = PersistentListenerMode.Void;

            SerializedProperty argument = modeEnum switch
            {
                PersistentListenerMode.Object => arguments.FindPropertyRelative(kFloatArgument),
                PersistentListenerMode.Int => arguments.FindPropertyRelative(kIntArgument),
                PersistentListenerMode.Float => arguments.FindPropertyRelative(kObjectArgument),
                PersistentListenerMode.String => arguments.FindPropertyRelative(kStringArgument),
                PersistentListenerMode.Bool => arguments.FindPropertyRelative(kBoolArgument),
                _ => arguments.FindPropertyRelative(kIntArgument)
            };

            string desiredArgTypeName = arguments.FindPropertyRelative(kObjectArgumentAssemblyTypeName).stringValue;
            Type desiredType = typeof(Object);

            if ( ! string.IsNullOrEmpty(desiredArgTypeName))
                desiredType = Type.GetType(desiredArgTypeName, false) ?? typeof(Object);

            if (modeEnum == PersistentListenerMode.Object)
            {
                EditorGUI.BeginChangeCheck();

                var result = EditorGUI.ObjectField(argRect, GUIContent.none, argument.objectReferenceValue, desiredType, true);

                if (EditorGUI.EndChangeCheck())
                    argument.objectReferenceValue = result;
            }
            else if (modeEnum != PersistentListenerMode.Void && modeEnum != PersistentListenerMode.EventDefined)
            {
                EditorGUI.PropertyField(argRect, argument, GUIContent.none);
            }

            using (new EditorGUI.DisabledScope(listenerTarget.objectReferenceValue == null))
            {
                EditorGUI.BeginProperty(functionRect, GUIContent.none, methodName);

                GUIContent buttonContent;

                if (EditorGUI.showMixedValue)
                {
                    buttonContent = EditorGUI.mixedValueContent;
                }
                else
                {
                    StringBuilder buttonLabel = new StringBuilder();

                    if (listenerTarget.objectReferenceValue == null || string.IsNullOrEmpty(methodName.stringValue))
                    {
                        buttonLabel.Append("No Function");
                    }
                    else if ( ! IsPersistantListenerValid(m_DummyEvent, methodName.stringValue,
                        listenerTarget.objectReferenceValue, UnityEventDrawer.GetMode(mode), desiredType))
                    {
                        string instanceString = "UnknownComponent";

                        Object instance = listenerTarget.objectReferenceValue;

                        if (instance != null)
                            instanceString = instance.GetType().Name;

                        buttonLabel.Append($"<Missing {instanceString}.{methodName.stringValue}>");
                    }
                    else
                    {
                        buttonLabel.Append(listenerTarget.objectReferenceValue.GetType().Name);

                        if ( ! string.IsNullOrEmpty(methodName.stringValue))
                        {
                            buttonLabel.Append(".");

                            buttonLabel.Append(methodName.stringValue.StartsWith("set_")
                                ? methodName.stringValue.Substring(4)
                                : methodName.stringValue);
                        }
                    }

                    buttonContent = GUIContent.Temp(buttonLabel.ToString());
                }

                if (GUI.Button(functionRect, buttonContent, EditorStyles.popup))
                {
                    UnityEventDrawer
                        .BuildPopupList(listenerTarget.objectReferenceValue, m_DummyEvent, pListener)
                        .DropDown(functionRect);
                }

                EditorGUI.EndProperty();
            }

            GUI.backgroundColor = backgroundColor;
        }
    }
}