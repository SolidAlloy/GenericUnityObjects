namespace GenericScriptableObjects.Util
{
    using System;
    using System.Linq;
    using TypeReferences;

    public static class TypeReferenceCollectionExtensions
    {
        public static TypeReference[] CastToTypeReference(this Type[] types)
        {
            return types.Select(type => (TypeReference) type).ToArray();
        }

        public static Type[] CastToType(this TypeReference[] typeReferences)
        {
            return typeReferences.Select(typeRef => (Type) typeRef).ToArray();
        }
    }
}