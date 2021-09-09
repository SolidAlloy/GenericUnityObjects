namespace GenericUnityObjects.Editor
{
    using UnityEditor;
    using UnityEngine;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector.Editor;
#endif

    internal class GenericUnityObjectDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GenericObjectDrawer.ObjectField(position, property, label);
        }
    }
    
    [CustomPropertyDrawer(typeof(GenericScriptableObject), true)]
#if ODIN_INSPECTOR
    [DrawerPriority(0, 0, 2)]
#endif
    internal class GenericSODrawer : GenericUnityObjectDrawer
    {
        private static CreatableObjectDrawer _creatableObjectDrawer;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (ProjectSettings.AlwaysCreatable)
            {
                _creatableObjectDrawer ??= new CreatableObjectDrawer();
                _creatableObjectDrawer.OnGUI(position, property, label);
            }
            else
            {
                // ReSharper disable once Unity.PropertyDrawerOnGUIBase
                base.OnGUI(position, property, label);
            }
        }
    }

    [CustomPropertyDrawer(typeof(MonoBehaviour), true)]
#if ODIN_INSPECTOR
    [DrawerPriority(0, 0, 2)]
#endif
    internal class GenericBehaviourDrawer : GenericUnityObjectDrawer { }
}