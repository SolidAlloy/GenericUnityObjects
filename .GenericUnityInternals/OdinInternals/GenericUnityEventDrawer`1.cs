namespace OdinInternals
{
    // Note that both split and non-split DLLs are held as dependencies.
    // Odin dependencies are messed up, and it needs both DLL versions to build this project successfully.
    using System.Reflection;
    using GenericUnityObjects.UnityEditorInternals;
    using JetBrains.Annotations;
    using Sirenix.OdinInspector.Editor;
    using UnityEngine;
    using UnityEngine.Events;

    // A copy of UnityEventDrawer<T> from Odin that creates a GenericUnityEventDrawer instance instead of UnityEventDrawer.
    // Unlike UnityEventDrawer<T> it passes 1 as the super parameter which makes it override any other UnityEvent drawer.
    [DrawerPriority(1.0, 0.0, 0.45)]
    [UsedImplicitly]
    public sealed class GenericUnityEventDrawer<T> : UnityPropertyDrawer<GenericUnityEventDrawer, T>
        where T : UnityEventBase
    {
        protected override void Initialize()
        {
            base.Initialize();
            delayApplyValueUntilRepaint = true;
            drawer = new GenericUnityEventDrawer();

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
