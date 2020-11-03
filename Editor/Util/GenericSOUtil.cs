namespace GenericScriptableObjects.Editor.Util
{
    internal static class GenericSOUtil
    {
        public static string GetClassSafeTypeName(string rawTypeName)
        {
            return rawTypeName
                .Replace('.', '_')
                .Replace('`', '_');
        }
    }
}