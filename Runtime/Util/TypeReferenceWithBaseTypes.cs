namespace GenericUnityObjects.Util
{
    using System;
    using TypeReferences;

    [Serializable]
    internal class TypeReferenceWithBaseTypes : TypeReference
    {
        public string[] BaseTypeNames;
    }
}