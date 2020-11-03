namespace GenericScriptableObjects.Util
{
    using System;
    using System.Linq;
    using TypeReferences;

    public static class TypeReferenceCollectionExtensions
    {
        public static TypeReference[] CastToTypeReference(this Type[] types)
        {
            return types.Select(type => new TypeReference(type, true)).ToArray();
        }

        public static Type[] CastToType(this TypeReference[] typeReferences)
        {
            return typeReferences.Select(typeRef => typeRef.Type).ToArray();
        }
    }
}