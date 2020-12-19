namespace GenericUnityObjects.Editor.MonoBehaviour
{
    using System;

    [Serializable]
    internal class ArgumentInfo : TypeInfo
    {
        public ArgumentInfo(string typeNameAndAssembly, string guid)
            : base(typeNameAndAssembly, guid) { }

        public ArgumentInfo(string typeFullName, string assemblyName, string guid)
            : base(typeFullName, assemblyName, guid) { }

        public ArgumentInfo(Type type)
            : base(type) { }
    }
}