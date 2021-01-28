namespace GenericUnityObjects.Util
{
    using System;
    using System.Linq;
    using TypeReferences;

    internal static class TypeReferenceCollectionExtensions
    {
        public static Type[] CastToType(this TypeReferenceWithBaseTypes[] typeReferences)
        {
            var types = new Type[typeReferences.Length];

            for (int i = 0; i < typeReferences.Length; i++)
            {
                types[i] = typeReferences[i].Type;
            }

            return types;
        }
    }
}