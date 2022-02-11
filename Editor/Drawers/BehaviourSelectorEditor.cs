namespace GenericUnityObjects.Editor
{
    using System.Linq;
    using GenericUnityObjects.Util;
    using MonoBehaviours;
    using SolidUtilities;
    using SolidUtilities.Editor;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(BehaviourSelector), true)]
    internal class BehaviourSelectorEditor : Editor
    {
        private SerializedProperty _typesArray;
        private BehaviourSelector _targetSelector;
        private string _genericTypeNameWithoutSuffix;
        private string[] _argumentNames;
        private string[] _genericArgNames;
        private TypeReferenceWithBaseTypesDrawer _drawer;

        private void OnEnable()
        {
            if ( ! (target is BehaviourSelector targetSelector))
                return;

            _targetSelector = targetSelector;
            _typesArray = serializedObject.FindProperty(nameof(BehaviourSelector.TypeRefs));
            _genericTypeNameWithoutSuffix = _targetSelector.GenericBehaviourType.Name.StripGenericSuffix();
            _argumentNames = new string[_targetSelector.TypeRefs.Length];
            _genericArgNames = TypeHelper.GetNiceArgsOfGenericType(_targetSelector.GenericBehaviourType);
            _drawer = new TypeReferenceWithBaseTypesDrawer();
        }

        public override void OnInspectorGUI()
        {
            for (int i = 0; i < _typesArray.arraySize; i++)
            {
                SerializedProperty prop = _typesArray.GetArrayElementAtIndex(i);
                GUIContent label = GUIContentHelper.Temp(_genericArgNames[i]);
                Rect propertyRect = EditorGUILayout.GetControlRect(true, _drawer.GetPropertyHeight(prop, label));

                if (_targetSelector.JustBeenAdded && _targetSelector.TypeRefs.Length == 1)
                {
                    _targetSelector.JustBeenAdded = false;
                    _drawer.TriggerDropdownImmediately(propertyRect, prop, label);
                }
                else
                {
                    _drawer.OnGUI(propertyRect, prop, label);
                }
            }

            if ( ! GUILayout.Button(GetButtonName()))
                return;

            if (_targetSelector.TypeRefs.Any(typeRef => typeRef.Type == null))
            {
                Debug.LogWarning("Choose all the type parameters first!");
            }
            else
            {
                GenericBehaviourCreator.AddComponent(
                    _targetSelector.GetType(),
                    _targetSelector.gameObject,
                    _targetSelector.GenericBehaviourType,
                    _targetSelector.TypeRefs.CastToType());
            }
        }

        private string GetButtonName()
        {
            for (int i = 0; i < _targetSelector.TypeRefs.Length; i++)
            {
                TypeReferenceWithBaseTypes typeRef = _targetSelector.TypeRefs[i];
                string fullName = typeRef.Type == null ? string.Empty : typeRef.Type.FullName;
                fullName = fullName.ReplaceWithBuiltInName();
                _argumentNames[i] = fullName.GetSubstringAfterLast('.');
            }

            return $"Add {_genericTypeNameWithoutSuffix}<{string.Join(",", _argumentNames)}>";
        }
    }
}