namespace GenericUnityObjects.Util.Database
{
    using TypeReferences;

    internal static class TypeReferenceExtensions
    {
        public static bool TypeIsMissing(this TypeReference typeRef) =>
            typeRef.Type == null && typeRef.GUID == string.Empty;
    }
}