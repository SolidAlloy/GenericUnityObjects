namespace GenericUnityObjects.Util
{
    internal static class Config
    {
        public const string MainFolderPath = "Assets/GenericUnityObjects";
        public const string ResourcesPath = MainFolderPath + "/Resources";
        public const string EditorResourcesPath = MainFolderPath + "/EditorResources";
        public const string AssembliesDirPath = MainFolderPath + "/Assemblies";

        public const int AssemblyGenerationOrder = 1;
        public const int UnityObjectCreationOrder = 2;

        // Not const so that analyzers don't complain that the expression is always true/false.
        public static readonly bool Debug = true;
    }
}