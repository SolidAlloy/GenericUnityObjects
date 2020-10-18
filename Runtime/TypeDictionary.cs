namespace GenericScriptableObjects
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using TypeReferences;
    using UnityEngine;
    using Util;

    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Cryptography;
    using System.Threading;
    using Debug = UnityEngine.Debug;

    /// <summary>
    /// A serializable dictionary of type Dictionary&lt;TypeReference[], TypeReference>.
    /// </summary>
    [Serializable]
    internal class TypeDictionary : ISerializationCallbackReceiver
    {
        private readonly Dictionary<TypeReference[], TypeReference> _dict =
            new Dictionary<TypeReference[], TypeReference>(new TypeReferenceArrayComparer());

        [SerializeField] private TypeReferenceCollection[] _keys;

        [SerializeField] private TypeReference[] _values;

        public void Add(Type[] key, Type value) => _dict.Add(key.CastToTypeReference(), value);

        public bool ContainsKey(Type[] key)
        {
            Debug.Log($"Trying to check for key: {key[0]}");

            TypeReference[] lastKey = null;

            foreach (var kvp in _dict)
            {
                lastKey = kvp.Key;
            }

            var comparer = new TypeReferenceArrayComparer();
            bool equals = comparer.Equals(key.CastToTypeReference(), lastKey);
            Debug.Log($"What comparer says about keys equality: {equals}");

            if (equals)
            {
              Debug.Log($"hashcodes are equal: {comparer.GetHashCode(lastKey) == comparer.GetHashCode(key.CastToTypeReference())}");
            }

            return _dict.ContainsKey(key.CastToTypeReference());
        }

        public bool TryGetValue(TypeReference[] key, out TypeReference value) => _dict.TryGetValue(key, out value);

        public bool TryGetValue(Type[] key, out Type value)
        {
            bool result = TryGetValue(key.CastToTypeReference(), out TypeReference typeRef);
            value = typeRef;
            return result;
        }

        public void OnAfterDeserialize()
        {
            if (_keys == null || _values == null || _keys.Length != _values.Length)
                return;

            _dict.Clear();
            int keysLength = _keys.Length;

            for (int i = 0; i < keysLength; ++i)
            {
                if (_values[i].Type != null)
                    _dict[_keys[i]] = _values[i];
            }

            _keys = null;
            _values = null;
        }

        public void OnBeforeSerialize()
        {
            int dictLength = _dict.Count;
            _keys = new TypeReferenceCollection[dictLength];
            _values = new TypeReference[dictLength];

            int keysIndex = 0;
            foreach (var pair in _dict)
            {
                _keys[keysIndex] = pair.Key;
                _values[keysIndex] = pair.Value;
                ++keysIndex;
            }
        }

        /// <summary>
        /// A TypeReference[] container that is used because TypeReference[][] cannot be serialized by Unity.
        /// </summary>
        [Serializable]
        private class TypeReferenceCollection
        {
            [SerializeField] private TypeReference[] _array;

            public TypeReferenceCollection(TypeReference[] collection) => _array = collection;

            public TypeReferenceCollection() : this((TypeReference[]) null) { }

            public TypeReferenceCollection(Type[] collection) : this(collection.CastToTypeReference()) { }

            public static implicit operator TypeReferenceCollection(Type[] typeCollection) =>
                new TypeReferenceCollection(typeCollection);

            public static implicit operator TypeReferenceCollection(TypeReference[] typeRefCollection) =>
                new TypeReferenceCollection(typeRefCollection);

            public static implicit operator TypeReference[](TypeReferenceCollection typeRefCollection) =>
                typeRefCollection._array;
        }
    }

  /// <summary>Represents a collection of keys and values.</summary>
  /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
  /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
  [DebuggerDisplay("Count = {Count}")]
  [ComVisible(false)]
  [Serializable]
  public class Dictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IDictionary, ICollection, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, ISerializable, IDeserializationCallback
  {
    private int[] buckets;
    private Dictionary<TKey, TValue>.Entry[] entries;
    private int count;
    private int version;
    private int freeList;
    private int freeCount;
    private IEqualityComparer<TKey> comparer;
    private Dictionary<TKey, TValue>.KeyCollection keys;
    private Dictionary<TKey, TValue>.ValueCollection values;
    private object _syncRoot;
    private const string VersionName = "Version";
    private const string HashSizeName = "HashSize";
    private const string KeyValuePairsName = "KeyValuePairs";
    private const string ComparerName = "Comparer";

    /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that is empty, has the default initial capacity, and uses the default equality comparer for the key type.</summary>
    public Dictionary()
      : this(0, (IEqualityComparer<TKey>) null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that is empty, has the specified initial capacity, and uses the default equality comparer for the key type.</summary>
    /// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Generic.Dictionary`2" /> can contain.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="capacity" /> is less than 0.</exception>

    public Dictionary(int capacity)
      : this(capacity, (IEqualityComparer<TKey>) null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that is empty, has the default initial capacity, and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
    /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing keys, or <see langword="null" /> to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> for the type of the key.</param>

    public Dictionary(IEqualityComparer<TKey> comparer)
      : this(0, comparer)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that is empty, has the specified initial capacity, and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
    /// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Generic.Dictionary`2" /> can contain.</param>
    /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing keys, or <see langword="null" /> to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> for the type of the key.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="capacity" /> is less than 0.</exception>

    public Dictionary(int capacity, IEqualityComparer<TKey> comparer)
    {
      if (capacity < 0)
        throw new Exception();
      if (capacity > 0)
        this.Initialize(capacity);
      this.comparer = comparer ?? (IEqualityComparer<TKey>) EqualityComparer<TKey>.Default;
    }

    /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.Generic.IDictionary`2" /> and uses the default equality comparer for the key type.</summary>
    /// <param name="dictionary">The <see cref="T:System.Collections.Generic.IDictionary`2" /> whose elements are copied to the new <see cref="T:System.Collections.Generic.Dictionary`2" />.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="dictionary" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentException">
    /// <paramref name="dictionary" /> contains one or more duplicate keys.</exception>

    public Dictionary(IDictionary<TKey, TValue> dictionary)
      : this(dictionary, (IEqualityComparer<TKey>) null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.Generic.IDictionary`2" /> and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
    /// <param name="dictionary">The <see cref="T:System.Collections.Generic.IDictionary`2" /> whose elements are copied to the new <see cref="T:System.Collections.Generic.Dictionary`2" />.</param>
    /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing keys, or <see langword="null" /> to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> for the type of the key.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="dictionary" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentException">
    /// <paramref name="dictionary" /> contains one or more duplicate keys.</exception>

    public Dictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
      : this(dictionary != null ? dictionary.Count : 0, comparer)
    {
      if (dictionary == null)
        throw new Exception();
      foreach (KeyValuePair<TKey, TValue> keyValuePair in (IEnumerable<KeyValuePair<TKey, TValue>>) dictionary)
        this.Add(keyValuePair.Key, keyValuePair.Value);
    }

    /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class with serialized data.</summary>
    /// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object containing the information required to serialize the <see cref="T:System.Collections.Generic.Dictionary`2" />.</param>
    /// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> structure containing the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Generic.Dictionary`2" />.</param>
    protected Dictionary(SerializationInfo info, StreamingContext context)
    {
    }

    /// <summary>Gets the <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> that is used to determine equality of keys for the dictionary.</summary>
    /// <returns>The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> generic interface implementation that is used to determine equality of keys for the current <see cref="T:System.Collections.Generic.Dictionary`2" /> and to provide hash values for the keys.</returns>
    public IEqualityComparer<TKey> Comparer
    {
      get => this.comparer;
    }

    /// <summary>Gets the number of key/value pairs contained in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
    /// <returns>The number of key/value pairs contained in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</returns>
    public int Count
    {
      get => this.count - this.freeCount;
    }

    /// <summary>Gets a collection containing the keys in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
    /// <returns>A <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" /> containing the keys in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</returns>
    public Dictionary<TKey, TValue>.KeyCollection Keys
    {
      get
      {
        if (this.keys == null)
          this.keys = new Dictionary<TKey, TValue>.KeyCollection(this);
        return this.keys;
      }
    }

    ICollection<TKey> IDictionary<TKey, TValue>.Keys
    {
      get
      {
        if (this.keys == null)
          this.keys = new Dictionary<TKey, TValue>.KeyCollection(this);
        return (ICollection<TKey>) this.keys;
      }
    }

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
    {
       get
      {
        if (this.keys == null)
          this.keys = new Dictionary<TKey, TValue>.KeyCollection(this);
        return (IEnumerable<TKey>) this.keys;
      }
    }

    /// <summary>Gets a collection containing the values in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
    /// <returns>A <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" /> containing the values in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</returns>

    public Dictionary<TKey, TValue>.ValueCollection Values
    {
       get
      {
        if (this.values == null)
          this.values = new Dictionary<TKey, TValue>.ValueCollection(this);
        return this.values;
      }
    }


    ICollection<TValue> IDictionary<TKey, TValue>.Values
    {
       get
      {
        if (this.values == null)
          this.values = new Dictionary<TKey, TValue>.ValueCollection(this);
        return (ICollection<TValue>) this.values;
      }
    }


    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
    {
       get
      {
        if (this.values == null)
          this.values = new Dictionary<TKey, TValue>.ValueCollection(this);
        return (IEnumerable<TValue>) this.values;
      }
    }

    /// <summary>Gets or sets the value associated with the specified key.</summary>
    /// <param name="key">The key of the value to get or set.</param>
    /// <returns>The value associated with the specified key. If the specified key is not found, a get operation throws a <see cref="T:System.Collections.Generic.KeyNotFoundException" />, and a set operation creates a new element with the specified key.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="key" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key" /> does not exist in the collection.</exception>

    public TValue this[TKey key]
    {
       get
      {
        int entry = this.FindEntry(key);
        if (entry >= 0)
          return this.entries[entry].value;
        throw new Exception();
        return default (TValue);
      }
       set => this.Insert(key, value, false);
    }

    /// <summary>Adds the specified key and value to the dictionary.</summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add. The value can be <see langword="null" /> for reference types.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="key" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</exception>

    public void Add(TKey key, TValue value) => this.Insert(key, value, true);


    void ICollection<KeyValuePair<TKey, TValue>>.Add(
      KeyValuePair<TKey, TValue> keyValuePair)
    {
      this.Add(keyValuePair.Key, keyValuePair.Value);
    }


    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(
      KeyValuePair<TKey, TValue> keyValuePair)
    {
      int entry = this.FindEntry(keyValuePair.Key);
      return entry >= 0 && EqualityComparer<TValue>.Default.Equals(this.entries[entry].value, keyValuePair.Value);
    }


    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(
      KeyValuePair<TKey, TValue> keyValuePair)
    {
      int entry = this.FindEntry(keyValuePair.Key);
      if (entry < 0 || !EqualityComparer<TValue>.Default.Equals(this.entries[entry].value, keyValuePair.Value))
        return false;
      this.Remove(keyValuePair.Key);
      return true;
    }

    /// <summary>Removes all keys and values from the <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>

    public void Clear()
    {
      if (this.count <= 0)
        return;
      for (int index = 0; index < this.buckets.Length; ++index)
        this.buckets[index] = -1;
      Array.Clear((Array) this.entries, 0, this.count);
      this.freeList = -1;
      this.count = 0;
      this.freeCount = 0;
      ++this.version;
    }

    /// <summary>Determines whether the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains the specified key.</summary>
    /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</param>
    /// <returns>
    /// <see langword="true" /> if the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains an element with the specified key; otherwise, <see langword="false" />.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="key" /> is <see langword="null" />.</exception>

    public bool ContainsKey(TKey key) => this.FindEntry(key) >= 0;

    /// <summary>Determines whether the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains a specific value.</summary>
    /// <param name="value">The value to locate in the <see cref="T:System.Collections.Generic.Dictionary`2" />. The value can be <see langword="null" /> for reference types.</param>
    /// <returns>
    /// <see langword="true" /> if the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains an element with the specified value; otherwise, <see langword="false" />.</returns>

    public bool ContainsValue(TValue value)
    {
      if ((object) value == null)
      {
        for (int index = 0; index < this.count; ++index)
        {
          if (this.entries[index].hashCode >= 0 && (object) this.entries[index].value == null)
            return true;
        }
      }
      else
      {
        EqualityComparer<TValue> equalityComparer = EqualityComparer<TValue>.Default;
        for (int index = 0; index < this.count; ++index)
        {
          if (this.entries[index].hashCode >= 0 && equalityComparer.Equals(this.entries[index].value, value))
            return true;
        }
      }
      return false;
    }

    private void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
    {
      if (array == null)
        throw new Exception();
      if (index < 0 || index > array.Length)
        throw new Exception();
      if (array.Length - index < this.Count)
        throw new Exception();
      int count = this.count;
      Dictionary<TKey, TValue>.Entry[] entries = this.entries;
      for (int index1 = 0; index1 < count; ++index1)
      {
        if (entries[index1].hashCode >= 0)
          array[index++] = new KeyValuePair<TKey, TValue>(entries[index1].key, entries[index1].value);
      }
    }

    /// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
    /// <returns>A <see cref="T:System.Collections.Generic.Dictionary`2.Enumerator" /> structure for the <see cref="T:System.Collections.Generic.Dictionary`2" />.</returns>

    public Dictionary<TKey, TValue>.Enumerator GetEnumerator() => new Dictionary<TKey, TValue>.Enumerator(this, 2);


    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => (IEnumerator<KeyValuePair<TKey, TValue>>) new Dictionary<TKey, TValue>.Enumerator(this, 2);

    /// <summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and returns the data needed to serialize the <see cref="T:System.Collections.Generic.Dictionary`2" /> instance.</summary>
    /// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that contains the information required to serialize the <see cref="T:System.Collections.Generic.Dictionary`2" /> instance.</param>
    /// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> structure that contains the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Generic.Dictionary`2" /> instance.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="info" /> is <see langword="null" />.</exception>
    [SecurityCritical]
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new Exception();
      info.AddValue("Version", this.version);
      // info.AddValue("Comparer", HashHelpers.GetEqualityComparerForSerialization((object) this.comparer), typeof (IEqualityComparer<TKey>));
      info.AddValue("HashSize", this.buckets == null ? 0 : this.buckets.Length);
      if (this.buckets == null)
        return;
      KeyValuePair<TKey, TValue>[] array = new KeyValuePair<TKey, TValue>[this.Count];
      this.CopyTo(array, 0);
      info.AddValue("KeyValuePairs", (object) array, typeof (KeyValuePair<TKey, TValue>[]));
    }

    private int FindEntry(TKey key)
    {
      if ((object) key == null)
        throw new Exception();
      if (this.buckets != null)
      {
        int num = this.comparer.GetHashCode(key) & int.MaxValue;
        int testIndex = this.buckets[num % this.buckets.Length];
        if (testIndex < 0)
          Debug.Log("index < 0, there will be no further checks");
        
        for (int index = this.buckets[num % this.buckets.Length]; index >= 0; index = this.entries[index].next)
        {
          var first = this.entries[index].hashCode == num;
          var second = this.comparer.Equals(this.entries[index].key, key);
          Debug.Log($"first: {first}, second = {second}");
          if (first && second)
            return index;
        }
      }
      else
      {
        Debug.Log("buckets are null");
      }
      return -1;
    }

    private void Initialize(int capacity)
    {
      int prime = HashHelpers.GetPrime(capacity);
      this.buckets = new int[prime];
      for (int index = 0; index < this.buckets.Length; ++index)
        this.buckets[index] = -1;
      this.entries = new Dictionary<TKey, TValue>.Entry[prime];
      this.freeList = -1;
    }

    private void Insert(TKey key, TValue value, bool add)
    {
      if ((object) key == null)
        throw new Exception();
      if (this.buckets == null)
        this.Initialize(0);
      int num1 = this.comparer.GetHashCode(key) & int.MaxValue;
      int index1 = num1 % this.buckets.Length;
      int num2 = 0;
      for (int index2 = this.buckets[index1]; index2 >= 0; index2 = this.entries[index2].next)
      {
        if (this.entries[index2].hashCode == num1 && this.comparer.Equals(this.entries[index2].key, key))
        {
          if (add)
            throw new Exception();
          this.entries[index2].value = value;
          ++this.version;
          return;
        }
        ++num2;
      }
      int index3;
      if (this.freeCount > 0)
      {
        index3 = this.freeList;
        this.freeList = this.entries[index3].next;
        --this.freeCount;
      }
      else
      {
        if (this.count == this.entries.Length)
        {
          this.Resize();
          index1 = num1 % this.buckets.Length;
        }
        index3 = this.count;
        ++this.count;
      }
      this.entries[index3].hashCode = num1;
      this.entries[index3].next = this.buckets[index1];
      this.entries[index3].key = key;
      this.entries[index3].value = value;
      this.buckets[index1] = index3;
      ++this.version;
      this.Resize(this.entries.Length, true);
    }

    /// <summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and raises the deserialization event when the deserialization is complete.</summary>
    /// <param name="sender">The source of the deserialization event.</param>
    /// <exception cref="T:System.Runtime.Serialization.SerializationException">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object associated with the current <see cref="T:System.Collections.Generic.Dictionary`2" /> instance is invalid.</exception>
    public virtual void OnDeserialization(object sender)
    {
      SerializationInfo serializationInfo;
      HashHelpers.SerializationInfoTable.TryGetValue((object) this, out serializationInfo);
      if (serializationInfo == null)
        return;
      int int32_1 = serializationInfo.GetInt32("Version");
      int int32_2 = serializationInfo.GetInt32("HashSize");
      this.comparer = (IEqualityComparer<TKey>) serializationInfo.GetValue("Comparer", typeof (IEqualityComparer<TKey>));
      if (int32_2 != 0)
      {
        this.buckets = new int[int32_2];
        for (int index = 0; index < this.buckets.Length; ++index)
          this.buckets[index] = -1;
        this.entries = new Dictionary<TKey, TValue>.Entry[int32_2];
        this.freeList = -1;
        KeyValuePair<TKey, TValue>[] keyValuePairArray = (KeyValuePair<TKey, TValue>[]) serializationInfo.GetValue("KeyValuePairs", typeof (KeyValuePair<TKey, TValue>[]));
        if (keyValuePairArray == null)
          throw new Exception();
        for (int index = 0; index < keyValuePairArray.Length; ++index)
        {
          if ((object) keyValuePairArray[index].Key == null)
            throw new Exception();
          this.Insert(keyValuePairArray[index].Key, keyValuePairArray[index].Value, true);
        }
      }
      else
        this.buckets = (int[]) null;
      this.version = int32_1;
      HashHelpers.SerializationInfoTable.Remove((object) this);
    }

    private void Resize() => this.Resize(HashHelpers.ExpandPrime(this.count), false);

    private void Resize(int newSize, bool forceNewHashCodes)
    {
      int[] numArray = new int[newSize];
      for (int index = 0; index < numArray.Length; ++index)
        numArray[index] = -1;
      Dictionary<TKey, TValue>.Entry[] entryArray = new Dictionary<TKey, TValue>.Entry[newSize];
      Array.Copy((Array) this.entries, 0, (Array) entryArray, 0, this.count);
      if (forceNewHashCodes)
      {
        for (int index = 0; index < this.count; ++index)
        {
          if (entryArray[index].hashCode != -1)
            entryArray[index].hashCode = this.comparer.GetHashCode(entryArray[index].key) & int.MaxValue;
        }
      }
      for (int index1 = 0; index1 < this.count; ++index1)
      {
        if (entryArray[index1].hashCode >= 0)
        {
          int index2 = entryArray[index1].hashCode % newSize;
          entryArray[index1].next = numArray[index2];
          numArray[index2] = index1;
        }
      }
      this.buckets = numArray;
      this.entries = entryArray;
    }

    /// <summary>Removes the value with the specified key from the <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <returns>
    /// <see langword="true" /> if the element is successfully found and removed; otherwise, <see langword="false" />.  This method returns <see langword="false" /> if <paramref name="key" /> is not found in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="key" /> is <see langword="null" />.</exception>

    public bool Remove(TKey key)
    {
      if ((object) key == null)
        throw new Exception();
      if (this.buckets != null)
      {
        int num = this.comparer.GetHashCode(key) & int.MaxValue;
        int index1 = num % this.buckets.Length;
        int index2 = -1;
        for (int index3 = this.buckets[index1]; index3 >= 0; index3 = this.entries[index3].next)
        {
          if (this.entries[index3].hashCode == num && this.comparer.Equals(this.entries[index3].key, key))
          {
            if (index2 < 0)
              this.buckets[index1] = this.entries[index3].next;
            else
              this.entries[index2].next = this.entries[index3].next;
            this.entries[index3].hashCode = -1;
            this.entries[index3].next = this.freeList;
            this.entries[index3].key = default (TKey);
            this.entries[index3].value = default (TValue);
            this.freeList = index3;
            ++this.freeCount;
            ++this.version;
            return true;
          }
          index2 = index3;
        }
      }
      return false;
    }

    /// <summary>Gets the value associated with the specified key.</summary>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
    /// <returns>
    /// <see langword="true" /> if the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains an element with the specified key; otherwise, <see langword="false" />.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="key" /> is <see langword="null" />.</exception>

    public bool TryGetValue(TKey key, out TValue value)
    {
      int entry = this.FindEntry(key);
      if (entry >= 0)
      {
        value = this.entries[entry].value;
        return true;
      }
      value = default (TValue);
      return false;
    }

    internal TValue GetValueOrDefault(TKey key)
    {
      int entry = this.FindEntry(key);
      return entry >= 0 ? this.entries[entry].value : default (TValue);
    }


    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
    {
       get => false;
    }


    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(
      KeyValuePair<TKey, TValue>[] array,
      int index)
    {
      this.CopyTo(array, index);
    }


    void ICollection.CopyTo(Array array, int index)
    {
      if (array == null)
        throw new Exception();
      if (array.Rank != 1)
        throw new Exception();
      if (array.GetLowerBound(0) != 0)
        throw new Exception();
      if (index < 0 || index > array.Length)
        throw new Exception();
      if (array.Length - index < this.Count)
        throw new Exception();
      switch (array)
      {
        case KeyValuePair<TKey, TValue>[] array1:
          this.CopyTo(array1, index);
          break;
        case DictionaryEntry[] _:
          DictionaryEntry[] dictionaryEntryArray = array as DictionaryEntry[];
          Dictionary<TKey, TValue>.Entry[] entries1 = this.entries;
          for (int index1 = 0; index1 < this.count; ++index1)
          {
            if (entries1[index1].hashCode >= 0)
              dictionaryEntryArray[index++] = new DictionaryEntry((object) entries1[index1].key, (object) entries1[index1].value);
          }
          break;
        case object[] objArray:
label_18:
          try
          {
            int count = this.count;
            Dictionary<TKey, TValue>.Entry[] entries2 = this.entries;
            for (int index1 = 0; index1 < count; ++index1)
            {
              if (entries2[index1].hashCode >= 0)
              {
                int index2 = index++;
              }
            }
            break;
          }
          catch (ArrayTypeMismatchException ex)
          {
            throw new Exception();
            break;
          }
        default:
          throw new Exception();
          goto label_18;
      }
    }


    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) new Dictionary<TKey, TValue>.Enumerator(this, 2);


    bool ICollection.IsSynchronized
    {
       get => false;
    }


    object ICollection.SyncRoot
    {
       get
      {
        if (this._syncRoot == null)
          Interlocked.CompareExchange<object>(ref this._syncRoot, new object(), (object) null);
        return this._syncRoot;
      }
    }


    bool IDictionary.IsFixedSize
    {
       get => false;
    }


    bool IDictionary.IsReadOnly
    {
       get => false;
    }


    ICollection IDictionary.Keys
    {
       get => (ICollection) this.Keys;
    }


    ICollection IDictionary.Values
    {
       get => (ICollection) this.Values;
    }


    object IDictionary.this[object key]
    {
       get
      {
        if (Dictionary<TKey, TValue>.IsCompatibleKey(key))
        {
          int entry = this.FindEntry((TKey) key);
          if (entry >= 0)
            return (object) this.entries[entry].value;
        }
        return (object) null;
      }
       set
      {
        if (key == null)
          throw new Exception();

        try
        {
          TKey key1 = (TKey) key;
          try
          {
            this[key1] = (TValue) value;
          }
          catch (InvalidCastException ex)
          {
            throw new Exception();
          }
        }
        catch (InvalidCastException ex)
        {
          throw new Exception();
        }
      }
    }

    private static bool IsCompatibleKey(object key)
    {
      if (key == null)
        throw new Exception();
      return key is TKey;
    }


    void IDictionary.Add(object key, object value)
    {
      if (key == null)
        throw new Exception();

      try
      {
        TKey key1 = (TKey) key;
        try
        {
          this.Add(key1, (TValue) value);
        }
        catch (InvalidCastException ex)
        {
          throw new Exception();
        }
      }
      catch (InvalidCastException ex)
      {
        throw new Exception();
      }
    }


    bool IDictionary.Contains(object key) => Dictionary<TKey, TValue>.IsCompatibleKey(key) && this.ContainsKey((TKey) key);


    IDictionaryEnumerator IDictionary.GetEnumerator() => (IDictionaryEnumerator) new Dictionary<TKey, TValue>.Enumerator(this, 1);


    void IDictionary.Remove(object key)
    {
      if (!Dictionary<TKey, TValue>.IsCompatibleKey(key))
        return;
      this.Remove((TKey) key);
    }

    private struct Entry
    {
      public int hashCode;
      public int next;
      public TKey key;
      public TValue value;
    }

    /// <summary>Enumerates the elements of a <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
    /// <typeparam name="TKey" />
    /// <typeparam name="TValue" />

    [Serializable]
    public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable, IEnumerator, IDictionaryEnumerator
    {
      private Dictionary<TKey, TValue> dictionary;
      private int version;
      private int index;
      private KeyValuePair<TKey, TValue> current;
      private int getEnumeratorRetType;
      internal const int DictEntry = 1;
      internal const int KeyValuePair = 2;

      internal Enumerator(Dictionary<TKey, TValue> dictionary, int getEnumeratorRetType)
      {
        this.dictionary = dictionary;
        this.version = dictionary.version;
        this.index = 0;
        this.getEnumeratorRetType = getEnumeratorRetType;
        this.current = new KeyValuePair<TKey, TValue>();
      }

      /// <summary>Advances the enumerator to the next element of the <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
      /// <returns>
      /// <see langword="true" /> if the enumerator was successfully advanced to the next element; <see langword="false" /> if the enumerator has passed the end of the collection.</returns>
      /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created.</exception>

      public bool MoveNext()
      {
        if (this.version != this.dictionary.version)
          throw new Exception();
        for (; (uint) this.index < (uint) this.dictionary.count; ++this.index)
        {
          if (this.dictionary.entries[this.index].hashCode >= 0)
          {
            this.current = new KeyValuePair<TKey, TValue>(this.dictionary.entries[this.index].key, this.dictionary.entries[this.index].value);
            ++this.index;
            return true;
          }
        }
        this.index = this.dictionary.count + 1;
        this.current = new KeyValuePair<TKey, TValue>();
        return false;
      }

      /// <summary>Gets the element at the current position of the enumerator.</summary>
      /// <returns>The element in the <see cref="T:System.Collections.Generic.Dictionary`2" /> at the current position of the enumerator.</returns>

      public KeyValuePair<TKey, TValue> Current
      {
         get => this.current;
      }

      /// <summary>Releases all resources used by the <see cref="T:System.Collections.Generic.Dictionary`2.Enumerator" />.</summary>

      public void Dispose()
      {
      }


      object IEnumerator.Current
      {
         get
        {
          if (this.index == 0 || this.index == this.dictionary.count + 1)
            throw new Exception();
          return this.getEnumeratorRetType == 1 ? (object) new DictionaryEntry((object) this.current.Key, (object) this.current.Value) : (object) new KeyValuePair<TKey, TValue>(this.current.Key, this.current.Value);
        }
      }


      void IEnumerator.Reset()
      {
        if (this.version != this.dictionary.version)
          throw new Exception();
        this.index = 0;
        this.current = new KeyValuePair<TKey, TValue>();
      }


      DictionaryEntry IDictionaryEnumerator.Entry
      {
         get
        {
          if (this.index == 0 || this.index == this.dictionary.count + 1)
            throw new Exception();
          return new DictionaryEntry((object) this.current.Key, (object) this.current.Value);
        }
      }


      object IDictionaryEnumerator.Key
      {
         get
        {
          if (this.index == 0 || this.index == this.dictionary.count + 1)
            throw new Exception();
          return (object) this.current.Key;
        }
      }


      object IDictionaryEnumerator.Value
      {
         get
        {
          if (this.index == 0 || this.index == this.dictionary.count + 1)
            throw new Exception();
          return (object) this.current.Value;
        }
      }
    }

    /// <summary>Represents the collection of keys in a <see cref="T:System.Collections.Generic.Dictionary`2" />. This class cannot be inherited.</summary>
    /// <typeparam name="TKey" />
    /// <typeparam name="TValue" />
    [DebuggerDisplay("Count = {Count}")]

    [Serializable]
    public sealed class KeyCollection : ICollection<TKey>, IEnumerable<TKey>, IEnumerable, ICollection, IReadOnlyCollection<TKey>
    {
      private Dictionary<TKey, TValue> dictionary;

      /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" /> class that reflects the keys in the specified <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
      /// <param name="dictionary">The <see cref="T:System.Collections.Generic.Dictionary`2" /> whose keys are reflected in the new <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" />.</param>
      /// <exception cref="T:System.ArgumentNullException">
      /// <paramref name="dictionary" /> is <see langword="null" />.</exception>

      public KeyCollection(Dictionary<TKey, TValue> dictionary)
      {
        if (dictionary == null)
          throw new Exception();
        this.dictionary = dictionary;
      }

      /// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" />.</summary>
      /// <returns>A <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection.Enumerator" /> for the <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" />.</returns>

      public Dictionary<TKey, TValue>.KeyCollection.Enumerator GetEnumerator() => new Dictionary<TKey, TValue>.KeyCollection.Enumerator(this.dictionary);

      /// <summary>Copies the <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" /> elements to an existing one-dimensional <see cref="T:System.Array" />, starting at the specified array index.</summary>
      /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
      /// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
      /// <exception cref="T:System.ArgumentNullException">
      /// <paramref name="array" /> is <see langword="null" />.</exception>
      /// <exception cref="T:System.ArgumentOutOfRangeException">
      /// <paramref name="index" /> is less than zero.</exception>
      /// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>

      public void CopyTo(TKey[] array, int index)
      {
        if (array == null)
          throw new Exception();
        if (index < 0 || index > array.Length)
          throw new Exception();
        if (array.Length - index < this.dictionary.Count)
          throw new Exception();
        int count = this.dictionary.count;
        Dictionary<TKey, TValue>.Entry[] entries = this.dictionary.entries;
        for (int index1 = 0; index1 < count; ++index1)
        {
          if (entries[index1].hashCode >= 0)
            array[index++] = entries[index1].key;
        }
      }

      /// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" />.</summary>
      /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" />.
      /// Retrieving the value of this property is an O(1) operation.</returns>

      public int Count
      {
         get => this.dictionary.Count;
      }


      bool ICollection<TKey>.IsReadOnly
      {
         get => true;
      }


      void ICollection<TKey>.Add(TKey item) => throw new Exception();


      void ICollection<TKey>.Clear() => throw new Exception();


      bool ICollection<TKey>.Contains(TKey item) => this.dictionary.ContainsKey(item);


      bool ICollection<TKey>.Remove(TKey item)
      {
        throw new Exception();
        return false;
      }


      IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() => (IEnumerator<TKey>) new Dictionary<TKey, TValue>.KeyCollection.Enumerator(this.dictionary);


      IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) new Dictionary<TKey, TValue>.KeyCollection.Enumerator(this.dictionary);


      void ICollection.CopyTo(Array array, int index)
      {
        if (array == null)
          throw new Exception();
        if (array.Rank != 1)
          throw new Exception();
        if (array.GetLowerBound(0) != 0)
          throw new Exception();
        if (index < 0 || index > array.Length)
          throw new Exception();
        if (array.Length - index < this.dictionary.Count)
          throw new Exception();
        switch (array)
        {
          case TKey[] array1:
            this.CopyTo(array1, index);
            break;
          case object[] objArray:
label_13:
            int count = this.dictionary.count;
            Dictionary<TKey, TValue>.Entry[] entries = this.dictionary.entries;
            try
            {
              for (int index1 = 0; index1 < count; ++index1)
              {
                if (entries[index1].hashCode >= 0)
                {
                  int index2 = index++;
                }
              }
              break;
            }
            catch (ArrayTypeMismatchException ex)
            {
              throw new Exception();
              break;
            }
          default:
            throw new Exception();
            goto label_13;
        }
      }


      bool ICollection.IsSynchronized
      {
         get => false;
      }


      object ICollection.SyncRoot
      {
         get => ((ICollection) this.dictionary).SyncRoot;
      }

      /// <summary>Enumerates the elements of a <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" />.</summary>
      /// <typeparam name="TKey" />
      /// <typeparam name="TValue" />

      [Serializable]
      public struct Enumerator : IEnumerator<TKey>, IDisposable, IEnumerator
      {
        private Dictionary<TKey, TValue> dictionary;
        private int index;
        private int version;
        private TKey currentKey;

        internal Enumerator(Dictionary<TKey, TValue> dictionary)
        {
          this.dictionary = dictionary;
          this.version = dictionary.version;
          this.index = 0;
          this.currentKey = default (TKey);
        }

        /// <summary>Releases all resources used by the <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection.Enumerator" />.</summary>

        public void Dispose()
        {
        }

        /// <summary>Advances the enumerator to the next element of the <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" />.</summary>
        /// <returns>
        /// <see langword="true" /> if the enumerator was successfully advanced to the next element; <see langword="false" /> if the enumerator has passed the end of the collection.</returns>
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created.</exception>

        public bool MoveNext()
        {
          if (this.version != this.dictionary.version)
            throw new Exception();
          for (; (uint) this.index < (uint) this.dictionary.count; ++this.index)
          {
            if (this.dictionary.entries[this.index].hashCode >= 0)
            {
              this.currentKey = this.dictionary.entries[this.index].key;
              ++this.index;
              return true;
            }
          }
          this.index = this.dictionary.count + 1;
          this.currentKey = default (TKey);
          return false;
        }

        /// <summary>Gets the element at the current position of the enumerator.</summary>
        /// <returns>The element in the <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection" /> at the current position of the enumerator.</returns>

        public TKey Current
        {
           get => this.currentKey;
        }

        object IEnumerator.Current
        {
           get
          {
            if (this.index == 0 || this.index == this.dictionary.count + 1)
              throw new Exception();
            return (object) this.currentKey;
          }
        }

        void IEnumerator.Reset()
        {
          if (this.version != this.dictionary.version)
            throw new Exception();
          this.index = 0;
          this.currentKey = default (TKey);
        }
      }
    }

    /// <summary>Represents the collection of values in a <see cref="T:System.Collections.Generic.Dictionary`2" />. This class cannot be inherited.</summary>
    /// <typeparam name="TKey" />
    /// <typeparam name="TValue" />
    [DebuggerDisplay("Count = {Count}")]

    [Serializable]
    public sealed class ValueCollection : ICollection<TValue>, IEnumerable<TValue>, IEnumerable, ICollection, IReadOnlyCollection<TValue>
    {
      private Dictionary<TKey, TValue> dictionary;

      /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" /> class that reflects the values in the specified <see cref="T:System.Collections.Generic.Dictionary`2" />.</summary>
      /// <param name="dictionary">The <see cref="T:System.Collections.Generic.Dictionary`2" /> whose values are reflected in the new <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" />.</param>
      /// <exception cref="T:System.ArgumentNullException">
      /// <paramref name="dictionary" /> is <see langword="null" />.</exception>

      public ValueCollection(Dictionary<TKey, TValue> dictionary)
      {
        if (dictionary == null)
          throw new Exception();
        this.dictionary = dictionary;
      }

      /// <summary>Returns an enumerator that iterates through the <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" />.</summary>
      /// <returns>A <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection.Enumerator" /> for the <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" />.</returns>

      public Dictionary<TKey, TValue>.ValueCollection.Enumerator GetEnumerator() => new Dictionary<TKey, TValue>.ValueCollection.Enumerator(this.dictionary);

      /// <summary>Copies the <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" /> elements to an existing one-dimensional <see cref="T:System.Array" />, starting at the specified array index.</summary>
      /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
      /// <param name="index">The zero-based index in <paramref name="array" /> at which copying begins.</param>
      /// <exception cref="T:System.ArgumentNullException">
      /// <paramref name="array" /> is <see langword="null" />.</exception>
      /// <exception cref="T:System.ArgumentOutOfRangeException">
      /// <paramref name="index" /> is less than zero.</exception>
      /// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" /> is greater than the available space from <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>

      public void CopyTo(TValue[] array, int index)
      {
        if (array == null)
          throw new Exception();
        if (index < 0 || index > array.Length)
          throw new Exception();
        if (array.Length - index < this.dictionary.Count)
          throw new Exception();
        int count = this.dictionary.count;
        Dictionary<TKey, TValue>.Entry[] entries = this.dictionary.entries;
        for (int index1 = 0; index1 < count; ++index1)
        {
          if (entries[index1].hashCode >= 0)
            array[index++] = entries[index1].value;
        }
      }

      /// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" />.</summary>
      /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" />.</returns>

      public int Count
      {
         get => this.dictionary.Count;
      }


      bool ICollection<TValue>.IsReadOnly
      {
         get => true;
      }


      void ICollection<TValue>.Add(TValue item) => throw new Exception();


      bool ICollection<TValue>.Remove(TValue item)
      {
        throw new Exception();
        return false;
      }


      void ICollection<TValue>.Clear() => throw new Exception();


      bool ICollection<TValue>.Contains(TValue item) => this.dictionary.ContainsValue(item);


      IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => (IEnumerator<TValue>) new Dictionary<TKey, TValue>.ValueCollection.Enumerator(this.dictionary);


      IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) new Dictionary<TKey, TValue>.ValueCollection.Enumerator(this.dictionary);


      void ICollection.CopyTo(Array array, int index)
      {
        if (array == null)
          throw new Exception();
        if (array.Rank != 1)
          throw new Exception();
        if (array.GetLowerBound(0) != 0)
          throw new Exception();
        if (index < 0 || index > array.Length)
          throw new Exception();
        if (array.Length - index < this.dictionary.Count)
          throw new Exception();
        switch (array)
        {
          case TValue[] array1:
            this.CopyTo(array1, index);
            break;
          case object[] objArray:
label_13:
            int count = this.dictionary.count;
            Dictionary<TKey, TValue>.Entry[] entries = this.dictionary.entries;
            try
            {
              for (int index1 = 0; index1 < count; ++index1)
              {
                if (entries[index1].hashCode >= 0)
                {
                  int index2 = index++;
                }
              }
              break;
            }
            catch (ArrayTypeMismatchException ex)
            {
              throw new Exception();
              break;
            }
          default:
            throw new Exception();
            goto label_13;
        }
      }


      bool ICollection.IsSynchronized
      {
         get => false;
      }


      object ICollection.SyncRoot
      {
         get => ((ICollection) this.dictionary).SyncRoot;
      }

      /// <summary>Enumerates the elements of a <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" />.</summary>
      /// <typeparam name="TKey" />
      /// <typeparam name="TValue" />

      [Serializable]
      public struct Enumerator : IEnumerator<TValue>, IDisposable, IEnumerator
      {
        private Dictionary<TKey, TValue> dictionary;
        private int index;
        private int version;
        private TValue currentValue;

        internal Enumerator(Dictionary<TKey, TValue> dictionary)
        {
          this.dictionary = dictionary;
          this.version = dictionary.version;
          this.index = 0;
          this.currentValue = default (TValue);
        }

        /// <summary>Releases all resources used by the <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection.Enumerator" />.</summary>

        public void Dispose()
        {
        }

        /// <summary>Advances the enumerator to the next element of the <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" />.</summary>
        /// <returns>
        /// <see langword="true" /> if the enumerator was successfully advanced to the next element; <see langword="false" /> if the enumerator has passed the end of the collection.</returns>
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created.</exception>
        public bool MoveNext()
        {
          if (this.version != this.dictionary.version)
            throw new Exception();
          for (; (uint) this.index < (uint) this.dictionary.count; ++this.index)
          {
            if (this.dictionary.entries[this.index].hashCode >= 0)
            {
              this.currentValue = this.dictionary.entries[this.index].value;
              ++this.index;
              return true;
            }
          }
          this.index = this.dictionary.count + 1;
          this.currentValue = default (TValue);
          return false;
        }

        /// <summary>Gets the element at the current position of the enumerator.</summary>
        /// <returns>The element in the <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection" /> at the current position of the enumerator.</returns>
        public TValue Current
        {
          get => this.currentValue;
        }

        object IEnumerator.Current
        {
          get
          {
            if (this.index == 0 || this.index == this.dictionary.count + 1)
              throw new Exception();
            return (object) this.currentValue;
          }
        }

        void IEnumerator.Reset()
        {
          if (this.version != this.dictionary.version)
            throw new Exception();
          this.index = 0;
          this.currentValue = default (TValue);
        }
      }
    }
  }

  internal static class HashHelpers
  {
    public const int HashCollisionThreshold = 100;
    public static readonly int[] primes = new int[72]
    {
      3,
      7,
      11,
      17,
      23,
      29,
      37,
      47,
      59,
      71,
      89,
      107,
      131,
      163,
      197,
      239,
      293,
      353,
      431,
      521,
      631,
      761,
      919,
      1103,
      1327,
      1597,
      1931,
      2333,
      2801,
      3371,
      4049,
      4861,
      5839,
      7013,
      8419,
      10103,
      12143,
      14591,
      17519,
      21023,
      25229,
      30293,
      36353,
      43627,
      52361,
      62851,
      75431,
      90523,
      108631,
      130363,
      156437,
      187751,
      225307,
      270371,
      324449,
      389357,
      467237,
      560689,
      672827,
      807403,
      968897,
      1162687,
      1395263,
      1674319,
      2009191,
      2411033,
      2893249,
      3471899,
      4166287,
      4999559,
      5999471,
      7199369
    };
    private static ConditionalWeakTable<object, SerializationInfo> s_SerializationInfoTable;
    public const int MaxPrimeArrayLength = 2146435069;
    private const int bufferSize = 1024;
    private static RandomNumberGenerator rng;
    private static byte[] data;
    private static int currentIndex = 1024;
    private static readonly object lockObj = new object();

    internal static ConditionalWeakTable<object, SerializationInfo> SerializationInfoTable
    {
      get
      {
        if (HashHelpers.s_SerializationInfoTable == null)
        {
          ConditionalWeakTable<object, SerializationInfo> conditionalWeakTable = new ConditionalWeakTable<object, SerializationInfo>();
          Interlocked.CompareExchange<ConditionalWeakTable<object, SerializationInfo>>(ref HashHelpers.s_SerializationInfoTable, conditionalWeakTable, (ConditionalWeakTable<object, SerializationInfo>) null);
        }
        return HashHelpers.s_SerializationInfoTable;
      }
    }

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    public static bool IsPrime(int candidate)
    {
      if ((candidate & 1) == 0)
        return candidate == 2;
      int num = (int) Math.Sqrt((double) candidate);
      for (int index = 3; index <= num; index += 2)
      {
        if (candidate % index == 0)
          return false;
      }
      return true;
    }

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    public static int GetPrime(int min)
    {
      if (min < 0)
        throw new Exception();
      for (int index = 0; index < HashHelpers.primes.Length; ++index)
      {
        int prime = HashHelpers.primes[index];
        if (prime >= min)
          return prime;
      }
      for (int candidate = min | 1; candidate < int.MaxValue; candidate += 2)
      {
        if (HashHelpers.IsPrime(candidate) && (candidate - 1) % 101 != 0)
          return candidate;
      }
      return min;
    }

    public static int GetMinPrime() => HashHelpers.primes[0];

    public static int ExpandPrime(int oldSize)
    {
      int min = 2 * oldSize;
      return (uint) min > 2146435069U && 2146435069 > oldSize ? 2146435069 : HashHelpers.GetPrime(min);
    }

    internal static long GetEntropy()
    {
      lock (HashHelpers.lockObj)
      {
        if (HashHelpers.currentIndex == 1024)
        {
          if (HashHelpers.rng == null)
          {
            HashHelpers.rng = RandomNumberGenerator.Create();
            HashHelpers.data = new byte[1024];
          }
          HashHelpers.rng.GetBytes(HashHelpers.data);
          HashHelpers.currentIndex = 0;
        }
        long int64 = BitConverter.ToInt64(HashHelpers.data, HashHelpers.currentIndex);
        HashHelpers.currentIndex += 8;
        return int64;
      }
    }
  }

}