namespace GenericUnityObjects.Editor
{
    using UnityEditor;
    using UnityEngine;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector.Editor;
#endif

    internal abstract class GenericUnityObjectDrawer : PropertyDrawer
    {
        protected abstract bool AlwaysCreatable { get; }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (AlwaysCreatable)
            {
                CreatableObjectDrawer.Instance.OnGUI(position, property, label);
            }
            else
            {
                // ReSharper disable once Unity.PropertyDrawerOnGUIBase
                GenericObjectDrawer.ObjectField(position, property, label);
            }
        }
    }

    [CustomPropertyDrawer(typeof(ScriptableObject), true)]
#if ODIN_INSPECTOR
    [DrawerPriority(0, 0, 2)]
#endif
    internal class GenericSODrawer : GenericUnityObjectDrawer
    {
        protected override bool AlwaysCreatable => ProjectSettings.AlwaysCreatableScriptableObject;
    }

    [CustomPropertyDrawer(typeof(MonoBehaviour), true)]
#if ODIN_INSPECTOR
    [DrawerPriority(0, 0, 2)]
#endif
    internal class GenericBehaviourDrawer : GenericUnityObjectDrawer
    {
        protected override bool AlwaysCreatable => ProjectSettings.AlwaysCreatableMonoBehaviour;
    }
}