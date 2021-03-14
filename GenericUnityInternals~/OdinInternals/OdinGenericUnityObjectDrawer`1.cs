namespace OdinInternals
{
    using System.Reflection;
    using GenericUnityObjects;
    using GenericUnityObjects.Editor;
    using JetBrains.Annotations;
    using Sirenix.OdinInspector.Editor;
    using UnityEngine;

    // Some plugins have a property drawer for UnityEngine.Object (e.g. Peek). Unity processes it correctly and applies
    // it to everything that is not MonoBehaviour or GenericScriptableObject. However, it is not the case with Odin.
    // Odin fetches drawers for a type using TypeCache.GetTypesDerivedFrom and uses whatever comes first.
    // If the Peek property drawer comes first, it will be used for generic UnityEngine Objects even though it is
    // less specialized the GenericUnityObjectDrawer. Because of this, we need a class derived from UnityPropertyDrawer
    // to force Odin to draw generic UnityEngine Objects with our drawer.

    [UsedImplicitly]
    internal class MonoBehaviourDrawer<T> : UnityPropertyDrawer<GenericUnityObjectDrawer, T>
        where T : MonoBehaviour
    {
        protected override void Initialize()
        {
            base.Initialize();
            delayApplyValueUntilRepaint = true;
            drawer = new GenericUnityObjectDrawer();

            if (UnityPropertyHandlerUtility.IsAvailable)
                propertyHandler = UnityPropertyHandlerUtility.CreatePropertyHandler(drawer);
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (Property.Tree.GetUnityPropertyForPath(Property.Path, out FieldInfo _) == null)
            {
                CallNextDrawer(label);
                return;
            }

            base.DrawPropertyLayout(label);
        }
    }

    [UsedImplicitly]
    internal class GenericSODrawer<T> : UnityPropertyDrawer<GenericUnityObjectDrawer, T>
        where T : GenericScriptableObject
    {
        protected override void Initialize()
        {
            base.Initialize();
            delayApplyValueUntilRepaint = true;
            drawer = new GenericUnityObjectDrawer();

            if (UnityPropertyHandlerUtility.IsAvailable)
                propertyHandler = UnityPropertyHandlerUtility.CreatePropertyHandler(drawer);
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (Property.Tree.GetUnityPropertyForPath(Property.Path, out FieldInfo _) == null)
            {
                CallNextDrawer(label);
                return;
            }

            base.DrawPropertyLayout(label);
        }
    }
}