namespace GenericScriptableObjects.Editor
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Globalization;
  using System.Reflection;
  using System.Resources;
  using System.Runtime.Serialization;
  using System.Security;
  using System.Security.Cryptography;
  using System.Security.Permissions;
  using System.Threading;

  [Serializable]
  public class HashSet<T> : ISerializable, IDeserializationCallback, ISet<T>, IReadOnlyCollection<T>
  {
    private int[] m_buckets;
    private Slot[] m_slots;
    private int m_count;
    private int m_lastIndex;
    private int m_freeList;
    private IEqualityComparer<T> m_comparer;
    private int m_version;
    private SerializationInfo m_siInfo;

    /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that is empty and uses the default equality comparer for the set type.</summary>
    public HashSet()
      : this(EqualityComparer<T>.Default)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that is empty, but has reserved space for <paramref name="capacity" /> items and uses the default equality comparer for the set type.
    /// </summary>
    /// <param name="capacity">The initial size of the <see cref="T:System.Collections.Generic.HashSet`1" /></param>
    public HashSet(int capacity)
      : this(capacity, EqualityComparer<T>.Default)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that is empty and uses the specified equality comparer for the set type.</summary>
    /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing values in the set, or <see langword="null" /> to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> implementation for the set type.</param>
    public HashSet(IEqualityComparer<T> comparer)
    {
      if (comparer == null)
        comparer = EqualityComparer<T>.Default;
      m_comparer = comparer;
      m_lastIndex = 0;
      m_count = 0;
      m_freeList = -1;
      m_version = 0;
    }

    /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that uses the default equality comparer for the set type, contains elements copied from the specified collection, and has sufficient capacity to accommodate the number of elements copied.</summary>
    /// <param name="collection">The collection whose elements are copied to the new set.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="collection" /> is <see langword="null" />.</exception>
    public HashSet(IEnumerable<T> collection)
      : this(collection, EqualityComparer<T>.Default)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that uses the specified equality comparer for the set type, contains elements copied from the specified collection, and has sufficient capacity to accommodate the number of elements copied.</summary>
    /// <param name="collection">The collection whose elements are copied to the new set.</param>
    /// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing values in the set, or <see langword="null" /> to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> implementation for the set type.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="collection" /> is <see langword="null" />.</exception>
    public HashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
      : this(comparer)
    {
      int capacity;
      switch (collection)
      {
        case null:
          throw new ArgumentNullException(nameof (collection));
        case HashSet<T> objSet when AreEqualityComparersEqual(this, objSet):
          CopyFrom(objSet);
          return;
        case ICollection<T> objs:
          capacity = objs.Count;
          break;
        default:
          capacity = 0;
          break;
      }
      Initialize(capacity);
      UnionWith(collection);
      if (m_count <= 0 || m_slots.Length / m_count <= 3)
        return;
      TrimExcess();
    }

    private void CopyFrom(HashSet<T> source)
    {
      int count = source.m_count;
      if (count == 0)
        return;
      int length = source.m_buckets.Length;
      if (HashHelpers.ExpandPrime(count + 1) >= length)
      {
        m_buckets = (int[]) source.m_buckets.Clone();
        m_slots = (Slot[]) source.m_slots.Clone();
        m_lastIndex = source.m_lastIndex;
        m_freeList = source.m_freeList;
      }
      else
      {
        int lastIndex = source.m_lastIndex;
        Slot[] slots = source.m_slots;
        Initialize(count);
        int index1 = 0;
        for (int index2 = 0; index2 < lastIndex; ++index2)
        {
          int hashCode = slots[index2].hashCode;
          if (hashCode >= 0)
          {
            AddValue(index1, hashCode, slots[index2].value);
            ++index1;
          }
        }
        m_lastIndex = index1;
      }
      m_count = count;
    }

    /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class with serialized data.</summary>
    /// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that contains the information required to serialize the <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
    /// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> structure that contains the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
    protected HashSet(SerializationInfo info, StreamingContext context) => m_siInfo = info;

    /// <summary>
    ///       Initializes a new instance of the <see cref="T:System.Collections.Generic.HashSet`1" /> class that uses the specified equality comparer for the set type, and has sufficient capacity to accommodate <paramref name="capacity" /> elements.
    /// </summary>
    /// <param name="capacity">The initial size of the <see cref="T:System.Collections.Generic.HashSet`1" /></param>
    /// <param name="comparer">
    /// 	The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing values in the set, or null (Nothing in Visual Basic) to use the default <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation for the set type.
    /// </param>
    public HashSet(int capacity, IEqualityComparer<T> comparer)
      : this(comparer)
    {
      if (capacity < 0)
        throw new ArgumentOutOfRangeException(nameof (capacity));
      if (capacity <= 0)
        return;
      Initialize(capacity);
    }

    void ICollection<T>.Add(T item) => AddIfNotPresent(item);

    /// <summary>Removes all elements from a <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
    public void Clear()
    {
      if (m_lastIndex > 0)
      {
        Array.Clear(m_slots, 0, m_lastIndex);
        Array.Clear(m_buckets, 0, m_buckets.Length);
        m_lastIndex = 0;
        m_count = 0;
        m_freeList = -1;
      }
      ++m_version;
    }

    /// <summary>Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object contains the specified element.</summary>
    /// <param name="item">The element to locate in the <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
    /// <returns>
    /// <see langword="true" /> if the <see cref="T:System.Collections.Generic.HashSet`1" /> object contains the specified element; otherwise, <see langword="false" />.</returns>
    public bool Contains(T item)
    {
      if (m_buckets != null)
      {
        int hashCode = InternalGetHashCode(item);
        for (int index = m_buckets[hashCode % m_buckets.Length] - 1; index >= 0; index = m_slots[index].next)
        {
          if (m_slots[index].hashCode == hashCode && m_comparer.Equals(m_slots[index].value, item))
            return true;
        }
      }
      return false;
    }

    /// <summary>Copies the elements of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to an array, starting at the specified array index.</summary>
    /// <param name="array">The one-dimensional array that is the destination of the elements copied from the <see cref="T:System.Collections.Generic.HashSet`1" /> object. The array must have zero-based indexing.</param>
    /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="array" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="arrayIndex" /> is less than 0.</exception>
    /// <exception cref="T:System.ArgumentException">
    /// <paramref name="arrayIndex" /> is greater than the length of the destination <paramref name="array" />.</exception>
    public void CopyTo(T[] array, int arrayIndex) => CopyTo(array, arrayIndex, m_count);

    /// <summary>Removes the specified element from a <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
    /// <param name="item">The element to remove.</param>
    /// <returns>
    /// <see langword="true" /> if the element is successfully found and removed; otherwise, <see langword="false" />.  This method returns <see langword="false" /> if <paramref name="item" /> is not found in the <see cref="T:System.Collections.Generic.HashSet`1" /> object.</returns>
    public bool Remove(T item)
    {
      if (m_buckets != null)
      {
        int hashCode = InternalGetHashCode(item);
        int index1 = hashCode % m_buckets.Length;
        int index2 = -1;
        for (int index3 = m_buckets[index1] - 1; index3 >= 0; index3 = m_slots[index3].next)
        {
          if (m_slots[index3].hashCode == hashCode && m_comparer.Equals(m_slots[index3].value, item))
          {
            if (index2 < 0)
              m_buckets[index1] = m_slots[index3].next + 1;
            else
              m_slots[index2].next = m_slots[index3].next;
            m_slots[index3].hashCode = -1;
            m_slots[index3].value = default;
            m_slots[index3].next = m_freeList;
            --m_count;
            ++m_version;
            if (m_count == 0)
            {
              m_lastIndex = 0;
              m_freeList = -1;
            }
            else
              m_freeList = index3;
            return true;
          }
          index2 = index3;
        }
      }
      return false;
    }

    /// <summary>Gets the number of elements that are contained in a set.</summary>
    /// <returns>The number of elements that are contained in the set.</returns>
    public int Count
    {
      get => m_count;
    }

    bool ICollection<T>.IsReadOnly
    {
      get => false;
    }

    /// <summary>Returns an enumerator that iterates through a <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
    /// <returns>A <see cref="T:System.Collections.Generic.HashSet`1.Enumerator" /> object for the <see cref="T:System.Collections.Generic.HashSet`1" /> object.</returns>
    public Enumerator GetEnumerator() => new Enumerator(this);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);

    IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

    /// <summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and returns the data needed to serialize a <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
    /// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that contains the information required to serialize the <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
    /// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> structure that contains the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="info" /> is <see langword="null" />.</exception>
    [SecurityCritical]
    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new ArgumentNullException(nameof (info));
      info.AddValue("Version", m_version);
      info.AddValue("Comparer", HashHelpers.GetEqualityComparerForSerialization(m_comparer), typeof (IEqualityComparer<T>));
      info.AddValue("Capacity", m_buckets == null ? 0 : m_buckets.Length);
      if (m_buckets == null)
        return;
      T[] array = new T[m_count];
      CopyTo(array);
      info.AddValue("Elements", array, typeof (T[]));
    }

    /// <summary>Implements the <see cref="T:System.Runtime.Serialization.ISerializable" /> interface and raises the deserialization event when the deserialization is complete.</summary>
    /// <param name="sender">The source of the deserialization event.</param>
    /// <exception cref="T:System.Runtime.Serialization.SerializationException">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object associated with the current <see cref="T:System.Collections.Generic.HashSet`1" /> object is invalid.</exception>
    public virtual void OnDeserialization(object sender)
    {
      if (m_siInfo == null)
        return;
      int int32 = m_siInfo.GetInt32("Capacity");
      m_comparer = (IEqualityComparer<T>) m_siInfo.GetValue("Comparer", typeof (IEqualityComparer<T>));
      m_freeList = -1;
      if (int32 != 0)
      {
        m_buckets = new int[int32];
        m_slots = new Slot[int32];
        T[] objArray = (T[]) m_siInfo.GetValue("Elements", typeof (T[]));
        if (objArray == null)
          throw new SerializationException(SR.GetString("Serialization_MissingKeys"));
        for (int index = 0; index < objArray.Length; ++index)
          AddIfNotPresent(objArray[index]);
      }
      else
        m_buckets = null;
      m_version = m_siInfo.GetInt32("Version");
      m_siInfo = null;
    }

    /// <summary>Adds the specified element to a set.</summary>
    /// <param name="item">The element to add to the set.</param>
    /// <returns>
    /// <see langword="true" /> if the element is added to the <see cref="T:System.Collections.Generic.HashSet`1" /> object; <see langword="false" /> if the element is already present.</returns>
    public bool Add(T item) => AddIfNotPresent(item);

    /// <summary>Searches the set for a given value and returns the equal value it finds, if any.</summary>
    /// <param name="equalValue">The value to search for.</param>
    /// <param name="actualValue">The value from the set that the search found, or the default value of T when the search yielded no match.</param>
    /// <returns>A value indicating whether the search was successful.</returns>
    public bool TryGetValue(T equalValue, out T actualValue)
    {
      if (m_buckets != null)
      {
        int index = InternalIndexOf(equalValue);
        if (index >= 0)
        {
          actualValue = m_slots[index].value;
          return true;
        }
      }
      actualValue = default;
      return false;
    }

    /// <summary>Modifies the current <see cref="T:System.Collections.Generic.HashSet`1" /> object to contain all elements that are present in itself, the specified collection, or both.</summary>
    /// <param name="other">The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="other" /> is <see langword="null" />.</exception>
    public void UnionWith(IEnumerable<T> other)
    {
      if (other == null)
        throw new ArgumentNullException(nameof (other));
      foreach (T obj in other)
        AddIfNotPresent(obj);
    }

    /// <summary>Modifies the current <see cref="T:System.Collections.Generic.HashSet`1" /> object to contain only elements that are present in that object and in the specified collection.</summary>
    /// <param name="other">The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="other" /> is <see langword="null" />.</exception>
    public void IntersectWith(IEnumerable<T> other)
    {
      if (other == null)
        throw new ArgumentNullException(nameof (other));
      if (m_count == 0)
        return;
      if (other is ICollection<T> objs)
      {
        if (objs.Count == 0)
        {
          Clear();
          return;
        }
        if (other is HashSet<T> objSet && AreEqualityComparersEqual(this, objSet))
        {
          IntersectWithHashSetWithSameEC(objSet);
          return;
        }
      }
      IntersectWithEnumerable(other);
    }

    /// <summary>Removes all elements in the specified collection from the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
    /// <param name="other">The collection of items to remove from the <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="other" /> is <see langword="null" />.</exception>
    public void ExceptWith(IEnumerable<T> other)
    {
      if (other == null)
        throw new ArgumentNullException(nameof (other));
      if (m_count == 0)
        return;
      if (other == this)
      {
        Clear();
      }
      else
      {
        foreach (T obj in other)
          Remove(obj);
      }
    }

    /// <summary>Modifies the current <see cref="T:System.Collections.Generic.HashSet`1" /> object to contain only elements that are present either in that object or in the specified collection, but not both.</summary>
    /// <param name="other">The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="other" /> is <see langword="null" />.</exception>
    public void SymmetricExceptWith(IEnumerable<T> other)
    {
      if (other == null)
        throw new ArgumentNullException(nameof (other));
      if (m_count == 0)
        UnionWith(other);
      else if (other == this)
        Clear();
      else if (other is HashSet<T> objSet && AreEqualityComparersEqual(this, objSet))
        SymmetricExceptWithUniqueHashSet(objSet);
      else
        SymmetricExceptWithEnumerable(other);
    }

    /// <summary>Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object is a subset of the specified collection.</summary>
    /// <param name="other">The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
    /// <returns>
    /// <see langword="true" /> if the <see cref="T:System.Collections.Generic.HashSet`1" /> object is a subset of <paramref name="other" />; otherwise, <see langword="false" />.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="other" /> is <see langword="null" />.</exception>
    public bool IsSubsetOf(IEnumerable<T> other)
    {
      if (other == null)
        throw new ArgumentNullException(nameof (other));
      if (m_count == 0)
        return true;
      if (other is HashSet<T> objSet && AreEqualityComparersEqual(this, objSet))
        return m_count <= objSet.Count && IsSubsetOfHashSetWithSameEC(objSet);
      ElementCount elementCount = CheckUniqueAndUnfoundElements(other, false);
      return elementCount.uniqueCount == m_count && elementCount.unfoundCount >= 0;
    }

    /// <summary>Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object is a proper subset of the specified collection.</summary>
    /// <param name="other">The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
    /// <returns>
    /// <see langword="true" /> if the <see cref="T:System.Collections.Generic.HashSet`1" /> object is a proper subset of <paramref name="other" />; otherwise, <see langword="false" />.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="other" /> is <see langword="null" />.</exception>
    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
      if (other == null)
        throw new ArgumentNullException(nameof (other));
      if (other is ICollection<T> objs)
      {
        if (m_count == 0)
          return objs.Count > 0;
        if (other is HashSet<T> objSet && AreEqualityComparersEqual(this, objSet))
          return m_count < objSet.Count && IsSubsetOfHashSetWithSameEC(objSet);
      }
      ElementCount elementCount = CheckUniqueAndUnfoundElements(other, false);
      return elementCount.uniqueCount == m_count && elementCount.unfoundCount > 0;
    }

    /// <summary>Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object is a superset of the specified collection.</summary>
    /// <param name="other">The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
    /// <returns>
    /// <see langword="true" /> if the <see cref="T:System.Collections.Generic.HashSet`1" /> object is a superset of <paramref name="other" />; otherwise, <see langword="false" />.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="other" /> is <see langword="null" />.</exception>
    public bool IsSupersetOf(IEnumerable<T> other)
    {
      if (other == null)
        throw new ArgumentNullException(nameof (other));
      if (other is ICollection<T> objs)
      {
        if (objs.Count == 0)
          return true;
        if (other is HashSet<T> set2 && AreEqualityComparersEqual(this, set2) && set2.Count > m_count)
          return false;
      }
      return ContainsAllElements(other);
    }

    /// <summary>Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object is a proper superset of the specified collection.</summary>
    /// <param name="other">The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object. </param>
    /// <returns>
    /// <see langword="true" /> if the <see cref="T:System.Collections.Generic.HashSet`1" /> object is a proper superset of <paramref name="other" />; otherwise, <see langword="false" />.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="other" /> is <see langword="null" />.</exception>
    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
      if (other == null)
        throw new ArgumentNullException(nameof (other));
      if (m_count == 0)
        return false;
      if (other is ICollection<T> objs)
      {
        if (objs.Count == 0)
          return true;
        if (other is HashSet<T> set2 && AreEqualityComparersEqual(this, set2))
          return set2.Count < m_count && ContainsAllElements(set2);
      }
      ElementCount elementCount = CheckUniqueAndUnfoundElements(other, true);
      return elementCount.uniqueCount < m_count && elementCount.unfoundCount == 0;
    }

    /// <summary>Determines whether the current <see cref="T:System.Collections.Generic.HashSet`1" /> object and a specified collection share common elements.</summary>
    /// <param name="other">The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
    /// <returns>
    /// <see langword="true" /> if the <see cref="T:System.Collections.Generic.HashSet`1" /> object and <paramref name="other" /> share at least one common element; otherwise, <see langword="false" />.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="other" /> is <see langword="null" />.</exception>
    public bool Overlaps(IEnumerable<T> other)
    {
      if (other == null)
        throw new ArgumentNullException(nameof (other));
      if (m_count == 0)
        return false;
      foreach (T obj in other)
      {
        if (Contains(obj))
          return true;
      }
      return false;
    }

    /// <summary>Determines whether a <see cref="T:System.Collections.Generic.HashSet`1" /> object and the specified collection contain the same elements.</summary>
    /// <param name="other">The collection to compare to the current <see cref="T:System.Collections.Generic.HashSet`1" /> object.</param>
    /// <returns>
    /// <see langword="true" /> if the <see cref="T:System.Collections.Generic.HashSet`1" /> object is equal to <paramref name="other" />; otherwise, false.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="other" /> is <see langword="null" />.</exception>
    public bool SetEquals(IEnumerable<T> other)
    {
      switch (other)
      {
        case null:
          throw new ArgumentNullException(nameof (other));
        case HashSet<T> set2 when AreEqualityComparersEqual(this, set2):
          return m_count == set2.Count && ContainsAllElements(set2);
        case ICollection<T> objs when m_count == 0 && objs.Count > 0:
          return false;
        default:
          ElementCount elementCount = CheckUniqueAndUnfoundElements(other, true);
          return elementCount.uniqueCount == m_count && elementCount.unfoundCount == 0;
      }
    }

    /// <summary>Copies the elements of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to an array.</summary>
    /// <param name="array">The one-dimensional array that is the destination of the elements copied from the <see cref="T:System.Collections.Generic.HashSet`1" /> object. The array must have zero-based indexing.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="array" /> is <see langword="null" />.</exception>
    public void CopyTo(T[] array) => CopyTo(array, 0, m_count);

    /// <summary>Copies the specified number of elements of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to an array, starting at the specified array index.</summary>
    /// <param name="array">The one-dimensional array that is the destination of the elements copied from the <see cref="T:System.Collections.Generic.HashSet`1" /> object. The array must have zero-based indexing.</param>
    /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
    /// <param name="count">The number of elements to copy to <paramref name="array" />.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="array" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="arrayIndex" /> is less than 0.-or-
    /// <paramref name="count" /> is less than 0.</exception>
    /// <exception cref="T:System.ArgumentException">
    /// <paramref name="arrayIndex" /> is greater than the length of the destination <paramref name="array" />.-or-
    /// <paramref name="count" /> is greater than the available space from the <paramref name="index" /> to the end of the destination <paramref name="array" />.</exception>
    public void CopyTo(T[] array, int arrayIndex, int count)
    {
      if (array == null)
        throw new ArgumentNullException(nameof (array));
      if (arrayIndex < 0)
        throw new ArgumentOutOfRangeException(nameof (arrayIndex), SR.GetString("ArgumentOutOfRange_NeedNonNegNum"));
      if (count < 0)
        throw new ArgumentOutOfRangeException(nameof (count), SR.GetString("ArgumentOutOfRange_NeedNonNegNum"));
      if (arrayIndex > array.Length || count > array.Length - arrayIndex)
        throw new ArgumentException(SR.GetString("Arg_ArrayPlusOffTooSmall"));
      int num = 0;
      for (int index = 0; index < m_lastIndex && num < count; ++index)
      {
        if (m_slots[index].hashCode >= 0)
        {
          array[arrayIndex + num] = m_slots[index].value;
          ++num;
        }
      }
    }

    /// <summary>Removes all elements that match the conditions defined by the specified predicate from a <see cref="T:System.Collections.Generic.HashSet`1" /> collection.</summary>
    /// <param name="match">The <see cref="T:System.Predicate`1" /> delegate that defines the conditions of the elements to remove.</param>
    /// <returns>The number of elements that were removed from the <see cref="T:System.Collections.Generic.HashSet`1" /> collection.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="match" /> is <see langword="null" />.</exception>
    public int RemoveWhere(Predicate<T> match)
    {
      if (match == null)
        throw new ArgumentNullException(nameof (match));
      int num = 0;
      for (int index = 0; index < m_lastIndex; ++index)
      {
        if (m_slots[index].hashCode >= 0)
        {
          T obj = m_slots[index].value;
          if (match(obj) && Remove(obj))
            ++num;
        }
      }
      return num;
    }

    /// <summary>Gets the <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> object that is used to determine equality for the values in the set.</summary>
    /// <returns>The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> object that is used to determine equality for the values in the set.</returns>
    public IEqualityComparer<T> Comparer
    {
      get => m_comparer;
    }

    /// <summary>Sets the capacity of a <see cref="T:System.Collections.Generic.HashSet`1" /> object to the actual number of elements it contains, rounded up to a nearby, implementation-specific value.</summary>
    public void TrimExcess()
    {
      if (m_count == 0)
      {
        m_buckets = null;
        m_slots = null;
        ++m_version;
      }
      else
      {
        int prime = HashHelpers.GetPrime(m_count);
        Slot[] slotArray = new Slot[prime];
        int[] numArray = new int[prime];
        int index1 = 0;
        for (int index2 = 0; index2 < m_lastIndex; ++index2)
        {
          if (m_slots[index2].hashCode >= 0)
          {
            slotArray[index1] = m_slots[index2];
            int index3 = slotArray[index1].hashCode % prime;
            slotArray[index1].next = numArray[index3] - 1;
            numArray[index3] = index1 + 1;
            ++index1;
          }
        }
        m_lastIndex = index1;
        m_slots = slotArray;
        m_buckets = numArray;
        m_freeList = -1;
      }
    }

    /// <summary>Returns an <see cref="T:System.Collections.IEqualityComparer" /> object that can be used for equality testing of a <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
    /// <returns>An <see cref="T:System.Collections.IEqualityComparer" /> object that can be used for deep equality testing of the <see cref="T:System.Collections.Generic.HashSet`1" /> object.</returns>
    public static IEqualityComparer<HashSet<T>> CreateSetComparer() => new HashSetEqualityComparer<T>();

    private void Initialize(int capacity)
    {
      int prime = HashHelpers.GetPrime(capacity);
      m_buckets = new int[prime];
      m_slots = new Slot[prime];
    }

    private void IncreaseCapacity()
    {
      int newSize = HashHelpers.ExpandPrime(m_count);
      if (newSize <= m_count)
        throw new ArgumentException(SR.GetString("Arg_HSCapacityOverflow"));
      SetCapacity(newSize, false);
    }

    private void SetCapacity(int newSize, bool forceNewHashCodes)
    {
      Slot[] slotArray = new Slot[newSize];
      if (m_slots != null)
        Array.Copy(m_slots, 0, slotArray, 0, m_lastIndex);
      if (forceNewHashCodes)
      {
        for (int index = 0; index < m_lastIndex; ++index)
        {
          if (slotArray[index].hashCode != -1)
            slotArray[index].hashCode = InternalGetHashCode(slotArray[index].value);
        }
      }
      int[] numArray = new int[newSize];
      for (int index1 = 0; index1 < m_lastIndex; ++index1)
      {
        int index2 = slotArray[index1].hashCode % newSize;
        slotArray[index1].next = numArray[index2] - 1;
        numArray[index2] = index1 + 1;
      }
      m_slots = slotArray;
      m_buckets = numArray;
    }

    private bool AddIfNotPresent(T value)
    {
      if (m_buckets == null)
        Initialize(0);
      int hashCode = InternalGetHashCode(value);
      int index1 = hashCode % m_buckets.Length;
      int num = 0;
      for (int index2 = m_buckets[hashCode % m_buckets.Length] - 1; index2 >= 0; index2 = m_slots[index2].next)
      {
        if (m_slots[index2].hashCode == hashCode && m_comparer.Equals(m_slots[index2].value, value))
          return false;
        ++num;
      }
      int index3;
      if (m_freeList >= 0)
      {
        index3 = m_freeList;
        m_freeList = m_slots[index3].next;
      }
      else
      {
        if (m_lastIndex == m_slots.Length)
        {
          IncreaseCapacity();
          index1 = hashCode % m_buckets.Length;
        }
        index3 = m_lastIndex;
        ++m_lastIndex;
      }
      m_slots[index3].hashCode = hashCode;
      m_slots[index3].value = value;
      m_slots[index3].next = m_buckets[index1] - 1;
      m_buckets[index1] = index3 + 1;
      ++m_count;
      ++m_version;
      if (num > 100 && HashHelpers.IsWellKnownEqualityComparer(m_comparer))
      {
        m_comparer = (IEqualityComparer<T>) HashHelpers.GetRandomizedEqualityComparer(m_comparer);
        SetCapacity(m_buckets.Length, true);
      }
      return true;
    }

    private void AddValue(int index, int hashCode, T value)
    {
      int index1 = hashCode % m_buckets.Length;
      m_slots[index].hashCode = hashCode;
      m_slots[index].value = value;
      m_slots[index].next = m_buckets[index1] - 1;
      m_buckets[index1] = index + 1;
    }

    private bool ContainsAllElements(IEnumerable<T> other)
    {
      foreach (T obj in other)
      {
        if (!Contains(obj))
          return false;
      }
      return true;
    }

    private bool IsSubsetOfHashSetWithSameEC(HashSet<T> other)
    {
      foreach (T obj in this)
      {
        if (!other.Contains(obj))
          return false;
      }
      return true;
    }

    private void IntersectWithHashSetWithSameEC(HashSet<T> other)
    {
      for (int index = 0; index < m_lastIndex; ++index)
      {
        if (m_slots[index].hashCode >= 0)
        {
          T obj = m_slots[index].value;
          if (!other.Contains(obj))
            Remove(obj);
        }
      }
    }

    [SecuritySafeCritical]
    private unsafe void IntersectWithEnumerable(IEnumerable<T> other)
    {
      int lastIndex = m_lastIndex;
      int intArrayLength = BitHelper.ToIntArrayLength(lastIndex);
      BitHelper bitHelper;
      if (intArrayLength <= 100)
      {
        int* bitArrayPtr = stackalloc int[intArrayLength];
        bitHelper = new BitHelper(bitArrayPtr, intArrayLength);
      }
      else
        bitHelper = new BitHelper(new int[intArrayLength], intArrayLength);
      foreach (T obj in other)
      {
        int bitPosition = InternalIndexOf(obj);
        if (bitPosition >= 0)
          bitHelper.MarkBit(bitPosition);
      }
      for (int bitPosition = 0; bitPosition < lastIndex; ++bitPosition)
      {
        if (m_slots[bitPosition].hashCode >= 0 && !bitHelper.IsMarked(bitPosition))
          Remove(m_slots[bitPosition].value);
      }
    }

    private int InternalIndexOf(T item)
    {
      int hashCode = InternalGetHashCode(item);
      for (int index = m_buckets[hashCode % m_buckets.Length] - 1; index >= 0; index = m_slots[index].next)
      {
        if (m_slots[index].hashCode == hashCode && m_comparer.Equals(m_slots[index].value, item))
          return index;
      }
      return -1;
    }

    private void SymmetricExceptWithUniqueHashSet(HashSet<T> other)
    {
      foreach (T obj in other)
      {
        if (!Remove(obj))
          AddIfNotPresent(obj);
      }
    }

    [SecuritySafeCritical]
    private unsafe void SymmetricExceptWithEnumerable(IEnumerable<T> other)
    {
      int lastIndex = m_lastIndex;
      int intArrayLength = BitHelper.ToIntArrayLength(lastIndex);
      BitHelper bitHelper1;
      BitHelper bitHelper2;
      if (intArrayLength <= 50)
      {
        int* bitArrayPtr1 = stackalloc int[intArrayLength];
        bitHelper1 = new BitHelper(bitArrayPtr1, intArrayLength);
        int* bitArrayPtr2 = stackalloc int[intArrayLength];
        bitHelper2 = new BitHelper(bitArrayPtr2, intArrayLength);
      }
      else
      {
        bitHelper1 = new BitHelper(new int[intArrayLength], intArrayLength);
        bitHelper2 = new BitHelper(new int[intArrayLength], intArrayLength);
      }
      foreach (T obj in other)
      {
        int location = 0;
        if (AddOrGetLocation(obj, out location))
          bitHelper2.MarkBit(location);
        else if (location < lastIndex && !bitHelper2.IsMarked(location))
          bitHelper1.MarkBit(location);
      }
      for (int bitPosition = 0; bitPosition < lastIndex; ++bitPosition)
      {
        if (bitHelper1.IsMarked(bitPosition))
          Remove(m_slots[bitPosition].value);
      }
    }

    private bool AddOrGetLocation(T value, out int location)
    {
      int hashCode = InternalGetHashCode(value);
      int index1 = hashCode % m_buckets.Length;
      for (int index2 = m_buckets[hashCode % m_buckets.Length] - 1; index2 >= 0; index2 = m_slots[index2].next)
      {
        if (m_slots[index2].hashCode == hashCode && m_comparer.Equals(m_slots[index2].value, value))
        {
          location = index2;
          return false;
        }
      }
      int index3;
      if (m_freeList >= 0)
      {
        index3 = m_freeList;
        m_freeList = m_slots[index3].next;
      }
      else
      {
        if (m_lastIndex == m_slots.Length)
        {
          IncreaseCapacity();
          index1 = hashCode % m_buckets.Length;
        }
        index3 = m_lastIndex;
        ++m_lastIndex;
      }
      m_slots[index3].hashCode = hashCode;
      m_slots[index3].value = value;
      m_slots[index3].next = m_buckets[index1] - 1;
      m_buckets[index1] = index3 + 1;
      ++m_count;
      ++m_version;
      location = index3;
      return true;
    }

    [SecuritySafeCritical]
    private unsafe ElementCount CheckUniqueAndUnfoundElements(
      IEnumerable<T> other,
      bool returnIfUnfound)
    {
      if (m_count == 0)
      {
        int num = 0;
        using (IEnumerator<T> enumerator = other.GetEnumerator())
        {
          if (enumerator.MoveNext())
          {
            T current = enumerator.Current;
            ++num;
          }
        }
        ElementCount elementCount;
        elementCount.uniqueCount = 0;
        elementCount.unfoundCount = num;
        return elementCount;
      }
      int intArrayLength = BitHelper.ToIntArrayLength(m_lastIndex);
      BitHelper bitHelper;
      if (intArrayLength <= 100)
      {
        int* bitArrayPtr = stackalloc int[intArrayLength];
        bitHelper = new BitHelper(bitArrayPtr, intArrayLength);
      }
      else
        bitHelper = new BitHelper(new int[intArrayLength], intArrayLength);
      int num1 = 0;
      int num2 = 0;
      foreach (T obj in other)
      {
        int bitPosition = InternalIndexOf(obj);
        if (bitPosition >= 0)
        {
          if (!bitHelper.IsMarked(bitPosition))
          {
            bitHelper.MarkBit(bitPosition);
            ++num2;
          }
        }
        else
        {
          ++num1;
          if (returnIfUnfound)
            break;
        }
      }
      ElementCount elementCount1;
      elementCount1.uniqueCount = num2;
      elementCount1.unfoundCount = num1;
      return elementCount1;
    }

    internal T[] ToArray()
    {
      T[] array = new T[Count];
      CopyTo(array);
      return array;
    }

    internal static bool HashSetEquals(
      HashSet<T> set1,
      HashSet<T> set2,
      IEqualityComparer<T> comparer)
    {
      if (set1 == null)
        return set2 == null;
      if (set2 == null)
        return false;
      if (AreEqualityComparersEqual(set1, set2))
      {
        if (set1.Count != set2.Count)
          return false;
        foreach (T obj in set2)
        {
          if (!set1.Contains(obj))
            return false;
        }
        return true;
      }
      foreach (T x in set2)
      {
        bool flag = false;
        foreach (T y in set1)
        {
          if (comparer.Equals(x, y))
          {
            flag = true;
            break;
          }
        }
        if (!flag)
          return false;
      }
      return true;
    }

    private static bool AreEqualityComparersEqual(HashSet<T> set1, HashSet<T> set2) => set1.Comparer.Equals(set2.Comparer);

    private int InternalGetHashCode(T item) => (object) item == null ? 0 : m_comparer.GetHashCode(item) & int.MaxValue;

    internal struct ElementCount
    {
      internal int uniqueCount;
      internal int unfoundCount;
    }

    internal struct Slot
    {
      internal int hashCode;
      internal int next;
      internal T value;
    }

    /// <summary>Enumerates the elements of a <see cref="T:System.Collections.Generic.HashSet`1" /> object.</summary>
    [Serializable]
    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
    public struct Enumerator : IEnumerator<T>
    {
      private HashSet<T> set;
      private int index;
      private int version;
      private T current;

      internal Enumerator(HashSet<T> set)
      {
        this.set = set;
        index = 0;
        version = set.m_version;
        current = default;
      }

      /// <summary>Releases all resources used by a <see cref="T:System.Collections.Generic.HashSet`1.Enumerator" /> object.</summary>
      public void Dispose()
      {
      }

      /// <summary>Advances the enumerator to the next element of the <see cref="T:System.Collections.Generic.HashSet`1" /> collection.</summary>
      /// <returns>
      /// <see langword="true" /> if the enumerator was successfully advanced to the next element; <see langword="false" /> if the enumerator has passed the end of the collection.</returns>
      /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
      public bool MoveNext()
      {
        if (version != set.m_version)
          throw new InvalidOperationException(SR.GetString("InvalidOperation_EnumFailedVersion"));
        for (; index < set.m_lastIndex; ++index)
        {
          if (set.m_slots[index].hashCode >= 0)
          {
            current = set.m_slots[index].value;
            ++index;
            return true;
          }
        }
        index = set.m_lastIndex + 1;
        current = default;
        return false;
      }

      /// <summary>Gets the element at the current position of the enumerator.</summary>
      /// <returns>The element in the <see cref="T:System.Collections.Generic.HashSet`1" /> collection at the current position of the enumerator.</returns>
      public T Current
      {
        get => current;
      }

      object IEnumerator.Current
      {
        get
        {
          if (index == 0 || index == set.m_lastIndex + 1)
            throw new InvalidOperationException(SR.GetString("InvalidOperation_EnumOpCantHappen"));
          return Current;
        }
      }

      void IEnumerator.Reset()
      {
        if (version != set.m_version)
          throw new InvalidOperationException(SR.GetString("InvalidOperation_EnumFailedVersion"));
        index = 0;
        current = default;
      }
    }
  }
  internal static class HashHelpers
  {
    private static readonly int[] primes =
    {
      3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919, 1103, 1327,
      1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591, 17519, 21023, 25229, 30293,
      36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437, 187751, 225307, 270371, 324449, 389357, 467237,
      560689, 672827, 807403, 968897, 1162687, 1395263, 1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559,
      5999471, 7199369
    };

    private static readonly object lockObj = new object();
    private static int currentIndex = 1024;
    private static RandomNumberGenerator rng;
    private static byte[] data;

    private static bool IsPrime(int candidate)
    {
      if ((candidate & 1) == 0)
        return candidate == 2;
      int num = (int) Math.Sqrt(candidate);
      for (int index = 3; index <= num; index += 2)
      {
        if (candidate % index == 0)
          return false;
      }
      return true;
    }

    public static int GetPrime(int min)
    {
      if (min < 0)
        throw new ArgumentException("Arg_HTCapacityOverflow");
      for (int index = 0; index < primes.Length; ++index)
      {
        int prime = primes[index];
        if (prime >= min)
          return prime;
      }
      for (int candidate = min | 1; candidate < int.MaxValue; candidate += 2)
      {
        if (IsPrime(candidate) && (candidate - 1) % 101 != 0)
          return candidate;
      }
      return min;
    }

    public static int ExpandPrime(int oldSize)
    {
      int min = 2 * oldSize;
      return (uint) min > 2146435069U && 2146435069 > oldSize ? 2146435069 : GetPrime(min);
    }

    public static bool IsWellKnownEqualityComparer(object comparer) => comparer == null || comparer == EqualityComparer<string>.Default || comparer is IWellKnownStringEqualityComparer;

    public static IEqualityComparer GetRandomizedEqualityComparer(object comparer)
    {
      if (comparer == null)
        return new RandomizedObjectEqualityComparer();
      if (comparer == EqualityComparer<string>.Default)
        return new RandomizedStringEqualityComparer();
      return comparer is IWellKnownStringEqualityComparer equalityComparer ? equalityComparer.GetRandomizedEqualityComparer() : null;
    }

    public static object GetEqualityComparerForSerialization(object comparer)
    {
      if (comparer == null)
        return null;
      return comparer is IWellKnownStringEqualityComparer equalityComparer ? equalityComparer.GetEqualityComparerForSerialization() : comparer;
    }

    internal static long GetEntropy()
    {
      lock (lockObj)
      {
        if (currentIndex == 1024)
        {
          if (rng == null)
          {
            rng = RandomNumberGenerator.Create();
            data = new byte[1024];
          }
          rng.GetBytes(data);
          currentIndex = 0;
        }
        long int64 = BitConverter.ToInt64(data, currentIndex);
        currentIndex += 8;
        return int64;
      }
    }
  }

  internal sealed class SR
  {
    private static SR loader;
    private readonly ResourceManager resources;

    private SR() => resources = new ResourceManager("System.Core", GetType().Assembly);

    private static SR GetLoader()
    {
      if (loader == null)
      {
        SR sr = new SR();
        Interlocked.CompareExchange(ref loader, sr, null);
      }

      return loader;
    }

    private static CultureInfo Culture => null;

    public static string GetString(string name) => GetLoader()?.resources.GetString(name, Culture);
  }

  [Serializable]
  internal class HashSetEqualityComparer<T> : IEqualityComparer<HashSet<T>>
  {
    private IEqualityComparer<T> m_comparer;

    public HashSetEqualityComparer() => m_comparer = EqualityComparer<T>.Default;

    public HashSetEqualityComparer(IEqualityComparer<T> comparer)
    {
      if (comparer == null)
        m_comparer = EqualityComparer<T>.Default;
      else
        m_comparer = comparer;
    }

    public bool Equals(HashSet<T> x, HashSet<T> y) => HashSet<T>.HashSetEquals(x, y, m_comparer);

    public int GetHashCode(HashSet<T> obj)
    {
      int num = 0;
      if (obj != null)
      {
        foreach (T obj1 in obj)
          num ^= m_comparer.GetHashCode(obj1) & int.MaxValue;
      }
      return num;
    }

    public override bool Equals(object obj) => obj is HashSetEqualityComparer<T> equalityComparer && m_comparer == equalityComparer.m_comparer;

    public override int GetHashCode() => m_comparer.GetHashCode();
  }

  internal class BitHelper
  {
    private readonly int m_length;
    [SecurityCritical]
    private readonly unsafe int* m_arrayPtr;
    private readonly int[] m_array;
    private readonly bool useStackAlloc;

    [SecurityCritical]
    internal unsafe BitHelper(int* bitArrayPtr, int length)
    {
      m_arrayPtr = bitArrayPtr;
      m_length = length;
      useStackAlloc = true;
    }

    internal BitHelper(int[] bitArray, int length)
    {
      m_array = bitArray;
      m_length = length;
    }

    [SecuritySafeCritical]
    internal unsafe void MarkBit(int bitPosition)
    {
      if (useStackAlloc)
      {
        int num = bitPosition / 32;
        if (num >= m_length || num < 0)
          return;
        int* numPtr = m_arrayPtr + num;
        *numPtr = *numPtr | 1 << bitPosition % 32;
      }
      else
      {
        int index = bitPosition / 32;
        if (index >= m_length || index < 0)
          return;
        m_array[index] |= 1 << bitPosition % 32;
      }
    }

    [SecuritySafeCritical]
    internal unsafe bool IsMarked(int bitPosition)
    {
      if (useStackAlloc)
      {
        int index = bitPosition / 32;
        return index < m_length && index >= 0 && (uint) (m_arrayPtr[index] & 1 << bitPosition % 32) > 0U;
      }
      int index1 = bitPosition / 32;
      return index1 < m_length && index1 >= 0 && (uint) (m_array[index1] & 1 << bitPosition % 32) > 0U;
    }

    internal static int ToIntArrayLength(int n) => n <= 0 ? 0 : (n - 1) / 32 + 1;
  }

  internal interface IWellKnownStringEqualityComparer
  {
    IEqualityComparer GetRandomizedEqualityComparer();

    IEqualityComparer GetEqualityComparerForSerialization();
  }

  internal sealed class RandomizedObjectEqualityComparer : IEqualityComparer, IWellKnownStringEqualityComparer
  {
    private long _entropy;

    public RandomizedObjectEqualityComparer() => _entropy = HashHelpers.GetEntropy();

    public bool Equals(object x, object y) => x != null ? y != null && x.Equals(y) : y == null;

    [SecuritySafeCritical]
    public int GetHashCode(object obj)
    {
      if (obj == null)
        return 0;
      return obj is string s ? StringExtensions.InternalMarvin32HashString(s, s.Length, _entropy) : obj.GetHashCode();
    }

    public override bool Equals(object obj) => obj is RandomizedObjectEqualityComparer equalityComparer && _entropy == equalityComparer._entropy;

    public override int GetHashCode() => GetType().Name.GetHashCode() ^ (int) (_entropy & int.MaxValue);

    IEqualityComparer IWellKnownStringEqualityComparer.GetRandomizedEqualityComparer() => new RandomizedObjectEqualityComparer();

    IEqualityComparer IWellKnownStringEqualityComparer.GetEqualityComparerForSerialization() => null;
  }

  internal sealed class RandomizedStringEqualityComparer : IEqualityComparer<string>, IEqualityComparer, IWellKnownStringEqualityComparer
  {
    private long _entropy;

    public RandomizedStringEqualityComparer() => _entropy = HashHelpers.GetEntropy();

    public bool Equals(object x, object y)
    {
      if (x == y)
        return true;
      if (x == null || y == null)
        return false;
      if (x is string && y is string)
        return Equals((string) x, (string) y);
      throw new Exception("InvalidArgumentForComparison");
    }

    public bool Equals(string x, string y) => x != null ? y != null && x.Equals(y) : y == null;

    [SecuritySafeCritical]
    public int GetHashCode(string obj) => obj == null ? 0 : StringExtensions.InternalMarvin32HashString(obj, obj.Length, _entropy);

    [SecuritySafeCritical]
    public int GetHashCode(object obj)
    {
      if (obj == null)
        return 0;
      return obj is string s ? StringExtensions.InternalMarvin32HashString(s, s.Length, _entropy) : obj.GetHashCode();
    }

    public override bool Equals(object obj) => obj is RandomizedStringEqualityComparer equalityComparer && _entropy == equalityComparer._entropy;

    public override int GetHashCode() => GetType().Name.GetHashCode() ^ (int) (_entropy & int.MaxValue);

    IEqualityComparer IWellKnownStringEqualityComparer.GetRandomizedEqualityComparer() => new RandomizedStringEqualityComparer();

    IEqualityComparer IWellKnownStringEqualityComparer.GetEqualityComparerForSerialization() => EqualityComparer<string>.Default;
  }

  internal static class StringExtensions
  {
    public static int InternalMarvin32HashString(string s, int strLen, long additionalEntropy)
    {
      var internalMarvin32HashString = typeof(string).GetMethod(
        "InternalMarvin32HashString",
        BindingFlags.Static | BindingFlags.NonPublic);

      if (internalMarvin32HashString == null)
        throw new Exception("Could not find string.InternalMarvin32HashString() method.");

      return (int) internalMarvin32HashString.Invoke(null, new object[] { s, strLen, additionalEntropy });
    }
  }
}