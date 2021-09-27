namespace GenericUnityObjects.UnityEditorInternals
{
    using System;
    using UnityEditor;
    using Object = UnityEngine.Object;

    public static class ObjectSelectorHelper
    {
        public static void ShowGenericSelector(Object obj, Object objectBeingEdited, Type requiredType, bool allowSceneObjects, string niceTypeName, Action<Object> onObjectSelectedUpdated = null)
        {
            ObjectSelector.get.ShowGeneric(obj, objectBeingEdited, requiredType, allowSceneObjects, niceTypeName, onObjectSelectedUpdated);
        }
    }
}