namespace GenericUnityObjects.ScriptableObject_Example
{
    using JetBrains.Annotations;
    using SolidUtilities.Attributes;
    using UnityEngine;

    internal class Readme : ScriptableObject
    {
#pragma warning disable 414
        [SerializeField, ResizableTextArea, UsedImplicitly]
        private string _description = null;
#pragma warning restore 414
    }
}