namespace GenericUnityObjects.UnityEditorInternals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
                        buttonLabel.Append(GetTypeName(listenerTarget));

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

        private string GetTypeName(SerializedProperty target)
        {
            return target.objectReferenceValue.GetType().Name;
        }

        private static GenericMenu BuildPopupList(Object target, UnityEventBase dummyEvent, SerializedProperty listener)
        {
            // special case for components... we want all the game objects targets there!
            Object targetToUse = target is Component comp ? comp.gameObject : target;

            // find the current event target...
            SerializedProperty methodName = listener.FindPropertyRelative(kMethodNamePath);

            GenericMenu menu = new GenericMenu();

            menu.AddItem(
                new GUIContent(kNoFunctionString),
                string.IsNullOrEmpty(methodName.stringValue),
                UnityEventDrawer.ClearEventFunction,
                new UnityEventDrawer.UnityEventFunction(listener, null, null, PersistentListenerMode.EventDefined));

            if (targetToUse == null)
                return menu;

            menu.AddSeparator(string.Empty);

            /*

            GeneratePopUpForType(menu, targetToUse, false, listener, delegateArgumentsTypes);
            if (targetToUse is GameObject)
            {
                Component[] comps = (targetToUse as GameObject).GetComponents<Component>();
                var duplicateNames = comps.Where(c => c != null).Select(c => c.GetType().Name).GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                foreach (Component comp in comps)
                {
                    if (comp == null)
                        continue;

                    GeneratePopUpForType(menu, comp, duplicateNames.Contains(comp.GetType().Name), listener, delegateArgumentsTypes);
                }
            }

            return menu;*/

            // figure out the signature of this delegate...
            // The property at this stage points to the 'container' and has the field name
            var delegateArgumentsTypes = dummyEvent
                .GetType()
                .GetMethod("Invoke")
                ?.GetParameters()
                .Select(parameter => parameter.ParameterType)
                .ToArray();

            UnityEventDrawer.GeneratePopUpForType(menu, targetToUse, false, listener, delegateArgumentsTypes);

            if ( ! (targetToUse is GameObject gameObject))
                return menu;

            ComponentInfo.ClearNames();

            foreach (Component component in gameObject.GetComponents<Component>())
            {
                if (component != null)
                {
                    var componentName = new ComponentInfo(component);

                    GeneratePopUpForType(menu, component,
                        componentName.HasDuplicate, listener, delegateArgumentsTypes);
                }
            }

            return menu;
        }

        private static void GeneratePopUpForType(GenericMenu menu, Object target, bool useFullTargetName,
            SerializedProperty listener, Type[] delegateArgumentsTypes)
        {
            var methods = new List<UnityEventDrawer.ValidMethodMap>();
            string targetName = useFullTargetName ? target.GetType().FullName : target.GetType().Name;
            bool didAddDynamic = false;

            // skip 'void' event defined on the GUI as we have a void prebuilt type!
            if ( delegateArgumentsTypes.Length != 0)
            {
                UnityEventDrawer.GetMethodsForTargetAndMode(target, delegateArgumentsTypes, methods,
                    PersistentListenerMode.EventDefined);

                if (methods.Count > 0)
                {
                    menu.AddDisabledItem(new GUIContent(targetName + "/Dynamic " + string.Join(", ",
                        delegateArgumentsTypes
                        .Select(argumentType => UnityEventDrawer.GetTypeName(argumentType))
                        .ToArray())));

                    UnityEventDrawer.AddMethodsToMenu(menu, listener, methods, targetName);
                    didAddDynamic = true;
                }
            }

            methods.Clear();

            UnityEventDrawer.GetMethodsForTargetAndMode(target, new[] { typeof(float) }, methods, PersistentListenerMode.Float);
            UnityEventDrawer.GetMethodsForTargetAndMode(target, new[] { typeof(int) }, methods, PersistentListenerMode.Int);
            UnityEventDrawer.GetMethodsForTargetAndMode(target, new[] { typeof(string) }, methods, PersistentListenerMode.String);
            UnityEventDrawer.GetMethodsForTargetAndMode(target, new[] { typeof(bool) }, methods, PersistentListenerMode.Bool);
            UnityEventDrawer.GetMethodsForTargetAndMode(target, new[] { typeof(Object) }, methods, PersistentListenerMode.Object);
            UnityEventDrawer.GetMethodsForTargetAndMode(target, Array.Empty<Type>(), methods, PersistentListenerMode.Void);

            if (methods.Count == 0)
                return;

            if (didAddDynamic)
            {
                // AddSeperator doesn't seem to work for sub-menus, so we have to use this workaround instead of a proper separator for now.
                menu.AddItem(new GUIContent(targetName + "/ "), false, null);
            }

            if (delegateArgumentsTypes.Length != 0)
            {
                menu.AddDisabledItem(new GUIContent(targetName + "/Static Parameters"));
            }

            UnityEventDrawer.AddMethodsToMenu(menu, listener, methods, targetName);
        }

        private readonly struct ComponentInfo
        {
            private static readonly HashSet<string> _names = new HashSet<string>();

            public readonly bool HasDuplicate;
            public readonly string Name;

            public ComponentInfo(Component component)
            {
                Name = component.GetType().Name; // TODO: replace with method
                HasDuplicate = _names.Contains(Name);

                if ( ! HasDuplicate)
                    _names.Add(Name);
            }

            public static void ClearNames() => _names.Clear();
        }
    }
}