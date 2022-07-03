namespace GenericUnityObjects.UnityEditorInternals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using SolidUtilities.Editor;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEditorInternal;
    using UnityEngine;
    using UnityEngine.Events;
    using Util;
    using Object = UnityEngine.Object;

    [CustomPropertyDrawer(typeof(UnityEventBase), true)]
    public class GenericUnityEventDrawer : UnityEventDrawer
    {
        [DidReloadScripts]
        private static void ReplaceDefaultDrawer()
        {
            DrawerReplacer.ReplaceDefaultDrawer<UnityEventBase, GenericUnityEventDrawer>();
        }

        // The default implementation of DrawEvent. Only specific lines are changed and marked with the "Previously:" comments.
        // For meaningful names, look at https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/UnityEventDrawer.cs
        protected override void DrawEvent(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty arrayElementAtIndex = this.m_ListenersArray.GetArrayElementAtIndex(index);
            ++rect.y;
            Rect[] rowRects = this.GetRowRects(rect);
            Rect position1 = rowRects[0];
            Rect position2 = rowRects[1];
            Rect rect1 = rowRects[2];
            Rect position3 = rowRects[3];
            SerializedProperty propertyRelative1 = arrayElementAtIndex.FindPropertyRelative("m_CallState");
            SerializedProperty propertyRelative2 = arrayElementAtIndex.FindPropertyRelative("m_Mode");
            SerializedProperty propertyRelative3 = arrayElementAtIndex.FindPropertyRelative("m_Arguments");
            SerializedProperty propertyRelative4 = arrayElementAtIndex.FindPropertyRelative("m_Target");
            SerializedProperty propertyRelative5 = arrayElementAtIndex.FindPropertyRelative("m_MethodName");
            Color backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.white;
            EditorGUI.PropertyField(position1, propertyRelative1, GUIContent.none);
            EditorGUI.BeginChangeCheck();
            GUI.Box(position2, GUIContent.none);
            EditorGUI.PropertyField(position2, propertyRelative4, GUIContent.none);
            if (EditorGUI.EndChangeCheck()) propertyRelative5.stringValue = null;
            PersistentListenerMode persistentListenerMode = UnityEventDrawer.GetMode(propertyRelative2);
            if (propertyRelative4.objectReferenceValue == null ||
                string.IsNullOrEmpty(propertyRelative5.stringValue))
                persistentListenerMode = PersistentListenerMode.Void;
            SerializedProperty propertyRelative6;
            switch (persistentListenerMode)
            {
                case PersistentListenerMode.Object:
                    propertyRelative6 = propertyRelative3.FindPropertyRelative("m_ObjectArgument");
                    break;
                case PersistentListenerMode.Int:
                    propertyRelative6 = propertyRelative3.FindPropertyRelative("m_IntArgument");
                    break;
                case PersistentListenerMode.Float:
                    propertyRelative6 = propertyRelative3.FindPropertyRelative("m_FloatArgument");
                    break;
                case PersistentListenerMode.String:
                    propertyRelative6 = propertyRelative3.FindPropertyRelative("m_StringArgument");
                    break;
                case PersistentListenerMode.Bool:
                    propertyRelative6 = propertyRelative3.FindPropertyRelative("m_BoolArgument");
                    break;
                default:
                    propertyRelative6 = propertyRelative3.FindPropertyRelative("m_IntArgument");
                    break;
            }

            string stringValue = propertyRelative3.FindPropertyRelative("m_ObjectArgumentAssemblyTypeName").stringValue;
            Type type1 = typeof(Object);
            if (!string.IsNullOrEmpty(stringValue))
            {
                Type type2 = Type.GetType(stringValue, false);
                if ((object) type2 == null) type2 = typeof(Object);
                type1 = type2;
            }

            int num;
            switch (persistentListenerMode)
            {
                case PersistentListenerMode.Void:
                    num = 0;
                    break;
                case PersistentListenerMode.Object:
                    EditorGUI.BeginChangeCheck();

                    // Previously: Object @object = EditorGUI.ObjectField(position3, GUIContent.none, propertyRelative6.objectReferenceValue, type1, true);
                    Object @object = ObjectField(position3, GUIContent.none,
                        propertyRelative6.objectReferenceValue, type1, true);

                    if (EditorGUI.EndChangeCheck())
                    {
                        propertyRelative6.objectReferenceValue = @object;
                        goto label_22;
                    }
                    else goto label_22;
                default:
                    num = (uint) persistentListenerMode > 0U ? 1 : 0;
                    break;
            }

            if (num != 0) EditorGUI.PropertyField(position3, propertyRelative6, GUIContent.none);
            label_22:
            using (new EditorGUI.DisabledScope(propertyRelative4.objectReferenceValue == null))
            {
                EditorGUI.BeginProperty(rect1, GUIContent.none, propertyRelative5);
                GUIContent content;
                if (EditorGUI.showMixedValue)
                {
                    content = EditorGUI.mixedValueContent;
                }
                else
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    if (propertyRelative4.objectReferenceValue == null ||
                        string.IsNullOrEmpty(propertyRelative5.stringValue)) stringBuilder.Append("No Function");
                    else if (!IsPersistantListenerValid(this.m_DummyEvent,
                        propertyRelative5.stringValue, propertyRelative4.objectReferenceValue,
                        UnityEventDrawer.GetMode(propertyRelative2), type1))
                    {
                        string str = "UnknownComponent";
                        Object objectReferenceValue = propertyRelative4.objectReferenceValue;
                        if (objectReferenceValue != null)
                            // Previously: str = ((object) objectReferenceValue).GetType().Name;
                            str = ComponentInfo.GetTypeName(objectReferenceValue, true);
                        stringBuilder.Append(string.Format("<Missing {0}.{1}>", str,
                            propertyRelative5.stringValue));
                    }
                    else
                    {
                        // Previously: ((object) propertyRelative4.objectReferenceValue).GetType().Name
                        stringBuilder.Append(ComponentInfo.GetTypeName(propertyRelative4.objectReferenceValue, true));
                        if (!string.IsNullOrEmpty(propertyRelative5.stringValue))
                        {
                            stringBuilder.Append(".");
                            if (propertyRelative5.stringValue.StartsWith("set_"))
                                stringBuilder.Append(propertyRelative5.stringValue.Substring(4));
                            else stringBuilder.Append(propertyRelative5.stringValue);
                        }
                    }

                    content = GUIContent.Temp(stringBuilder.ToString());
                }

                if (EditorGUI.DropdownButton(rect1, content, FocusType.Passive, EditorStyles.popup))
                    // Previously: UnityEventDrawer.BuildPopupList
                    BuildPopupList(propertyRelative4.objectReferenceValue, this.m_DummyEvent, arrayElementAtIndex).DropDown(rect1);
                EditorGUI.EndProperty();
            }

            GUI.backgroundColor = backgroundColor;
        }

        private static Object ObjectField(Rect position, GUIContent label, Object currentTarget, Type objType, bool allowSceneObjects)
        {
            return currentTarget?.GetType().Name.StartsWith("ConcreteClass_") is true
                ? EditorGUIHelper.GenericObjectField(position, label, currentTarget, objType, allowSceneObjects)
                : EditorGUI.ObjectField(position, label, currentTarget, objType, allowSceneObjects);
        }

        // Default implementation, only GetType().Name is replaced with ComponentInfo.GetTypeName()
        private static GenericMenu BuildPopupList(Object target, UnityEventBase dummyEvent, SerializedProperty listener)
        {
            // special case for components... we want all the game objects targets there!
            Object targetToUse = target is Component comp ? comp.gameObject : target;

            // find the current event target...
            SerializedProperty methodName = listener.FindPropertyRelative(kMethodNamePath);

            var menu = new GenericMenu();

            menu.AddItem(
                new GUIContent(kNoFunctionString),
                string.IsNullOrEmpty(methodName.stringValue),
                UnityEventDrawer.ClearEventFunction,
                new UnityEventDrawer.UnityEventFunction(listener, null, null, PersistentListenerMode.EventDefined));

            if (targetToUse == null)
                return menu;

            menu.AddSeparator(string.Empty);

            // figure out the signature of this delegate...
            // The property at this stage points to the 'container' and has the field name
            var delegateArgumentsTypes = dummyEvent
                .GetType()
                .GetMethod("Invoke")
                ?.GetParameters()
                .Select(parameter => parameter.ParameterType)
                .ToArray();

            GeneratePopUpForType(menu, targetToUse, ComponentInfo.GetTypeName(target, true), listener, delegateArgumentsTypes);

            if ( ! (targetToUse is GameObject gameObject))
                return menu;

            ComponentInfo.ClearNames();

            foreach (Component component in gameObject.GetComponents<Component>())
            {
                if (component == null)
                    continue;

                // GeneratePopUpForType(new ComponentInfo(component), menu, listener, delegateArgumentsTypes);
                GeneratePopUpForType(menu, component, ComponentInfo.GetTypeName(component, false), listener, delegateArgumentsTypes);
            }

            return menu;
        }

        protected override void SetupReorderableList(ReorderableList list)
        {
            base.SetupReorderableList(list);
            list.draggable = true;
        }

        // Default implementation of GeneratePopUpForType, nothing's changed.
        // It is duplicated because the signature of the method differs between 2020.3.16 and newer versions of Unity.
        // The method is duplicated so that we don't have to rely on predefined symbols to use different methods.
        private static void GeneratePopUpForType(
            GenericMenu menu,
            Object target,
            string targetName,
            SerializedProperty listener,
            Type[] delegateArgumentsTypes)
        {
            List<UnityEventDrawer.ValidMethodMap> methods = new List<UnityEventDrawer.ValidMethodMap>();
            bool flag = false;
            if ((uint) delegateArgumentsTypes.Length > 0U)
            {
                UnityEventDrawer.GetMethodsForTargetAndMode(target, delegateArgumentsTypes, methods,
                    PersistentListenerMode.EventDefined);
                if (methods.Count > 0)
                {
                    menu.AddDisabledItem(new GUIContent(targetName + "/Dynamic " + string.Join(", ",
                        delegateArgumentsTypes
                        .Select(e => UnityEventDrawer.GetTypeName(e))
                        .ToArray())));
                    UnityEventDrawer.AddMethodsToMenu(menu, listener, methods, targetName);
                    flag = true;
                }
            }

            methods.Clear();
            UnityEventDrawer.GetMethodsForTargetAndMode(target, new Type[1]
            {
                typeof(float)
            }, methods, PersistentListenerMode.Float);
            UnityEventDrawer.GetMethodsForTargetAndMode(target, new Type[1]
            {
                typeof(int)
            }, methods, PersistentListenerMode.Int);
            UnityEventDrawer.GetMethodsForTargetAndMode(target, new Type[1]
            {
                typeof(string)
            }, methods, PersistentListenerMode.String);
            UnityEventDrawer.GetMethodsForTargetAndMode(target, new Type[1]
            {
                typeof(bool)
            }, methods, PersistentListenerMode.Bool);
            UnityEventDrawer.GetMethodsForTargetAndMode(target, new Type[1]
            {
                typeof(Object)
            }, methods, PersistentListenerMode.Object);
            UnityEventDrawer.GetMethodsForTargetAndMode(target, new Type[0], methods,
                PersistentListenerMode.Void);
            if (methods.Count <= 0)
                return;
            if (flag)
                menu.AddItem(new GUIContent(targetName + "/ "), false, null);
            if ((uint) delegateArgumentsTypes.Length > 0U)
                menu.AddDisabledItem(new GUIContent(targetName + "/Static Parameters"));
            UnityEventDrawer.AddMethodsToMenu(menu, listener, methods, targetName);
        }

        // A struct responsible for obtaining the correct component name and keeping cache of generated names
        private readonly struct ComponentInfo
        {
            private static readonly Dictionary<Type, string> _typeNameCache = new Dictionary<Type, string>();
            private static readonly HashSet<string> _names = new HashSet<string>();

            public static void ClearNames() => _names.Clear();

            public static string GetTypeName(Object unityObject, bool onlyShort)
            {
                Type unityObjectType = unityObject.GetType();

                if ( ! _typeNameCache.TryGetValue(unityObjectType, out string shortName))
                {
                    shortName = unityObject is MonoBehaviour
                        ? GetBehaviourShortName(unityObjectType)
                        : GetScriptableObjectShortName(unityObjectType);

                    _typeNameCache.Add(unityObjectType, shortName);
                }

                if (onlyShort)
                    return shortName;

                if (_names.Contains(shortName))
                    return $"{unityObjectType.Namespace}.{shortName}";

                _names.Add(shortName);
                return shortName;
            }

            private static string GetBehaviourShortName(Type unityObjectType)
            {
                var componentAttribute = unityObjectType.GetCustomAttribute<AddComponentMenu>();

                return componentAttribute == null
                    ? unityObjectType.Name :
                    componentAttribute.componentMenu.Split('/').Last();
            }

            private static string GetScriptableObjectShortName(Type unityObjectType)
            {
                return unityObjectType.BaseType.IsGenericType ? TypeUtility.GetNiceNameOfGenericType(unityObjectType.BaseType) : unityObjectType.Name;
            }
        }
    }
}