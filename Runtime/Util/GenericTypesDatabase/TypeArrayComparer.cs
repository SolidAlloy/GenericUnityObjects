namespace GenericUnityObjects.Util
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal readonly struct TypeArrayComparer : IEqualityComparer<Type[]>
    {
        public bool Equals(Type[] firstArray, Type[] secondArray)
        {
            if (firstArray == null && secondArray == null)
                return true;

            if (firstArray == null || secondArray == null)
                return false;

            return firstArray.SequenceEqual(secondArray);
        }

        public int GetHashCode(Type[] array)
        {
            int hashCode = 0;

            foreach (Type item in array)
                hashCode ^= item.GetHashCode();

            return hashCode;
        }
    }
}