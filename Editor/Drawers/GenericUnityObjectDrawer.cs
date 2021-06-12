namespace GenericUnityObjects.Editor
{
    using UnityEditor;
    using UnityEngine;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector.Editor;
#endif

    /// <summary>
    /// The default object field drawn for types derived from <see cref="GenericScriptableObject"/> does not list
    /// available assets in the object picker window. This custom property drawer looks the same but lists the
    /// available assets.
    /// </summary>
    [CustomPropertyDrawer(typeof(GenericScriptableObject), true)]
#if ODIN_INSPECTOR
    [DrawerPriority(0, 0, 2)]
#endif
    internal class GenericSODrawer : GenericUnityObjectDrawer { }

    [CustomPropertyDrawer(typeof(MonoBehaviour), true)]
#if ODIN_INSPECTOR
    [DrawerPriority(0, 0, 2)]
#endif
    internal class GenericBehaviourDrawer : GenericUnityObjectDrawer { }

    internal class GenericUnityObjectDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GenericObjectDrawer.ObjectField(position, property, label);
        }
    }
}