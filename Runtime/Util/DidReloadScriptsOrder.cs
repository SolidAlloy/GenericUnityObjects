namespace GenericUnityObjects.Util
{
    internal enum DidReloadScriptsOrder
    {
        BeforeAssemblyGeneration = 1,
        AssemblyGeneration = 2,
        AfterAssemblyGeneration = 3,
        UnityObjectCreation = 4
    }
}