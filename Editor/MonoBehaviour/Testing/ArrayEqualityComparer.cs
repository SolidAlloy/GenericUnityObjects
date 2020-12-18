namespace GenericUnityObjects.Editor.MonoBehaviour
{
    using System.Collections.Generic;

    public sealed class ArrayEqualityComparer<T> : IEqualityComparer<T[]>
    {
        // You could make this a per-instance field with a constructor parameter
        private static readonly EqualityComparer<T> ElementComparer
            = EqualityComparer<T>.Default;

        public bool Equals(T[] first, T[] second)
        {
            if (first == second)
                return true;

            if (first == null || second == null)
                return false;

            if (first.Length != second.Length)
                return false;

            for (int i = 0; i < first.Length; i++)
            {
                if ( ! ElementComparer.Equals(first[i], second[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(T[] array)
        {
            unchecked
            {
                int hash = 17;

                foreach (T element in array)
                {
                    hash = hash * 31 + ElementComparer.GetHashCode(element);
                }

                return hash;
            }
        }
    }
}