#if SL5
/*  
 Copyright 2008 The 'A Concurrent Hashtable' development team  
 (http://www.codeplex.com/CH/People/ProjectPeople.aspx)

 This library is licensed under the GNU Library General Public License (LGPL).  You should 
 have received a copy of the license along with the source code.  If not, an online copy
 of the license can be found at http://www.codeplex.com/CH/license.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Linq;

namespace ServiceStack.Text
{
    /// <summary>
    /// Search key structure for <see cref="ConcurrentDictionary{TKey,TValue}"/>
    /// </summary>
    /// <typeparam name="TKey">Type of the key.</typeparam>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    public struct ConcurrentDictionaryKey<TKey, TValue>
    {
        internal TKey _Key;
        internal TValue _Value;
        internal bool _IgnoreValue;

        internal ConcurrentDictionaryKey(TKey key)
        {
            _Key = key;
            _IgnoreValue = true;
            _Value = default(TValue);
        }

        internal ConcurrentDictionaryKey(TKey key, TValue value)
        {
            _Key = key;
            _IgnoreValue = false;
            _Value = value;
        }
    }

    /// <summary>
    /// A Concurrent <see cref="IDictionary{TKey,TValue}"/> implementation.
    /// </summary>
    /// <typeparam name="TKey">Type of the keys.</typeparam>
    /// <typeparam name="TValue">Type of the values.</typeparam>
    /// <remarks>
    /// This class is threadsafe and highly concurrent. This means that multiple threads can do lookup and insert operations
    /// on this dictionary simultaneously. 
    /// It is not guaranteed that collisions will not occur. The dictionary is partitioned in segments. A segment contains
    /// a set of items based on a hash of those items. The more segments there are and the beter the hash, the fewer collisions will occur.
    /// This means that a nearly empty ConcurrentDictionary is not as concurrent as one containing many items. 
    /// </remarks>
    public class ConcurrentDictionary<TKey, TValue>
        : ConcurrentHashtable<KeyValuePair<TKey, TValue>?, ConcurrentDictionaryKey<TKey, TValue>>
            , IDictionary<TKey, TValue>, IDictionary
    {
#region Constructors

        /// <summary>
        /// Constructs a <see cref="ConcurrentDictionary{TKey,TValue}"/> instance using the default <see cref="IEqualityComparer{TKey}"/> to compare keys.
        /// </summary>
        public ConcurrentDictionary()
            : this(EqualityComparer<TKey>.Default)
        { }

        /// <summary>
        /// Constructs a <see cref="ConcurrentDictionary{TKey,TValue}"/> instance using the specified <see cref="IEqualityComparer{TKey}"/> to compare keys.
        /// </summary>
        /// <param name="comparer">The <see cref="IEqualityComparer{TKey}"/> tp compare keys with.</param>
        /// <exception cref="ArgumentNullException"><paramref name="comparer"/> is null.</exception>
        public ConcurrentDictionary(IEqualityComparer<TKey> comparer)
            : base()
        {
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            _Comparer = comparer;

            Initialize();
        }

#endregion

#region Traits

        readonly IEqualityComparer<TKey> _Comparer;

        /// <summary>
        /// Gives the <see cref="IEqualityComparer{TKey}"/> of TKey that is used to compare keys.
        /// </summary>
        public IEqualityComparer<TKey> Comparer { get { return _Comparer; } }

        /// <summary>
        /// Get a hashcode for given storeable item.
        /// </summary>
        /// <param name="item">Reference to the item to get a hash value for.</param>
        /// <returns>The hash value as an <see cref="UInt32"/>.</returns>
        /// <remarks>
        /// The hash returned should be properly randomized hash. The standard GetItemHashCode methods are usually not good enough.
        /// A storeable item and a matching search key should return the same hash code.
        /// So the statement <code>ItemEqualsItem(storeableItem, searchKey) ? GetItemHashCode(storeableItem) == GetItemHashCode(searchKey) : true </code> should always be true;
        /// </remarks>
        internal protected override UInt32 GetItemHashCode(ref KeyValuePair<TKey, TValue>? item)
        { return item.HasValue ? Hasher.Rehash(_Comparer.GetHashCode(item.Value.Key)) : 0; }

        /// <summary>
        /// Get a hashcode for given search key.
        /// </summary>
        /// <param name="key">Reference to the key to get a hash value for.</param>
        /// <returns>The hash value as an <see cref="UInt32"/>.</returns>
        /// <remarks>
        /// The hash returned should be properly randomized hash. The standard GetItemHashCode methods are usually not good enough.
        /// A storeable item and a matching search key should return the same hash code.
        /// So the statement <code>ItemEqualsItem(storeableItem, searchKey) ? GetItemHashCode(storeableItem) == GetItemHashCode(searchKey) : true </code> should always be true;
        /// </remarks>
        internal protected override UInt32 GetKeyHashCode(ref ConcurrentDictionaryKey<TKey, TValue> key)
        { return Hasher.Rehash(_Comparer.GetHashCode(key._Key)); }

        /// <summary>
        /// Compares a storeable item to a search key. Should return true if they match.
        /// </summary>
        /// <param name="item">Reference to the storeable item to compare.</param>
        /// <param name="key">Reference to the search key to compare.</param>
        /// <returns>True if the storeable item and search key match; false otherwise.</returns>
        internal protected override bool ItemEqualsKey(ref KeyValuePair<TKey, TValue>? item, ref ConcurrentDictionaryKey<TKey, TValue> key)
        { return item.HasValue && _Comparer.Equals(item.Value.Key, key._Key) && (key._IgnoreValue || EqualityComparer<TValue>.Default.Equals(item.Value.Value, key._Value)); }

        /// <summary>
        /// Compares two storeable items for equality.
        /// </summary>
        /// <param name="item1">Reference to the first storeable item to compare.</param>
        /// <param name="item2">Reference to the second storeable item to compare.</param>
        /// <returns>True if the two soreable items should be regarded as equal.</returns>
        internal protected override bool ItemEqualsItem(ref KeyValuePair<TKey, TValue>? item1, ref KeyValuePair<TKey, TValue>? item2)
        { return item1.HasValue && item2.HasValue && _Comparer.Equals(item1.Value.Key, item2.Value.Key); }

        /// <summary>
        /// Indicates if a specific item reference contains a valid item.
        /// </summary>
        /// <param name="item">The storeable item reference to check.</param>
        /// <returns>True if the reference doesn't refer to a valid item; false otherwise.</returns>
        /// <remarks>The statement <code>IsEmpty(default(TStoredI))</code> should always be true.</remarks>
        internal protected override bool IsEmpty(ref KeyValuePair<TKey, TValue>? item)
        { return !item.HasValue; }

        protected internal override Type GetKeyType(ref KeyValuePair<TKey, TValue>? item)
        { return !item.HasValue || item.Value.Key == null ? null : item.Value.Key.GetType(); }

#endregion

#region IDictionary<TKey,TValue> Members

        /// <summary>
        /// Adds an element with the provided key and value to the dictionary.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="ArgumentException">An element with the same key already exists in the dictionary.</exception>
        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        { ((ICollection<KeyValuePair<TKey, TValue>>)this).Add(new KeyValuePair<TKey, TValue>(key, value)); }

        /// <summary>
        /// Determines whether the dictionary
        /// contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the dictionary.</param>
        /// <returns>true if the dictionary contains
        /// an element with the key; otherwise, false.</returns>
        public bool ContainsKey(TKey key)
        {
            KeyValuePair<TKey, TValue>? presentItem;
            ConcurrentDictionaryKey<TKey, TValue> searchKey = new ConcurrentDictionaryKey<TKey, TValue>(key);
            return FindItem(ref searchKey, out presentItem);
        }

        /// <summary>
        /// Gets an <see cref="ICollection{TKey}"/>  containing the keys of
        /// the dictionary.           
        /// </summary>
        /// <returns>An <see cref="ICollection{TKey}"/> containing the keys of the dictionary.</returns>
        /// <remarks>This property takes a snapshot of the current keys collection of the dictionary at the moment of invocation.</remarks>
        public ICollection<TKey> Keys
        {
            get
            {
                lock (SyncRoot)
                    return base.Items.Select(kvp => kvp.Value.Key).ToList();
            }
        }

        /// <summary>
        /// Removes the element with the specified key from the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>true if the element is successfully removed; otherwise, false. This method
        /// also returns false if key was not found in the original dictionary.</returns>
        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            KeyValuePair<TKey, TValue>? oldItem;
            ConcurrentDictionaryKey<TKey, TValue> searchKey = new ConcurrentDictionaryKey<TKey, TValue>(key);
            return base.RemoveItem(ref searchKey, out oldItem);
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">
        /// When this method returns, the value associated with the specified key, if
        /// the key is found; otherwise, the default value for the type of the value
        /// parameter. This parameter is passed uninitialized.
        ///</param>
        /// <returns>
        /// true if the dictionary contains an element with the specified key; otherwise, false.
        /// </returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            KeyValuePair<TKey, TValue>? presentItem;
            ConcurrentDictionaryKey<TKey, TValue> searchKey = new ConcurrentDictionaryKey<TKey, TValue>(key);

            var res = FindItem(ref searchKey, out presentItem);

            if (res)
            {
                value = presentItem.Value.Value;
                return true;
            }
            else
            {
                value = default(TValue);
                return false;
            }
        }

        /// <summary>
        /// Gets an <see cref="ICollection{TKey}"/> containing the values in
        ///     the dictionary.
        /// </summary>
        /// <returns>
        /// An <see cref="ICollection{TKey}"/> containing the values in the dictionary.
        /// </returns>
        /// <remarks>This property takes a snapshot of the current keys collection of the dictionary at the moment of invocation.</remarks>
        public ICollection<TValue> Values
        {
            get
            {
                lock (SyncRoot)
                    return base.Items.Select(kvp => kvp.Value.Value).ToList();
            }
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value associated with the specified key. If the specified key is not found, a get operation throws a KeyNotFoundException, and a set operation creates a new element with the specified key.</returns>
        /// <remarks>
        /// When working with multiple threads, that can each potentialy remove the searched for item, a <see cref="KeyNotFoundException"/> can always be expected.
        /// </remarks>
        public TValue this[TKey key]
        {
            get
            {
                KeyValuePair<TKey, TValue>? presentItem;
                ConcurrentDictionaryKey<TKey, TValue> searchKey = new ConcurrentDictionaryKey<TKey, TValue>(key);

                if (!FindItem(ref searchKey, out presentItem))
                    throw new KeyNotFoundException("The property is retrieved and key is not found.");
                return presentItem.Value.Value;
            }
            set
            {
                KeyValuePair<TKey, TValue>? newItem = new KeyValuePair<TKey, TValue>(key, value);
                KeyValuePair<TKey, TValue>? presentItem;
                InsertItem(ref newItem, out presentItem);
            }
        }

#endregion

#region IDictionary Members

        void IDictionary.Add(object key, object value)
        { ((IDictionary<TKey, TValue>)this).Add((TKey)key, (TValue)value); }

        void IDictionary.Clear()
        { ((IDictionary<TKey, TValue>)this).Clear(); }

        bool IDictionary.Contains(object key)
        { return ((IDictionary<TKey, TValue>)this).ContainsKey((TKey)key); }

        class DictionaryEnumerator : IDictionaryEnumerator
        {
            public IEnumerator<KeyValuePair<TKey, TValue>> _source;

#region IDictionaryEnumerator Members

            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    var current = _source.Current;
                    return new DictionaryEntry(current.Key, current.Value);
                }
            }

            object IDictionaryEnumerator.Key
            { get { return _source.Current.Key; } }

            object IDictionaryEnumerator.Value
            { get { return _source.Current.Value; } }

#endregion

#region IEnumerator Members

            object IEnumerator.Current
            { get { return ((IDictionaryEnumerator)this).Entry; } }

            bool IEnumerator.MoveNext()
            { return _source.MoveNext(); }

            void IEnumerator.Reset()
            { _source.Reset(); }

#endregion
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        { return new DictionaryEnumerator { _source = ((IDictionary<TKey, TValue>)this).GetEnumerator() }; }

        bool IDictionary.IsFixedSize
        { get { return false; } }

        bool IDictionary.IsReadOnly
        { get { return false; } }

        ICollection IDictionary.Keys
        { get { return (ICollection)((IDictionary<TKey, TValue>)this).Keys; } }

        void IDictionary.Remove(object key)
        { ((IDictionary<TKey, TValue>)this).Remove((TKey)key); }

        ICollection IDictionary.Values
        { get { return (ICollection)((IDictionary<TKey, TValue>)this).Values; } }

        object IDictionary.this[object key]
        {
            get { return ((IDictionary<TKey, TValue>)this)[(TKey)key]; }
            set { ((IDictionary<TKey, TValue>)this)[(TKey)key] = (TValue)value; }
        }

#endregion

#region ICollection<KeyValuePair<TKey,TValue>> Members

        /// <summary>
        /// Adds an association to the dictionary.
        /// </summary>
        /// <param name="item">A <see cref="KeyValuePair{TKey,TValue}"/> that represents the association to add.</param>
        /// <exception cref="ArgumentException">An association with an equal key already exists in the dicitonary.</exception>
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            KeyValuePair<TKey, TValue>? newItem = item;
            KeyValuePair<TKey, TValue>? presentItem;

            if (GetOldestItem(ref newItem, out presentItem))
                throw new ArgumentException("An element with the same key already exists.");
        }

        /// <summary>
        /// Removes all items from the dictionary.
        /// </summary>
        /// <remarks>WHen working with multiple threads, that each can add items to this dictionary, it is not guaranteed that the dictionary will be empty when this method returns.</remarks>
        public new void Clear()
        { base.Clear(); }

        /// <summary>
        /// Determines whether the specified association exists in the dictionary.
        /// </summary>
        /// <param name="item">The key-value association to search fo in the dicionary.</param>
        /// <returns>True if item is found in the dictionary; otherwise, false.</returns>
        /// <remarks>
        /// This method compares both key and value. It uses the default equality comparer to compare values.
        /// </remarks>
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            KeyValuePair<TKey, TValue>? presentItem;
            ConcurrentDictionaryKey<TKey, TValue> searchKey = new ConcurrentDictionaryKey<TKey, TValue>(item.Key, item.Value);

            return
                FindItem(ref searchKey, out presentItem);
        }

        /// <summary>
        /// Copies all associations of the dictionary to an
        ///    <see cref="System.Array"/>, starting at a particular <see cref="System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="System.Array"/> that is the destination of the associations
        ///     copied from <see cref="ConcurrentDictionaryKey{TKey,TValue}"/>. The <see cref="System.Array"/> must
        ///     have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="ArgumentNullException"><paramref name="array"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
        /// <exception cref="ArgumentException"><paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.</exception>
        /// <exception cref="ArgumentException">The number of associations to be copied
        /// is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination
        /// <paramref name="array"/>.</exception>
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            lock (SyncRoot)
                Items.Select(nkvp => nkvp.Value).ToList().CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ConcurrentDictionaryKey{TKey,TValue}"/>.
        /// </summary>
        public new int Count
        { get { return base.Count; } }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ConcurrentDictionaryKey{TKey,TValue}"/> is read-only, which is always false.
        /// </summary>
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        { get { return false; } }

        /// <summary>
        /// Removes the specified association from the <see cref="ConcurrentDictionaryKey{TKey,TValue}"/>, comparing both key and value.
        /// </summary>
        /// <param name="item">A <see cref="KeyValuePair{TKey,TValue}"/> representing the association to remove.</param>
        /// <returns>true if the association was successfully removed from the <see cref="ConcurrentDictionaryKey{TKey,TValue}"/>;
        /// otherwise, false. This method also returns false if the association is not found in
        /// the original <see cref="ConcurrentDictionaryKey{TKey,TValue}"/>.
        ///</returns>
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            KeyValuePair<TKey, TValue>? oldItem;
            ConcurrentDictionaryKey<TKey, TValue> searchKey = new ConcurrentDictionaryKey<TKey, TValue>(item.Key, item.Value);
            return base.RemoveItem(ref searchKey, out oldItem);
        }

#endregion

#region ICollection Members

        void ICollection.CopyTo(Array array, int index)
        { ((ICollection<KeyValuePair<TKey, TValue>>)this).CopyTo((KeyValuePair<TKey, TValue>[])array, index); }

        int ICollection.Count
        { get { return ((ICollection<KeyValuePair<TKey, TValue>>)this).Count; } }

        bool ICollection.IsSynchronized
        { get { return true; } }

        object ICollection.SyncRoot
        { get { return this; } }

#endregion

#region IEnumerable<KeyValuePair<TKey,TValue>> Members

        /// <summary>
        /// Returns an enumerator that iterates through all associations in the <see cref="ConcurrentDictionaryKey{TKey,TValue}"/> at the moment of invocation.
        /// </summary>
        /// <returns>A <see cref="IEnumerator{T}"/> that can be used to iterate through the associations.</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lock (SyncRoot)
                return Items.Select(nkvp => nkvp.Value).ToList().GetEnumerator();
        }

#endregion

#region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through all associations in the <see cref="ConcurrentDictionaryKey{TKey,TValue}"/> at the moment of invocation.
        /// </summary>
        /// <returns>A <see cref="System.Collections.IEnumerator"/> that can be used to iterate through the associations.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        { return GetEnumerator(); }

#endregion

        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (null == addValueFactory)
                throw new ArgumentNullException("addValueFactory");

            if (null == updateValueFactory)
                throw new ArgumentNullException("updateValueFactory");

            var searchKey = new ConcurrentDictionaryKey<TKey, TValue>(key);
            KeyValuePair<TKey, TValue>? latestItem;

            while (true)
                if (this.FindItem(ref searchKey, out latestItem))
                {
                    TValue storedValue = latestItem.Value.Value;
                    TValue newValue = updateValueFactory(key, storedValue);

                    if (TryUpdate(key, newValue, storedValue))
                        return newValue;
                }
                else
                    return AddOrUpdate(key, addValueFactory(key), updateValueFactory);
        }

        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (null == updateValueFactory)
                throw new ArgumentNullException("updateValueFactory");

            KeyValuePair<TKey, TValue>? latestItem;
            KeyValuePair<TKey, TValue>? addItem = new KeyValuePair<TKey, TValue>(key, addValue);

            while (true)
                if (this.GetOldestItem(ref addItem, out latestItem))
                {
                    TValue storedValue = latestItem.Value.Value;
                    TValue newValue = updateValueFactory(key, storedValue);

                    if (TryUpdate(key, newValue, storedValue))
                        return newValue;
                }
                else
                    return latestItem.Value.Value;
        }

        public bool TryAdd(TKey key, TValue value)
        {
            KeyValuePair<TKey, TValue>? addKey = new KeyValuePair<TKey, TValue>(key, value);
            KeyValuePair<TKey, TValue>? oldItem;

            return !this.GetOldestItem(ref addKey, out oldItem);
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            var searchKey = new ConcurrentDictionaryKey<TKey, TValue>(key);
            KeyValuePair<TKey, TValue>? oldItem;

            var res = base.RemoveItem(ref searchKey, out oldItem);

            value = res ? oldItem.Value.Value : default(TValue);

            return res;
        }

        public bool TryUpdate(
            TKey key,
            TValue newValue,
            TValue comparisonValue
        )
        {
            var searchKey = new ConcurrentDictionaryKey<TKey, TValue>(key);
            KeyValuePair<TKey, TValue>? newItem = new KeyValuePair<TKey, TValue>(key, newValue);
            KeyValuePair<TKey, TValue>? dummy;

            return base.ReplaceItem(ref searchKey, ref newItem, out dummy, item => EqualityComparer<TValue>.Default.Equals(item.Value.Value, comparisonValue));
        }

        public TValue GetOrAdd(
            TKey key,
            TValue value
        )
        {
            KeyValuePair<TKey, TValue>? newItem = new KeyValuePair<TKey, TValue>(key, value);
            KeyValuePair<TKey, TValue>? oldItem;

            return base.GetOldestItem(ref newItem, out oldItem) ? oldItem.Value.Value : value;
        }

        public TValue GetOrAdd(
            TKey key,
            Func<TKey, TValue> valueFactory
        )
        {
            if (null == valueFactory)
                throw new ArgumentNullException("valueFactory");

            var searchKey = new ConcurrentDictionaryKey<TKey, TValue>(key);
            KeyValuePair<TKey, TValue>? oldItem;

            if (base.FindItem(ref searchKey, out oldItem))
                return oldItem.Value.Value;

            KeyValuePair<TKey, TValue>? newItem = new KeyValuePair<TKey, TValue>(key, valueFactory(key));

            base.GetOldestItem(ref newItem, out oldItem);

            return oldItem.Value.Value;
        }
    }

    /// <summary>
    /// Base class for concurrent hashtable implementations
    /// </summary>
    /// <typeparam name="TStored">Type of the items stored in the hashtable.</typeparam>
    /// <typeparam name="TSearch">Type of the key to search with.</typeparam>
    public abstract class ConcurrentHashtable<TStored, TSearch>
    {
        /// <summary>
        /// Constructor (protected)
        /// </summary>
        /// <remarks>Use Initialize method after construction.</remarks>
        protected ConcurrentHashtable()
        { }

        /// <summary>
        /// Initialize the newly created ConcurrentHashtable. Invoke in final (sealed) constructor
        /// or Create method.
        /// </summary>
        protected virtual void Initialize()
        {
            var minSegments = MinSegments;
            var segmentAllocatedSpace = MinSegmentAllocatedSpace;

            _CurrentRange = CreateSegmentRange(minSegments, segmentAllocatedSpace);
            _NewRange = _CurrentRange;
            _SwitchPoint = 0;
            _AllocatedSpace = minSegments * segmentAllocatedSpace;
        }

        /// <summary>
        /// Create a segment range
        /// </summary>
        /// <param name="segmentCount">Number of segments in range.</param>
        /// <param name="initialSegmentSize">Number of slots allocated initialy in each segment.</param>
        /// <returns>The created <see cref="Segmentrange{TStored,TSearch}"/> instance.</returns>
        internal virtual Segmentrange<TStored, TSearch> CreateSegmentRange(int segmentCount, int initialSegmentSize)
        { return Segmentrange<TStored, TSearch>.Create(segmentCount, initialSegmentSize); }

        /// <summary>
        /// While adjusting the segmentation, _NewRange will hold a reference to the new range of segments.
        /// when the adjustment is complete this reference will be copied to _CurrentRange.
        /// </summary>
        internal Segmentrange<TStored, TSearch> _NewRange;

        /// <summary>
        /// Will hold the most current reange of segments. When busy adjusting the segmentation, this
        /// field will hold a reference to the old range.
        /// </summary>
        internal Segmentrange<TStored, TSearch> _CurrentRange;

        /// <summary>
        /// While adjusting the segmentation this field will hold a boundary.
        /// Clients accessing items with a key hash value below this boundary (unsigned compared)
        /// will access _NewRange. The others will access _CurrentRange
        /// </summary>
        Int32 _SwitchPoint;

#region Traits

        //Methods used by Segment objects that tell them how to treat stored items and search keys.

        /// <summary>
        /// Get a hashcode for given storeable item.
        /// </summary>
        /// <param name="item">Reference to the item to get a hash value for.</param>
        /// <returns>The hash value as an <see cref="UInt32"/>.</returns>
        /// <remarks>
        /// The hash returned should be properly randomized hash. The standard GetItemHashCode methods are usually not good enough.
        /// A storeable item and a matching search key should return the same hash code.
        /// So the statement <code>ItemEqualsItem(storeableItem, searchKey) ? GetItemHashCode(storeableItem) == GetItemHashCode(searchKey) : true </code> should always be true;
        /// </remarks>
        internal protected abstract UInt32 GetItemHashCode(ref TStored item);

        /// <summary>
        /// Get a hashcode for given search key.
        /// </summary>
        /// <param name="key">Reference to the key to get a hash value for.</param>
        /// <returns>The hash value as an <see cref="UInt32"/>.</returns>
        /// <remarks>
        /// The hash returned should be properly randomized hash. The standard GetItemHashCode methods are usually not good enough.
        /// A storeable item and a matching search key should return the same hash code.
        /// So the statement <code>ItemEqualsItem(storeableItem, searchKey) ? GetItemHashCode(storeableItem) == GetItemHashCode(searchKey) : true </code> should always be true;
        /// </remarks>
        internal protected abstract UInt32 GetKeyHashCode(ref TSearch key);

        /// <summary>
        /// Compares a storeable item to a search key. Should return true if they match.
        /// </summary>
        /// <param name="item">Reference to the storeable item to compare.</param>
        /// <param name="key">Reference to the search key to compare.</param>
        /// <returns>True if the storeable item and search key match; false otherwise.</returns>
        internal protected abstract bool ItemEqualsKey(ref TStored item, ref TSearch key);

        /// <summary>
        /// Compares two storeable items for equality.
        /// </summary>
        /// <param name="item1">Reference to the first storeable item to compare.</param>
        /// <param name="item2">Reference to the second storeable item to compare.</param>
        /// <returns>True if the two soreable items should be regarded as equal.</returns>
        internal protected abstract bool ItemEqualsItem(ref TStored item1, ref TStored item2);

        /// <summary>
        /// Indicates if a specific item reference contains a valid item.
        /// </summary>
        /// <param name="item">The storeable item reference to check.</param>
        /// <returns>True if the reference doesn't refer to a valid item; false otherwise.</returns>
        /// <remarks>The statement <code>IsEmpty(default(TStoredI))</code> should always be true.</remarks>
        internal protected abstract bool IsEmpty(ref TStored item);

        /// <summary>
        /// Returns the type of the key value or object.
        /// </summary>
        /// <param name="item">The stored item to get the type of the key for.</param>
        /// <returns>The actual type of the key or null if it can not be determined.</returns>
        /// <remarks>
        /// Used for diagnostics purposes.
        /// </remarks>
        internal protected virtual Type GetKeyType(ref TStored item)
        { return item == null ? null : item.GetType(); }

#endregion

#region SyncRoot

        readonly object _SyncRoot = new object();

        /// <summary>
        /// Returns an object that serves as a lock for range operations 
        /// </summary>
        /// <remarks>
        /// Clients use this primarily for enumerating over the Tables contents.
        /// Locking doesn't guarantee that the contents don't change, but prevents operations that would
        /// disrupt the enumeration process.
        /// Operations that use this lock:
        /// Count, Clear, DisposeGarbage and AssessSegmentation.
        /// Keeping this lock will prevent the table from re-segmenting.
        /// </remarks>
        protected object SyncRoot { get { return _SyncRoot; } }

#endregion

#region Per segment accessors

        /// <summary>
        /// Gets a segment out of either _NewRange or _CurrentRange based on the hash value.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        internal Segment<TStored, TSearch> GetSegment(UInt32 hash)
        { return ((UInt32)hash < (UInt32)_SwitchPoint ? _NewRange : _CurrentRange).GetSegment(hash); }

        /// <summary>
        /// Gets a LOCKED segment out of either _NewRange or _CurrentRange based on the hash value.
        /// Unlock needs to be called on this segment before it can be used by other clients.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        internal Segment<TStored, TSearch> GetSegmentLockedForWriting(UInt32 hash)
        {
            while (true)
            {
                var segment = GetSegment(hash);

                segment.LockForWriting();

                if (segment.IsAlive)
                    return segment;

                segment.ReleaseForWriting();
            }
        }

        /// <summary>
        /// Gets a LOCKED segment out of either _NewRange or _CurrentRange based on the hash value.
        /// Unlock needs to be called on this segment before it can be used by other clients.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        internal Segment<TStored, TSearch> GetSegmentLockedForReading(UInt32 hash)
        {
            while (true)
            {
                var segment = GetSegment(hash);

                segment.LockForReading();

                if (segment.IsAlive)
                    return segment;

                segment.ReleaseForReading();
            }
        }

        /// <summary>
        /// Finds an item in the table collection that maches the given searchKey
        /// </summary>
        /// <param name="searchKey">The key to the item.</param>
        /// <param name="item">Out reference to a field that will receive the found item.</param>
        /// <returns>A boolean that will be true if an item has been found and false otherwise.</returns>
        protected bool FindItem(ref TSearch searchKey, out TStored item)
        {
            var segment = GetSegmentLockedForReading(this.GetKeyHashCode(ref searchKey));

            try
            {
                return segment.FindItem(ref searchKey, out item, this);
            }
            finally
            { segment.ReleaseForReading(); }
        }

        /// <summary>
        /// Looks for an existing item in the table contents using an alternative copy. If it can be found it will be returned. 
        /// If not then the alternative copy will be added to the table contents and the alternative copy will be returned.
        /// </summary>
        /// <param name="searchKey">A copy to search an already existing instance with</param>
        /// <param name="item">Out reference to receive the found item or the alternative copy</param>
        /// <returns>A boolean that will be true if an existing copy was found and false otherwise.</returns>
        protected virtual bool GetOldestItem(ref TStored searchKey, out TStored item)
        {
            var segment = GetSegmentLockedForWriting(this.GetItemHashCode(ref searchKey));

            try
            {
                return segment.GetOldestItem(ref searchKey, out item, this);
            }
            finally
            { segment.ReleaseForWriting(); }
        }

        /// <summary>
        /// Replaces and existing item
        /// </summary>
        /// <param name="newItem"></param>
        /// <param name="oldItem"></param>
        /// <param name="sanction"></param>
        /// <returns>true is the existing item was successfully replaced.</returns>
        protected bool ReplaceItem(ref TSearch searchKey, ref TStored newItem, out TStored oldItem, Func<TStored, bool> sanction)
        {
            var segment = GetSegmentLockedForWriting(this.GetItemHashCode(ref newItem));

            try
            {
                TStored dummy;

                if (!segment.InsertItem(ref newItem, out oldItem, this))
                {
                    segment.RemoveItem(ref searchKey, out dummy, this);
                    return false;
                }

                if (sanction(oldItem))
                    return true;

                segment.InsertItem(ref oldItem, out dummy, this);
                return false;
            }
            finally
            { segment.ReleaseForWriting(); }
        }


        /// <summary>
        /// Inserts an item in the table contents possibly replacing an existing item.
        /// </summary>
        /// <param name="searchKey">The item to insert in the table</param>
        /// <param name="replacedItem">Out reference to a field that will receive any possibly replaced item.</param>
        /// <returns>A boolean that will be true if an existing copy was found and replaced and false otherwise.</returns>
        protected bool InsertItem(ref TStored searchKey, out TStored replacedItem)
        {
            var segment = GetSegmentLockedForWriting(this.GetItemHashCode(ref searchKey));

            try
            {
                return segment.InsertItem(ref searchKey, out replacedItem, this);
            }
            finally
            { segment.ReleaseForWriting(); }
        }

        /// <summary>
        /// Removes an item from the table contents.
        /// </summary>
        /// <param name="searchKey">The key to find the item with.</param>
        /// <param name="removedItem">Out reference to a field that will receive the found and removed item.</param>
        /// <returns>A boolean that will be rue if an item was found and removed and false otherwise.</returns>
        protected bool RemoveItem(ref TSearch searchKey, out TStored removedItem)
        {
            var segment = GetSegmentLockedForWriting(this.GetKeyHashCode(ref searchKey));

            try
            {
                return segment.RemoveItem(ref searchKey, out removedItem, this);
            }
            finally
            { segment.ReleaseForWriting(); }
        }

#endregion

#region Collection wide accessors

        //These methods require a lock on SyncRoot. They will not block regular per segment accessors (for long)

        /// <summary>
        /// Enumerates all segments in _CurrentRange and locking them before yielding them and resleasing the lock afterwards
        /// The order in which the segments are returned is undefined.
        /// Lock SyncRoot before using this enumerable.
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<Segment<TStored, TSearch>> EnumerateAmorphLockedSegments(bool forReading)
        {
            //if segments are locked a queue will be created to try them later
            //this is so that we can continue with other not locked segments.
            Queue<Segment<TStored, TSearch>> lockedSegmentIxs = null;

            for (int i = 0, end = _CurrentRange.Count; i != end; ++i)
            {
                var segment = _CurrentRange.GetSegmentByIndex(i);

                if (segment.Lock(forReading))
                {
                    try { yield return segment; }
                    finally { segment.Release(forReading); }
                }
                else
                {
                    if (lockedSegmentIxs == null)
                        lockedSegmentIxs = new Queue<Segment<TStored, TSearch>>();

                    lockedSegmentIxs.Enqueue(segment);
                }
            }

            if (lockedSegmentIxs != null)
            {
                var ctr = lockedSegmentIxs.Count;

                while (lockedSegmentIxs.Count != 0)
                {
                    //once we retried them all and we are still not done.. wait a bit.
                    if (ctr-- == 0)
                    {
                        Thread.Sleep(0);
                        ctr = lockedSegmentIxs.Count;
                    }

                    var segment = lockedSegmentIxs.Dequeue();

                    if (segment.Lock(forReading))
                    {
                        try { yield return segment; }
                        finally { segment.Release(forReading); }
                    }
                    else
                        lockedSegmentIxs.Enqueue(segment);
                }
            }
        }

        /// <summary>
        /// Gets an IEnumerable to iterate over all items in all segments.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// A lock should be aquired and held on SyncRoot while this IEnumerable is being used.
        /// The order in which the items are returned is undetermined.
        /// </remarks>
        protected IEnumerable<TStored> Items
        {
            get
            {
                foreach (var segment in EnumerateAmorphLockedSegments(true))
                {
                    int j = -1;
                    TStored foundItem;

                    while ((j = segment.GetNextItem(j, out foundItem, this)) >= 0)
                        yield return foundItem;
                }
            }
        }

        /// <summary>
        /// Removes all items from the collection. 
        /// Aquires a lock on SyncRoot before it does it's thing.
        /// When this method returns and multiple threads have access to this table it
        /// is not guaranteed that the table is actually empty.
        /// </summary>
        protected void Clear()
        {
            lock (SyncRoot)
                foreach (var segment in EnumerateAmorphLockedSegments(false))
                    segment.Clear(this);
        }

        /// <summary>
        /// Returns a count of all items in teh collection. This may not be
        /// aqurate when multiple threads are accessing this table.
        /// Aquires a lock on SyncRoot before it does it's thing.
        /// </summary>
        protected int Count
        {
            get
            {
                lock (SyncRoot)
                {
                    Int32 count = 0;

                    //Don't need to lock a segment to get the count.
                    for (int i = 0, end = _CurrentRange.Count; i != end; ++i)
                        count += _CurrentRange.GetSegmentByIndex(i)._Count;

                    return count;
                }
            }
        }

#endregion

#region Table Maintenance methods

        /// <summary>
        /// Gives the minimum number of segments a hashtable can contain. This should be 1 or more and always a power of 2.
        /// </summary>
        protected virtual Int32 MinSegments { get { return 4; } }

        /// <summary>
        /// Gives the minimum number of allocated item slots per segment. This should be 1 or more, always a power of 2
        /// and less than 1/2 of MeanSegmentAllocatedSpace.
        /// </summary>
        protected virtual Int32 MinSegmentAllocatedSpace { get { return 4; } }

        /// <summary>
        /// Gives the prefered number of allocated item slots per segment. This should be 4 or more and always a power of 2.
        /// </summary>
        protected virtual Int32 MeanSegmentAllocatedSpace { get { return 16; } }

        /// <summary>
        /// Determines if a segmentation adjustment is needed.
        /// </summary>
        /// <returns>True</returns>
        bool SegmentationAdjustmentNeeded()
        {
            var minSegments = MinSegments;
            var meanSegmentAllocatedSpace = MeanSegmentAllocatedSpace;

            var newSpace = Math.Max(_AllocatedSpace, minSegments * meanSegmentAllocatedSpace);
            var meanSpace = _CurrentRange.Count * meanSegmentAllocatedSpace;

            return newSpace > (meanSpace << 1) || newSpace <= (meanSpace >> 1);
        }

        /// <summary>
        /// Bool as int (for interlocked functions) that is true if a Segmentation assesment is pending.
        /// </summary>
        Int32 _AssessSegmentationPending;

        /// <summary>
        /// The total allocated number of item slots. Filled with nonempty items or not.
        /// </summary>
        Int32 _AllocatedSpace;

        /// <summary>
        /// When a segment resizes it uses this method to inform the hashtable of the change in allocated space.
        /// </summary>
        /// <param name="effect"></param>
        internal void EffectTotalAllocatedSpace(Int32 effect)
        {
            //this might be a point of contention. But resizing of segments should happen (far) less often
            //than inserts and removals and therefore this should not pose a problem. 
            Interlocked.Add(ref _AllocatedSpace, effect);

            if (SegmentationAdjustmentNeeded() && Interlocked.Exchange(ref _AssessSegmentationPending, 1) == 0)
                ThreadPool.QueueUserWorkItem(AssessSegmentation);
        }

        /// <summary>
        /// Schedule a call to the AssessSegmentation() method.
        /// </summary>
        protected void ScheduleMaintenance()
        {
            if (Interlocked.Exchange(ref _AssessSegmentationPending, 1) == 0)
                ThreadPool.QueueUserWorkItem(AssessSegmentation);
        }


        /// <summary>
        /// Checks if segmentation needs to be adjusted and if so performs the adjustment.
        /// </summary>
        /// <param name="dummy"></param>
        void AssessSegmentation(object dummy)
        {
            try
            {
                AssessSegmentation();
            }
            finally
            {
                Interlocked.Exchange(ref _AssessSegmentationPending, 0);
                EffectTotalAllocatedSpace(0);
            }
        }

        /// <summary>
        /// This method is called when a re-segmentation is expected to be needed. It checks if it actually is needed and, if so, performs the re-segementation.
        /// </summary>
        protected virtual void AssessSegmentation()
        {
            //in case of a sudden loss of almost all content we
            //may need to do this multiple times.
            while (SegmentationAdjustmentNeeded())
            {
                var meanSegmentAllocatedSpace = MeanSegmentAllocatedSpace;

                int allocatedSpace = _AllocatedSpace;
                int atleastSegments = allocatedSpace / meanSegmentAllocatedSpace;

                Int32 segments = MinSegments;

                while (atleastSegments > segments)
                    segments <<= 1;

                SetSegmentation(segments, meanSegmentAllocatedSpace);
            }
        }

        /// <summary>
        /// Adjusts the segmentation to the new segment count
        /// </summary>
        /// <param name="newSegmentCount">The new number of segments to use. This must be a power of 2.</param>
        /// <param name="segmentSize">The number of item slots to reserve in each segment.</param>
        void SetSegmentation(Int32 newSegmentCount, Int32 segmentSize)
        {
            //Variables to detect a bad hash.
            var totalNewSegmentSize = 0;
            var largestSegmentSize = 0;
            Segment<TStored, TSearch> largestSegment = null;

            lock (SyncRoot)
            {
#if DEBUG
                //<<<<<<<<<<<<<<<<<<<< debug <<<<<<<<<<<<<<<<<<<<<<<<
                //{
                //    int minSize = _CurrentRange.GetSegmentByIndex(0)._List.Length;
                //    int maxSize = minSize;

                //    for (int i = 1, end = _CurrentRange.Count; i < end; ++i)
                //    {
                //        int currentSize = _CurrentRange.GetSegmentByIndex(i)._List.Length;

                //        if (currentSize < minSize)
                //            minSize = currentSize;

                //        if (currentSize > maxSize)
                //            maxSize = currentSize;
                //    }

                //    System.Diagnostics.Debug.Assert(maxSize <= 8 * minSize, "Probably a bad hash");
                //}
                //>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
#endif
                unchecked
                {
                    //create the new range
                    Segmentrange<TStored, TSearch> newRange = CreateSegmentRange(newSegmentCount, segmentSize);

                    //increase total allocated space now. We can do this safely
                    //because at this point _AssessSegmentationPending flag will be true,
                    //preventing an immediate reassesment.
                    Interlocked.Add(ref _AllocatedSpace, newSegmentCount * segmentSize);

                    //lock all new segments
                    //we are going to release these locks while we migrate the items from the
                    //old (current) range to the new range.
                    for (int i = 0, end = newRange.Count; i != end; ++i)
                        newRange.GetSegmentByIndex(i).LockForWriting();

                    //set new (completely locked) range
                    Interlocked.Exchange(ref _NewRange, newRange);


                    //calculate the step sizes for our switch points            
                    var currentSwitchPointStep = (UInt32)(1 << _CurrentRange.Shift);
                    var newSwitchPointStep = (UInt32)(1 << newRange.Shift);

                    //position in new range up from where the new segments are locked
                    var newLockedPoint = (UInt32)0;

                    //At this moment _SwitchPoint should be 0
                    var switchPoint = (UInt32)_SwitchPoint;

                    do
                    {
                        Segment<TStored, TSearch> currentSegment;

                        do
                        {
                            //aquire segment to migrate
                            currentSegment = _CurrentRange.GetSegment(switchPoint);

                            //lock segment 
                            currentSegment.LockForWriting();

                            if (currentSegment.IsAlive)
                                break;

                            currentSegment.ReleaseForWriting();
                        }
                        while (true);

                        //migrate all items in the segment to the new range
                        TStored currentKey;

                        int it = -1;

                        while ((it = currentSegment.GetNextItem(it, out currentKey, this)) >= 0)
                        {
                            var currentKeyHash = this.GetItemHashCode(ref currentKey);

                            //get the new segment. this is already locked.
                            var newSegment = _NewRange.GetSegment(currentKeyHash);

                            TStored dummyKey;
                            newSegment.InsertItem(ref currentKey, out dummyKey, this);
                        }

                        //substract allocated space from allocated space count and trash segment.
                        currentSegment.Bye(this);
                        currentSegment.ReleaseForWriting();

                        if (switchPoint == 0 - currentSwitchPointStep)
                        {
                            //we are about to wrap _SwitchPoint arround.
                            //We have migrated all items from the entire table to the
                            //new range.
                            //replace current with new before advancing, otherwise
                            //we would create a completely blocked table.
                            Interlocked.Exchange(ref _CurrentRange, newRange);
                        }

                        //advance _SwitchPoint
                        switchPoint = (UInt32)Interlocked.Add(ref _SwitchPoint, (Int32)currentSwitchPointStep);

                        //release lock of new segments upto the point where we can still add new items
                        //during this migration.
                        while (true)
                        {
                            var nextNewLockedPoint = newLockedPoint + newSwitchPointStep;

                            if (nextNewLockedPoint > switchPoint || nextNewLockedPoint == 0)
                                break;

                            var newSegment = newRange.GetSegment(newLockedPoint);
                            newSegment.Trim(this);

                            var newSegmentSize = newSegment._Count;

                            totalNewSegmentSize += newSegmentSize;

                            if (newSegmentSize > largestSegmentSize)
                            {
                                largestSegmentSize = newSegmentSize;
                                largestSegment = newSegment;
                            }

                            newSegment.ReleaseForWriting();
                            newLockedPoint = nextNewLockedPoint;
                        }
                    }
                    while (switchPoint != 0);

                    //unlock any remaining new segments
                    while (newLockedPoint != 0)
                    {
                        var newSegment = newRange.GetSegment(newLockedPoint);
                        newSegment.Trim(this);

                        var newSegmentSize = newSegment._Count;

                        totalNewSegmentSize += newSegmentSize;

                        if (newSegmentSize > largestSegmentSize)
                        {
                            largestSegmentSize = newSegmentSize;
                            largestSegment = newSegment;
                        }

                        newSegment.ReleaseForWriting();
                        newLockedPoint += newSwitchPointStep;
                    }
                }
            }
        }

#endregion
    }

    static class Hasher
    {
        public static UInt32 Rehash(Int32 hash)
        {
            unchecked
            {
                Int64 prod = ((Int64)hash ^ 0x00000000691ac2e9L) * 0x00000000a931b975L;
                return (UInt32)(prod ^ (prod >> 32));
            }
        }
    }

    internal class Segment<TStored, TSearch>
    {
#region Construction

        protected Segment()
        { }

        public static Segment<TStored, TSearch> Create(Int32 initialSize)
        {
            var instance = new Segment<TStored, TSearch>();
            instance.Initialize(initialSize);
            return instance;
        }

        /// <summary>
        /// Initialize the segment.
        /// </summary>
        /// <param name="initialSize"></param>
        protected virtual void Initialize(Int32 initialSize)
        { _List = new TStored[Math.Max(4, initialSize)]; }

        /// <summary>
        /// When segment gets introduced into hashtable then its allocated space should be added to the
        /// total allocated space.
        /// Single threaded access or locking is needed
        /// </summary>
        /// <param name="traits"></param>
        public void Welcome(ConcurrentHashtable<TStored, TSearch> traits)
        { traits.EffectTotalAllocatedSpace(_List.Length); }

        /// <summary>
        /// When segment gets removed from hashtable then its allocated space should be subtracted to the
        /// total allocated space.
        /// Single threaded access or locking is needed
        /// </summary>
        /// <param name="traits"></param>
        public void Bye(ConcurrentHashtable<TStored, TSearch> traits)
        {
            traits.EffectTotalAllocatedSpace(-_List.Length);
            _List = null;
        }


#endregion

#region Locking

        TinyReaderWriterLock _Lock;

        internal void LockForWriting()
        { _Lock.LockForWriting(); }

        internal void LockForReading()
        { _Lock.LockForReading(); }

        internal bool Lock(bool forReading)
        {
            return forReading ? _Lock.LockForReading(false) : _Lock.LockForWriting(false);
        }

        internal void ReleaseForWriting()
        { _Lock.ReleaseForWriting(); }

        internal void ReleaseForReading()
        { _Lock.ReleaseForReading(); }

        internal void Release(bool forReading)
        {
            if (forReading)
                _Lock.ReleaseForReading();
            else
                _Lock.ReleaseForWriting();
        }

#endregion

        /// <summary>
        /// Array with 'slots'. Each slot can be filled or empty.
        /// </summary>
        internal TStored[] _List;

        /// <summary>
        /// Boolean value indicating if this segment has not been trashed yet.
        /// </summary>
        internal bool IsAlive { get { return _List != null; } }


#region Item Manipulation methods

        /// <summary>
        /// Inserts an item into a *not empty* spot given by position i. It moves items forward until an empty spot is found.
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="i"></param>
        /// <param name="itemCopy"></param>
        /// <param name="traits"></param>
        private void InsertItemAtIndex(UInt32 mask, UInt32 i, TStored itemCopy, ConcurrentHashtable<TStored, TSearch> traits)
        {
            do
            {
                //swap
                TStored temp = _List[i];
                _List[i] = itemCopy;
                itemCopy = temp;

                i = (i + 1) & mask;
            }
            while (!traits.IsEmpty(ref _List[i]));

            _List[i] = itemCopy;
        }

        /// <summary>
        /// Find item in segment.
        /// </summary>
        /// <param name="key">Reference to the search key to use.</param>
        /// <param name="item">Out reference to store the found item in.</param>
        /// <param name="traits">Object that tells this segment how to treat items and keys.</param>
        /// <returns>True if an item could be found, otherwise false.</returns>
        public bool FindItem(ref TSearch key, out TStored item, ConcurrentHashtable<TStored, TSearch> traits)
        {
            var searchHash = traits.GetKeyHashCode(ref key);
            var mask = (UInt32)(_List.Length - 1);
            var i = searchHash & mask;

            if (!traits.IsEmpty(ref _List[i]))
            {
                var firstHash = traits.GetItemHashCode(ref _List[i]);
                var storedItemHash = firstHash;
                var searchHashDiff = (searchHash - firstHash) & mask;

                do
                {
                    if (storedItemHash == searchHash && traits.ItemEqualsKey(ref _List[i], ref key))
                    {
                        item = _List[i];
                        return true;
                    }

                    i = (i + 1) & mask;

                    if (traits.IsEmpty(ref _List[i]))
                        break;

                    storedItemHash = traits.GetItemHashCode(ref _List[i]);
                }
                while (((storedItemHash - firstHash) & mask) <= searchHashDiff);
            }

            item = default(TStored);
            return false;
        }

        /// <summary>
        /// Find an existing item or, if it can't be found, insert a new item.
        /// </summary>
        /// <param name="key">Reference to the item that will be inserted if an existing item can't be found. It will also be used to search with.</param>
        /// <param name="item">Out reference to store the found item or, if it can not be found, the new inserted item.</param>
        /// <param name="traits">Object that tells this segment how to treat items and keys.</param>
        /// <returns>True if an existing item could be found, otherwise false.</returns>
        public bool GetOldestItem(ref TStored key, out TStored item, ConcurrentHashtable<TStored, TSearch> traits)
        {
            var searchHash = traits.GetItemHashCode(ref key);
            var mask = (UInt32)(_List.Length - 1);
            var i = searchHash & mask;

            if (!traits.IsEmpty(ref _List[i]))
            {
                var firstHash = traits.GetItemHashCode(ref _List[i]);
                var storedItemHash = firstHash;
                var searchHashDiff = (searchHash - firstHash) & mask;

                while (true)
                {
                    if (storedItemHash == searchHash && traits.ItemEqualsItem(ref _List[i], ref key))
                    {
                        item = _List[i];
                        return true;
                    }

                    i = (i + 1) & mask;

                    if (traits.IsEmpty(ref _List[i]))
                        break;

                    storedItemHash = traits.GetItemHashCode(ref _List[i]);

                    if (((storedItemHash - firstHash) & mask) > searchHashDiff)
                    {
                        //insert
                        InsertItemAtIndex(mask, i, key, traits);
                        IncrementCount(traits);
                        item = key;
                        return false;
                    }
                }
            }

            item = _List[i] = key;
            IncrementCount(traits);
            return false;
        }

        /// <summary>
        /// Inserts an item in the segment, possibly replacing an equal existing item.
        /// </summary>
        /// <param name="key">A reference to the item to insert.</param>
        /// <param name="item">An out reference where any replaced item will be written to, if no item was replaced the new item will be written to this reference.</param>
        /// <param name="traits">Object that tells this segment how to treat items and keys.</param>
        /// <returns>True if an existing item could be found and is replaced, otherwise false.</returns>
        public bool InsertItem(ref TStored key, out TStored item, ConcurrentHashtable<TStored, TSearch> traits)
        {
            var searchHash = traits.GetItemHashCode(ref key);
            var mask = (UInt32)(_List.Length - 1);
            var i = searchHash & mask;

            if (!traits.IsEmpty(ref _List[i]))
            {
                var firstHash = traits.GetItemHashCode(ref _List[i]);
                var storedItemHash = firstHash;
                var searchHashDiff = (searchHash - firstHash) & mask;

                while (true)
                {
                    if (storedItemHash == searchHash && traits.ItemEqualsItem(ref _List[i], ref key))
                    {
                        item = _List[i];
                        _List[i] = key;
                        return true;
                    }

                    i = (i + 1) & mask;

                    if (traits.IsEmpty(ref _List[i]))
                        break;

                    storedItemHash = traits.GetItemHashCode(ref _List[i]);

                    if (((storedItemHash - firstHash) & mask) > searchHashDiff)
                    {
                        //insert                   
                        InsertItemAtIndex(mask, i, key, traits);
                        IncrementCount(traits);
                        item = key;
                        return false;
                    }
                }
            }

            item = _List[i] = key;
            IncrementCount(traits);
            return false;
        }

        /// <summary>
        /// Removes an item from the segment.
        /// </summary>
        /// <param name="key">A reference to the key to search with.</param>
        /// <param name="item">An out reference where the removed item will be stored or default(<typeparamref name="TStored"/>) if no item to remove can be found.</param>
        /// <param name="traits">Object that tells this segment how to treat items and keys.</param>
        /// <returns>True if an item could be found and is removed, false otherwise.</returns>
        public bool RemoveItem(ref TSearch key, out TStored item, ConcurrentHashtable<TStored, TSearch> traits)
        {
            var searchHash = traits.GetKeyHashCode(ref key);
            var mask = (UInt32)(_List.Length - 1);
            var i = searchHash & mask;

            if (!traits.IsEmpty(ref _List[i]))
            {
                var firstHash = traits.GetItemHashCode(ref _List[i]);
                var storedItemHash = firstHash;
                var searchHashDiff = (searchHash - firstHash) & mask;

                do
                {
                    if (storedItemHash == searchHash && traits.ItemEqualsKey(ref _List[i], ref key))
                    {
                        item = _List[i];
                        RemoveAtIndex(i, traits);
                        DecrementCount(traits);
                        return true;
                    }

                    i = (i + 1) & mask;

                    if (traits.IsEmpty(ref _List[i]))
                        break;

                    storedItemHash = traits.GetItemHashCode(ref _List[i]);
                }
                while (((storedItemHash - firstHash) & mask) <= searchHashDiff);
            }

            item = default(TStored);
            return false;
        }

        protected void RemoveAtIndex(UInt32 index, ConcurrentHashtable<TStored, TSearch> traits)
        {
            var mask = (UInt32)(_List.Length - 1);
            var i = index;
            var j = (index + 1) & mask;

            while (!traits.IsEmpty(ref _List[j]) && (traits.GetItemHashCode(ref _List[j]) & mask) != j)
            {
                _List[i] = _List[j];

                i = j;
                j = (j + 1) & mask;
            }

            _List[i] = default(TStored);
        }

        public void Clear(ConcurrentHashtable<TStored, TSearch> traits)
        {
            var oldList = _List;
            _List = new TStored[4];

            var effect = _List.Length - oldList.Length;

            if (effect != 0)
                traits.EffectTotalAllocatedSpace(effect);

            _Count = 0;
        }

        /// <summary>
        /// Iterate over items in the segment. 
        /// </summary>
        /// <param name="beyond">Position beyond which the next filled slot will be found and the item in that slot returned. (Starting with -1)</param>
        /// <param name="item">Out reference where the next item will be stored or default if the end of the segment is reached.</param>
        /// <param name="traits">Object that tells this segment how to treat items and keys.</param>
        /// <returns>The index position the next item has been found or -1 otherwise.</returns>
        public int GetNextItem(int beyond, out TStored item, ConcurrentHashtable<TStored, TSearch> traits)
        {
            for (int end = _List.Length; ++beyond < end; )
            {
                if (!traits.IsEmpty(ref _List[beyond]))
                {
                    item = _List[beyond];
                    return beyond;
                }
            }

            item = default(TStored);
            return -1;
        }

#endregion

#region Resizing

        protected virtual void ResizeList(ConcurrentHashtable<TStored, TSearch> traits)
        {
            var oldList = _List;
            var oldListLength = oldList.Length;

            var newListLength = 2;

            while (newListLength < _Count)
                newListLength <<= 1;

            newListLength <<= 1;

            if (newListLength != oldListLength)
            {
                _List = new TStored[newListLength];

                var mask = (UInt32)(newListLength - 1);

                for (int i = 0; i != oldListLength; ++i)
                    if (!traits.IsEmpty(ref oldList[i]))
                    {
                        var searchHash = traits.GetItemHashCode(ref oldList[i]);

                        //j is prefered insertion pos in new list.
                        var j = searchHash & mask;

                        if (traits.IsEmpty(ref _List[j]))
                            _List[j] = oldList[i];
                        else
                        {
                            var firstHash = traits.GetItemHashCode(ref _List[j]);
                            var storedItemHash = firstHash;
                            var searchHashDiff = (searchHash - firstHash) & mask;

                            while (true)
                            {
                                j = (j + 1) & mask;

                                if (traits.IsEmpty(ref _List[j]))
                                {
                                    _List[j] = oldList[i];
                                    break;
                                }

                                storedItemHash = traits.GetItemHashCode(ref _List[j]);

                                if (((storedItemHash - firstHash) & mask) > searchHashDiff)
                                {
                                    InsertItemAtIndex(mask, j, oldList[i], traits);
                                    break;
                                }
                            }
                        }
                    }

                traits.EffectTotalAllocatedSpace(newListLength - oldListLength);
            }
        }

        /// <summary>
        /// Total numer of filled slots in _List.
        /// </summary>
        internal Int32 _Count;

        protected void DecrementCount(ConcurrentHashtable<TStored, TSearch> traits, int amount)
        {
            var oldListLength = _List.Length;
            _Count -= amount;

            if (oldListLength > 4 && _Count < (oldListLength >> 2))
                //Shrink
                ResizeList(traits);
        }

        protected void DecrementCount(ConcurrentHashtable<TStored, TSearch> traits)
        { DecrementCount(traits, 1); }

        private void IncrementCount(ConcurrentHashtable<TStored, TSearch> traits)
        {
            var oldListLength = _List.Length;

            if (++_Count >= (oldListLength - (oldListLength >> 2)))
                //Grow
                ResizeList(traits);
        }

        /// <summary>
        /// Remove any excess allocated space
        /// </summary>
        /// <param name="traits"></param>
        internal void Trim(ConcurrentHashtable<TStored, TSearch> traits)
        { DecrementCount(traits, 0); }

#endregion
    }

    internal class Segmentrange<TStored, TSearch>
    {
        protected Segmentrange()
        { }

        public static Segmentrange<TStored, TSearch> Create(int segmentCount, int initialSegmentSize)
        {
            var instance = new Segmentrange<TStored, TSearch>();
            instance.Initialize(segmentCount, initialSegmentSize);
            return instance;
        }

        protected virtual void Initialize(int segmentCount, int initialSegmentSize)
        {
            _Segments = new Segment<TStored, TSearch>[segmentCount];

            for (int i = 0, end = _Segments.Length; i != end; ++i)
                _Segments[i] = CreateSegment(initialSegmentSize);

            for (int w = segmentCount; w != 0; w <<= 1)
                ++_Shift;
        }

        protected virtual Segment<TStored, TSearch> CreateSegment(int initialSegmentSize)
        { return Segment<TStored, TSearch>.Create(initialSegmentSize); }

        Segment<TStored, TSearch>[] _Segments;
        Int32 _Shift;

        public Segment<TStored, TSearch> GetSegment(UInt32 hash)
        { return _Segments[hash >> _Shift]; }

        public Segment<TStored, TSearch> GetSegmentByIndex(Int32 index)
        { return _Segments[index]; }

        public Int32 Count { get { return _Segments.Length; } }

        public Int32 Shift { get { return _Shift; } }
    }

    public struct TinyReaderWriterLock
    {
        Int32 _Bits;

        const int ReadersInRegionOffset = 0;
        const int ReadersInRegionsMask = 255;
        const int ReadersWaitingOffset = 8;
        const int ReadersWaitingMask = 255;
        const int WriterInRegionOffset = 16;
        const int WritersWaitingOffset = 17;
        const int WritersWaitingMask = 15;
        const int BiasOffset = 21;
        const int BiasMask = 3;

        enum Bias { None = 0, Readers = 1, Writers = 2 };

        struct Data
        {
            public int _ReadersInRegion;
            public int _ReadersWaiting;
            public bool _WriterInRegion;
            public int _WritersWaiting;
            public Bias _Bias;
            public Int32 _OldBits;
        }

        void GetData(out Data data)
        {
            Int32 bits = _Bits;

            data._ReadersInRegion = bits & ReadersInRegionsMask;
            data._ReadersWaiting = (bits >> ReadersWaitingOffset) & ReadersWaitingMask;
            data._WriterInRegion = ((bits >> WriterInRegionOffset) & 1) != 0;
            data._WritersWaiting = (bits >> WritersWaitingOffset) & WritersWaitingMask;
            data._Bias = (Bias)((bits >> BiasOffset) & BiasMask);
            data._OldBits = bits;
        }

        bool SetData(ref Data data)
        {
            Int32 bits;

            bits =
                data._ReadersInRegion
                | (data._ReadersWaiting << ReadersWaitingOffset)
                | ((data._WriterInRegion ? 1 : 0) << WriterInRegionOffset)
                | (data._WritersWaiting << WritersWaitingOffset)
                | ((int)data._Bias << BiasOffset);

            return Interlocked.CompareExchange(ref _Bits, bits, data._OldBits) == data._OldBits;
        }

        /// <summary>
        /// Release a reader lock
        /// </summary>
        public void ReleaseForReading()
        {
            //try shortcut first.
            if (Interlocked.CompareExchange(ref _Bits, 0, 1) == 1)
                return;

            Data data;

            do
            {
                GetData(out data);

                --data._ReadersInRegion;

                if (data._ReadersInRegion == 0 && data._ReadersWaiting == 0)
                    data._Bias = data._WritersWaiting != 0 ? Bias.Writers : Bias.None;
            }
            while (!SetData(ref data));
        }

        /// <summary>
        /// Release a writer lock
        /// </summary>
        public void ReleaseForWriting()
        {
            //try shortcut first.
            if (Interlocked.CompareExchange(ref _Bits, 0, 1 << WriterInRegionOffset) == 1 << WriterInRegionOffset)
                return;

            Data data;

            do
            {
                GetData(out data);

                data._WriterInRegion = false;

                if (data._WritersWaiting == 0)
                    data._Bias = data._ReadersWaiting != 0 ? Bias.Readers : Bias.None;
            }
            while (!SetData(ref data));
        }

        /// <summary>
        /// Aquire a reader lock. Wait until lock is aquired.
        /// </summary>
        public void LockForReading()
        { LockForReading(true); }

        /// <summary>
        /// Aquire a reader lock.
        /// </summary>
        /// <param name="wait">True if to wait until lock aquired, False to return immediately.</param>
        /// <returns>Boolean indicating if lock was successfuly aquired.</returns>
        public bool LockForReading(bool wait)
        {
            //try shortcut first.
            if (Interlocked.CompareExchange(ref _Bits, 1, 0) == 0)
                return true;

            bool waitingRegistered = false;

            try
            {
                while (true)
                {
                    bool retry = false;
                    Data data;
                    GetData(out data);

                    if (data._Bias != Bias.Writers)
                    {
                        if (data._ReadersInRegion < ReadersInRegionsMask && !data._WriterInRegion)
                        {
                            if (waitingRegistered)
                            {
                                data._Bias = Bias.Readers;
                                --data._ReadersWaiting;
                                ++data._ReadersInRegion;
                                if (SetData(ref data))
                                {
                                    waitingRegistered = false;
                                    return true;
                                }
                                else
                                    retry = true;
                            }
                            else if (data._WritersWaiting == 0)
                            {
                                data._Bias = Bias.Readers;
                                ++data._ReadersInRegion;
                                if (SetData(ref data))
                                    return true;
                                else
                                    retry = true;
                            }
                        }

                        //sleep
                    }
                    else
                    {
                        if (!waitingRegistered && data._ReadersWaiting < ReadersWaitingMask && wait)
                        {
                            ++data._ReadersWaiting;
                            if (SetData(ref data))
                            {
                                waitingRegistered = true;
                                //sleep
                            }
                            else
                                retry = true;
                        }

                        //sleep
                    }

                    if (!retry)
                    {
                        if (!wait)
                            return false;

                        System.Threading.Thread.Sleep(0);
                    }
                }
            }
            finally
            {
                if (waitingRegistered)
                {
                    //Thread aborted?
                    Data data;

                    do
                    {
                        GetData(out data);
                        --data._ReadersWaiting;

                        if (data._ReadersInRegion == 0 && data._ReadersWaiting == 0)
                            data._Bias = data._WritersWaiting != 0 ? Bias.Writers : Bias.None;
                    }
                    while (!SetData(ref data));
                }
            }
        }

        /// <summary>
        /// Aquire a writer lock. Wait until lock is aquired.
        /// </summary>
        public void LockForWriting()
        { LockForWriting(true); }

        /// <summary>
        /// Aquire a writer lock.
        /// </summary>
        /// <param name="wait">True if to wait until lock aquired, False to return immediately.</param>
        /// <returns>Boolean indicating if lock was successfuly aquired.</returns>
        public bool LockForWriting(bool wait)
        {
            //try shortcut first.
            if (Interlocked.CompareExchange(ref _Bits, 1 << WriterInRegionOffset, 0) == 0)
                return true;

            bool waitingRegistered = false;

            try
            {
                while (true)
                {
                    bool retry = false;
                    Data data;
                    GetData(out data);

                    if (data._Bias != Bias.Readers)
                    {
                        if (data._ReadersInRegion == 0 && !data._WriterInRegion)
                        {
                            if (waitingRegistered)
                            {
                                data._Bias = Bias.Writers;
                                --data._WritersWaiting;
                                data._WriterInRegion = true;
                                if (SetData(ref data))
                                {
                                    waitingRegistered = false;
                                    return true;
                                }
                                else
                                    retry = true;
                            }
                            else if (data._ReadersWaiting == 0)
                            {
                                data._Bias = Bias.Writers;
                                data._WriterInRegion = true;
                                if (SetData(ref data))
                                    return true;
                                else
                                    retry = true;
                            }
                        }

                        //sleep
                    }
                    else
                    {
                        if (!waitingRegistered && data._WritersWaiting < WritersWaitingMask && wait)
                        {
                            ++data._WritersWaiting;
                            if (SetData(ref data))
                            {
                                waitingRegistered = true;
                                //sleep
                            }
                            else
                                retry = true;
                        }

                        //sleep
                    }

                    if (!retry)
                    {
                        if (!wait)
                            return false;

                        System.Threading.Thread.Sleep(0);
                    }
                }
            }
            finally
            {
                if (waitingRegistered)
                {
                    //Thread aborted?
                    Data data;

                    do
                    {
                        GetData(out data);
                        --data._WritersWaiting;

                        if (!data._WriterInRegion && data._WritersWaiting == 0)
                            data._Bias = data._ReadersWaiting != 0 ? Bias.Readers : Bias.None;
                    }
                    while (!SetData(ref data));
                }
            }
        }
    }
}
#endif
