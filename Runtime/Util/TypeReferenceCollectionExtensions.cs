namespace GenericUnityObjects.Util
{
    using System;
    using System.Linq;
    using TypeReferences;

    internal static class TypeReferenceCollectionExtensions
    {
        public static TypeReference[] CastToTypeReference(this Type[] types)
        {
            return types.Select(type => new TypeReference(type, suppressLogs: true)).ToArray();
        }

        public static Type[] CastToType(this TypeReferenceWithBaseTypes[] typeReferences)
        {
            return typeReferences.Select(typeRef => typeRef.Type).ToArray();
        }
    }
}