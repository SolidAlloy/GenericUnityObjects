namespace GenericUnityObjects.Util
{
    internal static class Config
    {
        public const string GeneratedTypesNamespace = "GeneratedTypes";
        public const string MainFolderPath = "Assets/GenericUnityObjects";
        public const string ResourcesPath = MainFolderPath + "/Resources";
        public const string GeneratedTypesPath = MainFolderPath + "/GeneratedTypes";
        public const string MenuItemsPath = ResourcesPath + "/Editor/MenuItems.cs";

        public const string NewLine = "\r\n";
        public const string RawNewLine = @"\r\n";
        public const string Tab = "    ";

        public const string AssembliesDirPath = MainFolderPath + "/Assemblies";

        // Not const so that analyzers don't complain that the expression is always true/false.
        public static readonly bool Debug = true;

        public const int AssemblyGenerationOrder = 1;
        public const int UnityObjectCreationOrder = 2;
    }
}