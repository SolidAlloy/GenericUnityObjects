namespace GenericUnityObjects.Editor.GeneratedTypesDatabase
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;

    internal class Pool<T>
        where T : TypeInfo
    {
        private readonly Dictionary<T, T> _dict;
        private readonly Dictionary<string, string> _typeNameGUIDDict;

        public Pool(int capacity = 0)
        {
            _dict = new Dictionary<T, T>(capacity);
            _typeNameGUIDDict = new Dictionary<string, string>(capacity);
        }

        public T GetOrAdd(T item)
        {
            if (_dict.TryGetValue(item, out T existingItem))
                return existingItem;

            _dict.Add(item, item);
            _typeNameGUIDDict.Add(item.TypeNameAndAssembly, item.GUID);
            return item;
        }

        public void AddRange(T[] items)
        {
            foreach (T item in items)
            {
                if (_dict.ContainsKey(item))
                    continue;

                _dict.Add(item, item);
                _typeNameGUIDDict.Add(item.TypeNameAndAssembly, item.GUID);
            }
        }

        public void ChangeItem(ref T item, Action<T> changeItem)
        {
            if (_dict.TryGetValue(item, out T existingItem))
            {
                item = existingItem;
                _dict.Remove(item);
                _typeNameGUIDDict.Remove(item.TypeNameAndAssembly);
            }

            changeItem(item);

            _dict.Add(item, item);
            _typeNameGUIDDict.Add(item.TypeNameAndAssembly, item.GUID);
        }

        public void ChangeItem<TChildType>(ref TChildType item, Action<TChildType> changeItem)
            where TChildType : T
        {
            if (_dict.TryGetValue(item, out T existingItem))
            {
                item = (TChildType)existingItem;
                _dict.Remove(item);
                _typeNameGUIDDict.Remove(item.TypeNameAndAssembly);
            }

            changeItem(item);

            _dict.Add(item, item);
            _typeNameGUIDDict.Add(item.TypeNameAndAssembly, item.GUID);
        }

        [CanBeNull]
        public string GetGUID(string typeNameAndAssembly)
        {
            _typeNameGUIDDict.TryGetValue(typeNameAndAssembly, out string guid);
            return guid;
        }
    }
}