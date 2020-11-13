namespace GenericScriptableObjects.Usage_Example.Util
{
    using JetBrains.Annotations;
    using SolidUtilities.Attributes;
    using UnityEngine;

    internal class Readme : ScriptableObject
    {
        [SerializeField, ResizableTextArea, UsedImplicitly]
        private string _description = null;
    }
}