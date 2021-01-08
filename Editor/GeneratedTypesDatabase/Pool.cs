namespace GenericUnityObjects.Editor.GeneratedTypesDatabase
{
    using System;
    using System.Collections.Generic;

    internal class Pool<T>
    {
        private readonly Dictionary<T, T> _dict;

        public Pool(int capacity = 0)
        {
            _dict = new Dictionary<T, T>(capacity);
        }

        public T GetOrAdd(T item)
        {
            if (_dict.TryGetValue(item, out T existingItem))
                return existingItem;

            _dict[item] = item;
            return item;
        }

        public void AddRange(T[] items)
        {
            foreach (T item in items)
            {
                if (! _dict.ContainsKey(item))
                    _dict.Add(item, item);
            }
        }

        public void ChangeItem(ref T item, Action<T> changeItem)
        {
            if (_dict.TryGetValue(item, out T existingItem))
            {
                item = existingItem;
                _dict.Remove(item);
            }

            changeItem(item);

            _dict[item] = item;
        }
    }
}