// Needed for NET40

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Theraot.Collections.Specialized;

namespace Theraot.Collections.ThreadSafe
{
    /// <summary>
    /// Represent a thread-safe lock-free hash based dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <remarks>
    /// Consider wrapping this class to implement <see cref="IDictionary{TKey, TValue}" /> or any other desired interface.
    /// </remarks
#if !NETCOREAPP1_0 && NETCOREAPP1_1 && !NETSTANDARD1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_2 && !NETSTANDARD1_3 && !NETSTANDARD1_4 && !NETSTANDARD1_5 && !NETSTANDARD1_6
    [Serializable]
#endif

    public sealed partial class SafeDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private const int _defaultProbing = 1;

        private readonly IEqualityComparer<TKey> _keyComparer;
        private readonly IEqualityComparer<TValue> _valueComparer;
        private Bucket<KeyValuePair<TKey, TValue>> _bucket;

#if !NETCOREAPP1_0 && NETCOREAPP1_1 && !NETSTANDARD1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_2 && !NETSTANDARD1_3 && !NETSTANDARD1_4 && !NETSTANDARD1_5 && !NETSTANDARD1_6
        private readonly KeyCollection<TKey, TValue> _keyCollection;
        private readonly ValueCollection<TKey, TValue> _valueCollection;

                /// <summary>
        /// Initializes a new instance of the <see cref="SafeDictionary{TKey,TValue}" /> class.
        /// </summary>
        /// <param name="comparer">The key comparer.</param>
        /// <param name="initialProbing">The number of steps in linear probing.</param>
        public SafeDictionary(IEqualityComparer<TKey> comparer, int initialProbing)
        {
            _keyComparer = comparer ?? EqualityComparer<TKey>.Default;
            _valueComparer = EqualityComparer<TValue>.Default;
            _bucket = new Bucket<KeyValuePair<TKey, TValue>>();
            _probing = initialProbing;
            _keyCollection = new KeyCollection<TKey, TValue>(this);
            _valueCollection = new ValueCollection<TKey, TValue>(this);
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return _keyCollection;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return _valueCollection;
            }
        }
#else

        [NonSerialized]
        private KeyCollection<TKey, TValue> _keyCollection;

        [NonSerialized]
        private ValueCollection<TKey, TValue> _valueCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeDictionary{TKey,TValue}" /> class.
        /// </summary>
        /// <param name="comparer">The key comparer.</param>
        /// <param name="initialProbing">The number of steps in linear probing.</param>
        public SafeDictionary(IEqualityComparer<TKey> comparer, int initialProbing)
        {
            _keyComparer = comparer ?? EqualityComparer<TKey>.Default;
            _valueComparer = EqualityComparer<TValue>.Default;
            _bucket = new Bucket<KeyValuePair<TKey, TValue>>();
            _probing = initialProbing;
        }

        public ICollection<TKey> Keys
        {
            get
            {
                if (_keyCollection == null)
                {
                    var found = Volatile.Read(ref _keyCollection);
                    if (found == null)
                    {
                        var created = new KeyCollection<TKey, TValue>(this);
                        found = Interlocked.CompareExchange(ref _keyCollection, created, null);
                        if (found == null)
                        {
                            return created;
                        }
                    }
                    return found;
                }
                return _keyCollection;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                if (_valueCollection == null)
                {
                    var found = Volatile.Read(ref _valueCollection);
                    if (found == null)
                    {
                        var created = new ValueCollection<TKey, TValue>(this);
                        found = Interlocked.CompareExchange(ref _valueCollection, created, null);
                        if (found == null)
                        {
                            return created;
                        }
                    }
                    return found;
                }
                return _valueCollection;
            }
        }

#endif

        private int _probing;

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeDictionary{TKey,TValue}" /> class.
        /// </summary>
        public SafeDictionary()
            : this(EqualityComparer<TKey>.Default, _defaultProbing)
        {
            // Empty
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeDictionary{TKey,TValue}" /> class.
        /// </summary>
        /// <param name="initialProbing">The number of steps in linear probing.</param>
        public SafeDictionary(int initialProbing)
            : this(EqualityComparer<TKey>.Default, initialProbing)
        {
            // Empty
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeDictionary{TKey,TValue}" /> class.
        /// </summary>
        /// <param name="comparer">The key comparer.</param>
        public SafeDictionary(IEqualityComparer<TKey> comparer)
            : this(comparer, _defaultProbing)
        {
            // Empty
        }

        public int Count
        {
            get { return _bucket.Count; }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return false; }
        }

        public IEqualityComparer<TKey> KeyComparer
        {
            get { return _keyComparer; }
        }

        public TValue this[TKey key]
        {
            get
            {
                if (TryGetValue(key, out TValue value))
                {
                    return value;
                }
                throw new KeyNotFoundException();
            }

            set { Set(key, value); }
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            AddNew(item.Key, item.Value);
        }

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            AddNew(key, value);
        }

        /// <summary>
        /// Adds the specified key and associated value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentException">An item with the same key has already been added</exception>
        public void AddNew(TKey key, TValue value)
        {
            var insertPair = new KeyValuePair<TKey, TValue>(key, value);
            var hashCode = GetHashCode(key);
            var attempts = 0;
            while (true)
            {
                ExtendProbingIfNeeded(attempts);
                if (_bucket.Insert(hashCode + attempts, insertPair, out KeyValuePair<TKey, TValue> found))
                {
                    return;
                }
                if (_keyComparer.Equals(found.Key, key))
                {
                    throw new ArgumentException("An item with the same key has already been added", nameof(key));
                }
                attempts++;
            }
        }

        /// <summary>
        /// Removes all the elements.
        /// </summary>
        public void Clear()
        {
            Interlocked.Exchange(ref _bucket, _bucket = new Bucket<KeyValuePair<TKey, TValue>>());
        }

        /// <summary>
        /// Removes all the elements.
        /// </summary>
        /// <returns>Returns the removed pairs.</returns>
        public IEnumerable<KeyValuePair<TKey, TValue>> ClearEnumerable()
        {
            return Interlocked.Exchange(ref _bucket, _bucket = new Bucket<KeyValuePair<TKey, TValue>>());
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            var hashCode = GetHashCode(item.Key);
            for (var attempts = 0; attempts < _probing; attempts++)
            {
                if (_bucket.TryGet(hashCode + attempts, out KeyValuePair<TKey, TValue> found))
                {
                    if (_keyComparer.Equals(found.Key, item.Key))
                    {
                        if (_valueComparer.Equals(found.Value, item.Value))
                        {
                            return true;
                        }
                        return false;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether the specified key is contained.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   <c>true</c> if the specified key is contained; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(TKey key)
        {
            var hashCode = GetHashCode(key);
            for (var attempts = 0; attempts < _probing; attempts++)
            {
                if (_bucket.TryGet(hashCode + attempts, out KeyValuePair<TKey, TValue> found))
                {
                    if (_keyComparer.Equals(found.Key, key))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether the specified key is contained.
        /// </summary>
        /// <param name="hashCode">The hash code to look for.</param>
        /// <param name="keyCheck">The key predicate.</param>
        /// <returns>
        ///   <c>true</c> if the specified key is contained; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(int hashCode, Predicate<TKey> keyCheck)
        {
            if (keyCheck == null)
            {
                throw new ArgumentNullException(nameof(keyCheck));
            }
            for (var attempts = 0; attempts < _probing; attempts++)
            {
                if (_bucket.TryGet(hashCode + attempts, out KeyValuePair<TKey, TValue> found))
                {
                    if (GetHashCode(found.Key) == hashCode && keyCheck(found.Key))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether the specified key is contained.
        /// </summary>
        /// <param name="hashCode">The hash code to look for.</param>
        /// <param name="keyCheck">The key predicate.</param>
        /// <param name="valueCheck">The value predicate.</param>
        /// <returns>
        ///   <c>true</c> if the specified key is contained; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(int hashCode, Predicate<TKey> keyCheck, Predicate<TValue> valueCheck)
        {
            if (keyCheck == null)
            {
                throw new ArgumentNullException(nameof(keyCheck));
            }
            if (valueCheck == null)
            {
                throw new ArgumentNullException(nameof(valueCheck));
            }
            for (var attempts = 0; attempts < _probing; attempts++)
            {
                if (_bucket.TryGet(hashCode + attempts, out KeyValuePair<TKey, TValue> found))
                {
                    if (GetHashCode(found.Key) == hashCode && keyCheck(found.Key) && valueCheck(found.Value))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Copies the items to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        /// <exception cref="System.ArgumentNullException">array</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">arrayIndex;Non-negative number is required.</exception>
        /// <exception cref="System.ArgumentException">array;The array can not contain the number of elements.</exception>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _bucket.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns an <see cref="System.Collections.Generic.IEnumerator{T}" /> that allows to iterate through the collection.
        /// </summary>
        /// <returns>
        /// An <see cref="System.Collections.Generic.IEnumerator{T}" /> object that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _bucket.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            if (valueFactory == null)
            {
                throw new ArgumentNullException(nameof(valueFactory));
            }
            var hashCode = GetHashCode(key);
            var attempts = 0;
            while (true)
            {
                ExtendProbingIfNeeded(attempts);
                if (_bucket.TryGetOrInsert(hashCode + attempts, () => new KeyValuePair<TKey, TValue>(key, valueFactory(key)), out KeyValuePair<TKey, TValue> storedPair))
                {
                    return storedPair.Value;
                }
                if (_keyComparer.Equals(storedPair.Key, key))
                {
                    return storedPair.Value;
                }
                attempts++;
            }
        }

        public TValue GetOrAdd(TKey key, TValue value)
        {
            var hashCode = GetHashCode(key);
            var insertPair = new KeyValuePair<TKey, TValue>(key, value);
            var attempts = 0;
            while (true)
            {
                ExtendProbingIfNeeded(attempts);
                if (_bucket.TryGetOrInsert(hashCode + attempts, insertPair, out KeyValuePair<TKey, TValue> storedPair))
                {
                    return storedPair.Value;
                }
                if (_keyComparer.Equals(storedPair.Key, key))
                {
                    return storedPair.Value;
                }
                attempts++;
            }
        }

        /// <summary>
        /// Gets the pairs contained in this object.
        /// </summary>
        /// <returns>The pairs contained in this object</returns>
        public IList<KeyValuePair<TKey, TValue>> GetPairs()
        {
            var result = new List<KeyValuePair<TKey, TValue>>(_bucket.Count);
            result.AddRange(_bucket);
            return result;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            var hashCode = GetHashCode(item.Key);
            for (var attempts = 0; attempts < _probing; attempts++)
            {
                var done = false;
                var result = _bucket.RemoveAt
                    (
                        hashCode + attempts,
                        found =>
                        {
                            if (_keyComparer.Equals(found.Key, item.Key))
                            {
                                done = true;
                                if (_valueComparer.Equals(found.Value, item.Value))
                                {
                                    return true;
                                }
                            }
                            return false;
                        }
                    );
                if (done)
                {
                    return result;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   <c>true</c> if the specified key was removed; otherwise, <c>false</c>.
        /// </returns>
        public bool Remove(TKey key)
        {
            var hashCode = GetHashCode(key);
            for (var attempts = 0; attempts < _probing; attempts++)
            {
                var done = false;
                bool check(KeyValuePair<TKey, TValue> found)
                {
                    if (_keyComparer.Equals(found.Key, key))
                    {
                        done = true;
                        return true;
                    }
                    return false;
                }
                var result = _bucket.RemoveAt
                    (
                        hashCode + attempts,
                        check
                    );
                if (done)
                {
                    return result;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the specified key was removed; otherwise, <c>false</c>.
        /// </returns>
        public bool Remove(TKey key, out TValue value)
        {
            value = default;
            var hashCode = GetHashCode(key);
            for (var attempts = 0; attempts < _probing; attempts++)
            {
                var done = false;
                var previous = default(KeyValuePair<TKey, TValue>);
                bool check(KeyValuePair<TKey, TValue> found)
                {
                    previous = found;
                    if (_keyComparer.Equals(found.Key, key))
                    {
                        done = true;
                        return true;
                    }
                    return false;
                }
                var result = _bucket.RemoveAt
                    (
                        hashCode + attempts,
                        check
                    );
                if (done)
                {
                    value = previous.Value;
                    return result;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes a key by hash code and a key predicate.
        /// </summary>
        /// <param name="hashCode">The hash code to look for.</param>
        /// <param name="keyCheck">The key predicate.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the specified key was removed; otherwise, <c>false</c>.
        /// </returns>
        public bool Remove(int hashCode, Predicate<TKey> keyCheck, out TValue value)
        {
            if (keyCheck == null)
            {
                throw new ArgumentNullException(nameof(keyCheck));
            }
            value = default;
            for (var attempts = 0; attempts < _probing; attempts++)
            {
                var done = false;
                var previous = default(KeyValuePair<TKey, TValue>);
                bool check(KeyValuePair<TKey, TValue> found)
                {
                    previous = found;
                    if (GetHashCode(found.Key) == hashCode && keyCheck(found.Key))
                    {
                        done = true;
                        return true;
                    }
                    return false;
                }
                var result = _bucket.RemoveAt
                    (
                        hashCode + attempts,
                        check
                    );
                if (done)
                {
                    value = previous.Value;
                    return result;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes the specified key if the value predicate passes.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="valueCheck">The value predicate.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the specified key was removed; otherwise, <c>false</c>.
        /// </returns>
        public bool Remove(TKey key, Predicate<TValue> valueCheck, out TValue value)
        {
            if (valueCheck == null)
            {
                throw new ArgumentNullException(nameof(valueCheck));
            }
            value = default;
            var hashCode = GetHashCode(key);
            for (var attempts = 0; attempts < _probing; attempts++)
            {
                var done = false;
                var previous = default(KeyValuePair<TKey, TValue>);
                bool check(KeyValuePair<TKey, TValue> found)
                {
                    previous = found;
                    if (_keyComparer.Equals(found.Key, key))
                    {
                        done = true;
                        if (valueCheck(found.Value))
                        {
                            return true;
                        }
                    }
                    return false;
                }
                var result = _bucket.RemoveAt
                    (
                        hashCode + attempts,
                        check
                    );
                if (done)
                {
                    value = previous.Value;
                    return result;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes a key by hash code, key predicate and value predicate.
        /// </summary>
        /// <param name="hashCode">The hash code to look for.</param>
        /// <param name="keyCheck">The key predicate.</param>
        /// <param name="valueCheck">The value predicate.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the specified key was removed; otherwise, <c>false</c>.
        /// </returns>
        public bool Remove(int hashCode, Predicate<TKey> keyCheck, Predicate<TValue> valueCheck, out TValue value)
        {
            if (keyCheck == null)
            {
                throw new ArgumentNullException(nameof(keyCheck));
            }
            if (valueCheck == null)
            {
                throw new ArgumentNullException(nameof(valueCheck));
            }
            value = default;
            for (var attempts = 0; attempts < _probing; attempts++)
            {
                var done = false;
                var previous = default(KeyValuePair<TKey, TValue>);
                bool check(KeyValuePair<TKey, TValue> found)
                {
                    previous = found;
                    if (GetHashCode(found.Key) == hashCode && keyCheck(found.Key))
                    {
                        done = true;
                        if (valueCheck(found.Value))
                        {
                            return true;
                        }
                    }
                    return false;
                }
                var result = _bucket.RemoveAt
                    (
                        hashCode + attempts,
                        check
                    );
                if (done)
                {
                    value = previous.Value;
                    return result;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes the keys and associated values where the key satisfies the predicate.
        /// </summary>
        /// <param name="keyCheck">The predicate.</param>
        /// <returns>
        /// The number or removed pairs of keys and associated values.
        /// </returns>
        /// <remarks>
        /// It is not guaranteed that all the pairs of keys and associated values that satisfies the predicate will be removed.
        /// </remarks>
        public int RemoveWhereKey(Predicate<TKey> keyCheck)
        {
            if (keyCheck == null)
            {
                throw new ArgumentNullException(nameof(keyCheck));
            }
            var matches = _bucket.Where(pair => keyCheck(pair.Key));
            return matches.Count(pair => Remove(pair.Key));
        }

        /// <summary>
        /// Removes the keys and associated values where the key satisfies the predicate.
        /// </summary>
        /// <param name="keyCheck">The predicate.</param>
        /// <returns>
        /// An <see cref="IEnumerable{TValue}" /> that allows to iterate over the values of the removed pairs.
        /// </returns>
        /// <remarks>
        /// It is not guaranteed that all the pairs of keys and associated values that satisfies the predicate will be removed.
        /// </remarks>
        public IEnumerable<TValue> RemoveWhereKeyEnumerable(Predicate<TKey> keyCheck)
        {
            if (keyCheck == null)
            {
                throw new ArgumentNullException(nameof(keyCheck));
            }
            var matches = _bucket.Where(pair => keyCheck(pair.Key));
            return from pair in matches where Remove(pair.Key) select pair.Value;
        }

        /// <summary>
        /// Removes the keys and associated values where the value satisfies the predicate.
        /// </summary>
        /// <param name="valueCheck">The predicate.</param>
        /// <returns>
        /// The number or removed pairs of keys and associated values.
        /// </returns>
        /// <remarks>
        /// It is not guaranteed that all the pairs of keys and associated values that satisfies the predicate will be removed.
        /// </remarks>
        public int RemoveWhereValue(Predicate<TValue> valueCheck)
        {
            if (valueCheck == null)
            {
                throw new ArgumentNullException(nameof(valueCheck));
            }
            var matches = _bucket.Where(pair => valueCheck(pair.Value));
            return matches.Count(pair => Remove(pair.Key));
        }

        /// <summary>
        /// Removes the keys and associated values where the value satisfies the predicate.
        /// </summary>
        /// <param name="valueCheck">The predicate.</param>
        /// <returns>
        /// An <see cref="IEnumerable{TValue}" /> that allows to iterate over the values of the removed pairs.
        /// </returns>
        /// <remarks>
        /// It is not guaranteed that all the pairs of keys and associated values that satisfies the predicate will be removed.
        /// </remarks>
        public IEnumerable<TValue> RemoveWhereValueEnumerable(Predicate<TValue> valueCheck)
        {
            if (valueCheck == null)
            {
                throw new ArgumentNullException(nameof(valueCheck));
            }
            var matches = _bucket.Where(pair => valueCheck(pair.Value));
            return from pair in matches where Remove(pair.Key) select pair.Value;
        }

        /// <summary>
        /// Sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Set(TKey key, TValue value)
        {
            var hashCode = GetHashCode(key);
            var insertPair = new KeyValuePair<TKey, TValue>(key, value);
            var attempts = 0;
            while (true)
            {
                ExtendProbingIfNeeded(attempts);
                if (_bucket.InsertOrUpdate(hashCode + attempts, insertPair, found => _keyComparer.Equals(found.Key, key), out bool isNew))
                {
                    return;
                }
                attempts++;
            }
        }

        /// <summary>
        /// Sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="isNew">if set to <c>true</c> the item value was set.</param>
        public void Set(TKey key, TValue value, out bool isNew)
        {
            var hashCode = GetHashCode(key);
            var insertPair = new KeyValuePair<TKey, TValue>(key, value);
            var attempts = 0;
            while (true)
            {
                ExtendProbingIfNeeded(attempts);
                if (_bucket.InsertOrUpdate(hashCode + attempts, insertPair, found => _keyComparer.Equals(found.Key, key), out isNew))
                {
                    return;
                }
                attempts++;
            }
        }

        /// <summary>
        /// Attempts to add the specified key and associated value. The value is added if the key is not found.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the specified key and associated value were added; otherwise, <c>false</c>.
        /// </returns>
        public bool TryAdd(TKey key, TValue value)
        {
            var hashCode = GetHashCode(key);
            var insertPair = new KeyValuePair<TKey, TValue>(key, value);
            var attempts = 0;
            while (true)
            {
                ExtendProbingIfNeeded(attempts);
                if (_bucket.Insert(hashCode + attempts, insertPair, out KeyValuePair<TKey, TValue> found))
                {
                    return true;
                }
                if (_keyComparer.Equals(found.Key, key))
                {
                    return false;
                }
                attempts++;
            }
        }

        /// <summary>
        /// Attempts to add the specified key and associated value. The value is added if the key is not found.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="stored">The stored pair independently of success.</param>
        /// <returns>
        ///   <c>true</c> if the specified key and associated value were added; otherwise, <c>false</c>.
        /// </returns>
        public bool TryAdd(TKey key, TValue value, out KeyValuePair<TKey, TValue> stored)
        {
            var hashCode = GetHashCode(key);
            var insertPair = new KeyValuePair<TKey, TValue>(key, value);
            var attempts = 0;
            while (true)
            {
                ExtendProbingIfNeeded(attempts);
                if (_bucket.Insert(hashCode + attempts, insertPair, out stored))
                {
                    stored = insertPair;
                    return true;
                }
                if (_keyComparer.Equals(stored.Key, key))
                {
                    return false;
                }
                attempts++;
            }
        }

        public bool TryGetOrAdd(TKey key, TValue value, out TValue stored)
        {
            var hashCode = GetHashCode(key);
            var insertPair = new KeyValuePair<TKey, TValue>(key, value);
            var attempts = 0;
            while (true)
            {
                ExtendProbingIfNeeded(attempts);
                if (_bucket.TryGetOrInsert(hashCode + attempts, insertPair, out KeyValuePair<TKey, TValue> storedPair))
                {
                    stored = storedPair.Value;
                    return true;
                }
                if (_keyComparer.Equals(storedPair.Key, key))
                {
                    stored = storedPair.Value;
                    return false;
                }
                attempts++;
            }
        }

        public bool TryGetOrAdd(TKey key, Func<TKey, TValue> valueFactory, out TValue stored)
        {
            if (valueFactory == null)
            {
                throw new ArgumentException(nameof(valueFactory));
            }
            var hashCode = GetHashCode(key);
            var attempts = 0;
            while (true)
            {
                ExtendProbingIfNeeded(attempts);
                if (_bucket.TryGetOrInsert(hashCode + attempts, () => new KeyValuePair<TKey, TValue>(key, valueFactory(key)), out KeyValuePair<TKey, TValue> storedPair))
                {
                    stored = storedPair.Value;
                    return true;
                }
                if (_keyComparer.Equals(storedPair.Key, key))
                {
                    stored = storedPair.Value;
                    return false;
                }
                attempts++;
            }
        }

        /// <summary>
        /// Tries to retrieve the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the value was retrieved; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default;
            var hashCode = GetHashCode(key);
            for (var attempts = 0; attempts < _probing; attempts++)
            {
                if (_bucket.TryGet(hashCode + attempts, out KeyValuePair<TKey, TValue> found))
                {
                    if (_keyComparer.Equals(found.Key, key))
                    {
                        value = found.Value;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            var hashCode = GetHashCode(key);
            var insertPair = new KeyValuePair<TKey, TValue>(key, newValue);
            for (var attempts = 0; attempts < _probing; attempts++)
            {
                var keyMatch = false;
                ExtendProbingIfNeeded(attempts);
                bool check(KeyValuePair<TKey, TValue> found)
                {
                    keyMatch = _keyComparer.Equals(found.Key, key);
                    return keyMatch && _valueComparer.Equals(found.Value, comparisonValue);
                }
                if (_bucket.Update(hashCode + attempts, insertPair, check))
                {
                    return true;
                }
                if (keyMatch)
                {
                    return false;
                }
            }
            return false;
        }

        public bool TryUpdate(TKey key, TValue newValue, Predicate<TValue> valueCheck)
        {
            if (valueCheck == null)
            {
                throw new ArgumentNullException(nameof(valueCheck));
            }
            var hashCode = GetHashCode(key);
            var insertPair = new KeyValuePair<TKey, TValue>(key, newValue);
            for (var attempts = 0; attempts < _probing; attempts++)
            {
                var keyMatch = false;
                ExtendProbingIfNeeded(attempts);
                bool check(KeyValuePair<TKey, TValue> found)
                {
                    keyMatch = _keyComparer.Equals(found.Key, key);
                    return keyMatch && valueCheck(found.Value);
                }
                if (_bucket.Update(hashCode + attempts, _ => insertPair, check, out bool isEmpty))
                {
                    return true;
                }
                if (keyMatch)
                {
                    return false;
                }
            }
            return false;
        }

        public bool TryUpdate(TKey key, Func<TValue, TValue> newValue)
        {
            if (newValue == null)
            {
                throw new ArgumentNullException(nameof(newValue));
            }
            var hashCode = GetHashCode(key);
            for (var attempts = 0; attempts < _probing; attempts++)
            {
                var keyMatch = false;
                ExtendProbingIfNeeded(attempts);
                bool check(KeyValuePair<TKey, TValue> found)
                {
                    keyMatch = _keyComparer.Equals(found.Key, key);
                    return keyMatch;
                }
                if (_bucket.Update(hashCode + attempts, existing => new KeyValuePair<TKey, TValue>(key, newValue(existing.Value)), check, out bool isEmpty))
                {
                    return true;
                }
                if (keyMatch)
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the values where the key satisfies the predicate.
        /// </summary>
        /// <param name="keyCheck">The predicate.</param>
        /// <returns>
        /// An <see cref="IEnumerable{TValue}" /> that allows to iterate over the values of the matched pairs.
        /// </returns>
        /// <remarks>
        /// It is not guaranteed that all the pairs of keys and associated values that satisfies the predicate will be returned.
        /// </remarks>
        public IEnumerable<TValue> Where(Predicate<TKey> keyCheck)
        {
            if (keyCheck == null)
            {
                throw new ArgumentNullException(nameof(keyCheck));
            }
            var matches = _bucket.Where(pair => keyCheck(pair.Key));
            return matches.Select(pair => pair.Value);
        }

        /// <summary>
        /// Returns the values where the value satisfies the predicate.
        /// </summary>
        /// <param name="valueCheck">The predicate.</param>
        /// <returns>
        /// An <see cref="IEnumerable{TValue}" /> that allows to iterate over the values of the matched pairs.
        /// </returns>
        /// <remarks>
        /// It is not guaranteed that all the pairs of keys and associated values that satisfies the predicate will be returned.
        /// </remarks>
        public IEnumerable<TValue> WhereValue(Predicate<TValue> valueCheck)
        {
            if (valueCheck == null)
            {
                throw new ArgumentNullException(nameof(valueCheck));
            }
            var matches = _bucket.Where(pair => valueCheck(pair.Value));
            return matches.Select(pair => pair.Value);
        }

        /// <summary>
        /// Adds the specified key and associated value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="keyOverwriteCheck">The key predicate to approve overwriting.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentException">An item with the same key has already been added</exception>
        internal void AddNew(TKey key, Predicate<TKey> keyOverwriteCheck, TValue value)
        {
#if FAT
            // NOTICE this method has no null check in the public build as an optimization, this is just to appease the dragons
            if (keyOverwriteCheck == null)
            {
                throw new ArgumentNullException(nameof(keyOverwriteCheck));
            }
#endif
            var hashCode = GetHashCode(key);
            var insertPair = new KeyValuePair<TKey, TValue>(key, value);
            var attempts = 0;
            while (true)
            {
                ExtendProbingIfNeeded(attempts);
                bool check(KeyValuePair<TKey, TValue> found)
                {
                    if (_keyComparer.Equals(found.Key, key))
                    {
                        // This is the item that has been stored with the key
                        // Throw to abort overwrite
                        throw CreateKeyArgumentException(null); // This exception will buble up to the context where "key" is an argument.
                    }
                    // This is not the key, overwrite?
                    return keyOverwriteCheck(found.Key);
                }
                // No try-catch - let the exception go.
                // InsertOrUpdate will add if no item is found, otherwise it calls check
                _bucket.InsertOrUpdate(hashCode + attempts, insertPair, check, out bool isNew);
                if (isNew)
                {
                    // It added a new item
                    return;
                }
                attempts++;
            }
        }

        /// <summary>
        /// Sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="keyOverwriteCheck">The key predicate to approve overwriting.</param>
        /// <param name="value">The value.</param>
        internal void Set(TKey key, Predicate<TKey> keyOverwriteCheck, TValue value)
        {
#if FAT
            // NOTICE this method has no null check in the public build as an optimization, this is just to appease the dragons
            if (keyOverwriteCheck == null)
            {
                throw new ArgumentNullException(nameof(keyOverwriteCheck));
            }
#endif
            var hashCode = GetHashCode(key);
            var insertPair = new KeyValuePair<TKey, TValue>(key, value);
            var attempts = 0;
            while (true)
            {
                ExtendProbingIfNeeded(attempts);
                bool check(KeyValuePair<TKey, TValue> found) => _keyComparer.Equals(found.Key, key) || keyOverwriteCheck(found.Key);
                if (_bucket.InsertOrUpdate(hashCode + attempts, insertPair, check, out bool isNew))
                {
                    return;
                }
                attempts++;
            }
        }

        /// <summary>
        /// Sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="keyOverwriteCheck">The key predicate to approve overwriting.</param>
        /// <param name="value">The value.</param>
        /// <param name="isNew">if set to <c>true</c> the item value was set.</param>
        internal void Set(TKey key, Predicate<TKey> keyOverwriteCheck, TValue value, out bool isNew)
        {
#if FAT
            // NOTICE this method has no null check in the public build as an optimization, this is just to appease the dragons
            if (keyOverwriteCheck == null)
            {
                throw new ArgumentNullException(nameof(keyOverwriteCheck));
            }
#endif
            var hashCode = GetHashCode(key);
            var insertPair = new KeyValuePair<TKey, TValue>(key, value);
            var attempts = 0;
            while (true)
            {
                ExtendProbingIfNeeded(attempts);
                bool check(KeyValuePair<TKey, TValue> found) => _keyComparer.Equals(found.Key, key) || keyOverwriteCheck(found.Key);
                if (_bucket.InsertOrUpdate(hashCode + attempts, insertPair, check, out isNew))
                {
                    return;
                }
                attempts++;
            }
        }

        /// <summary>
        /// Attempts to add the specified key and associated value. The value is added if the key is not found.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="keyOverwriteCheck">The key predicate to approve overwriting.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the specified key and associated value were added; otherwise, <c>false</c>.
        /// </returns>
        internal bool TryAdd(TKey key, Predicate<TKey> keyOverwriteCheck, TValue value)
        {
#if FAT
            // NOTICE this method has no null check in the public build as an optimization, this is just to appease the dragons
            if (keyOverwriteCheck == null)
            {
                throw new ArgumentNullException(nameof(keyOverwriteCheck));
            }
#endif
            var hashCode = GetHashCode(key);
            var insertPair = new KeyValuePair<TKey, TValue>(key, value);
            var attempts = 0;
            while (true)
            {
                ExtendProbingIfNeeded(attempts);
                bool check(KeyValuePair<TKey, TValue> found)
                {
                    if (_keyComparer.Equals(found.Key, key))
                    {
                        // This is the item that has been stored with the key
                        // Throw to abort overwrite
                        throw CreateKeyArgumentException(null); // This exception will buble up to the context where "key" is an argument.
                    }
                    // This is not the key, overwrite?
                    return keyOverwriteCheck(found.Key);
                }
                try
                {
                    // InsertOrUpdate will add if no item is found, otherwise it calls check
                    _bucket.InsertOrUpdate(hashCode + attempts, insertPair, check, out bool isNew);
                    if (isNew)
                    {
                        // It added a new item
                        return true;
                    }
                }
                catch (ArgumentException)
                {
                    // An item with the same key has already been added
                    return false;
                }
                attempts++;
            }
        }

        internal bool TryGetOrAdd(TKey key, Predicate<TKey> keyOverwriteCheck, TValue value, out TValue stored)
        {
#if FAT
            // NOTICE this method has no null check in the public build as an optimization, this is just to appease the dragons
            if (keyOverwriteCheck == null)
            {
                throw new ArgumentNullException(nameof(keyOverwriteCheck));
            }
#endif
            var hashCode = GetHashCode(key);
            var insertPair = new KeyValuePair<TKey, TValue>(key, value);
            var attempts = 0;
            while (true)
            {
                ExtendProbingIfNeeded(attempts);
                bool check(KeyValuePair<TKey, TValue> found)
                {
                    if (_keyComparer.Equals(found.Key, key))
                    {
                        // This is the item that has been stored with the key
                        value = found.Value;
                        // Throw to abort overwrite
                        throw CreateKeyArgumentException(null); // This exception will buble up to the context where "key" is an argument.
                    }
                    // This is not the key, overwrite?
                    return keyOverwriteCheck(found.Key);
                }
                try
                {
                    // InsertOrUpdate will add if no item is found, otherwise it calls check
                    _bucket.InsertOrUpdate(hashCode + attempts, insertPair, check, out bool isNew);
                    if (isNew)
                    {
                        // It added a new item
                        stored = value;
                        return true;
                    }
                }
                catch (ArgumentException)
                {
                    // An item with the same key has already been added
                    // Return it
                    stored = value;
                    return false;
                }
                attempts++;
            }
        }

        private void ExtendProbingIfNeeded(int attempts)
        {
            var diff = 1 + attempts - _probing;
            if (diff > 0)
            {
                Interlocked.Add(ref _probing, diff);
            }
        }

        private int GetHashCode(TKey key)
        {
            var hashCode = _keyComparer.GetHashCode(key);
            if (hashCode < 0)
            {
                hashCode = -hashCode;
            }
            // unchecked(-int.MinValue == int.MinValue) == true
            if (hashCode < 0)
            {
                hashCode = 0;
            }
            return hashCode;
        }
    }

    public sealed partial class SafeDictionary<TKey, TValue>
    {
        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (addValueFactory == null)
            {
                throw new ArgumentNullException(nameof(addValueFactory));
            }
            if (updateValueFactory == null)
            {
                throw new ArgumentNullException(nameof(updateValueFactory));
            }
            var hashCode = GetHashCode(key);
            var attempts = 0;
            var insertPair = default(KeyValuePair<TKey, TValue>);
            var updatePair = default(KeyValuePair<TKey, TValue>);
            while (true)
            {
                ExtendProbingIfNeeded(attempts);
                KeyValuePair<TKey, TValue> itemFactory() => insertPair = new KeyValuePair<TKey, TValue>(key, addValueFactory(key));
                KeyValuePair<TKey, TValue> itemUpdateFactory(KeyValuePair<TKey, TValue> found) => updatePair = new KeyValuePair<TKey, TValue>(key, updateValueFactory(found.Key, found.Value));
                bool check(KeyValuePair<TKey, TValue> found) => _keyComparer.Equals(key, found.Key);
                var result = _bucket.InsertOrUpdate
                    (
                        hashCode + attempts,
                        itemFactory,
                        itemUpdateFactory,
                        check,
                        out bool isNew
                    );
                if (result)
                {
                    return isNew ? insertPair.Value : updatePair.Value;
                }
                attempts++;
            }
        }

        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (updateValueFactory == null)
            {
                throw new ArgumentNullException(nameof(updateValueFactory));
            }
            var hashCode = GetHashCode(key);
            var attempts = 0;
            var insertPair = new KeyValuePair<TKey, TValue>(key, addValue);
            var updatePair = default(KeyValuePair<TKey, TValue>);
            while (true)
            {
                ExtendProbingIfNeeded(attempts);
                KeyValuePair<TKey, TValue> updateFactory(KeyValuePair<TKey, TValue> found) => updatePair = new KeyValuePair<TKey, TValue>(key, updateValueFactory(found.Key, found.Value));
                bool check(KeyValuePair<TKey, TValue> found) => _keyComparer.Equals(key, found.Key);
                var result = _bucket.InsertOrUpdate
                    (
                        hashCode + attempts,
                        insertPair,
                        updateFactory,
                        check,
                        out bool isNew
                    );
                if (result)
                {
                    return isNew ? insertPair.Value : updatePair.Value;
                }
                attempts++;
            }
        }

        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory, out bool isNew)
        {
            if (addValueFactory == null)
            {
                throw new ArgumentNullException(nameof(addValueFactory));
            }
            if (updateValueFactory == null)
            {
                throw new ArgumentNullException(nameof(updateValueFactory));
            }
            var hashCode = GetHashCode(key);
            var attempts = 0;
            var insertPair = default(KeyValuePair<TKey, TValue>);
            var updatePair = default(KeyValuePair<TKey, TValue>);
            while (true)
            {
                ExtendProbingIfNeeded(attempts);
                KeyValuePair<TKey, TValue> valueFactory() => insertPair = new KeyValuePair<TKey, TValue>(key, addValueFactory(key));
                KeyValuePair<TKey, TValue> updateFactory(KeyValuePair<TKey, TValue> found) => updatePair = new KeyValuePair<TKey, TValue>(key, updateValueFactory(found.Key, found.Value));
                bool check(KeyValuePair<TKey, TValue> found) => _keyComparer.Equals(key, found.Key);
                var result = _bucket.InsertOrUpdate
                    (
                        hashCode + attempts,
                        valueFactory,
                        updateFactory,
                        check,
                        out isNew
                    );
                if (result)
                {
                    return isNew ? insertPair.Value : updatePair.Value;
                }
                attempts++;
            }
        }

        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory, out bool isNew)
        {
            if (updateValueFactory == null)
            {
                throw new ArgumentNullException(nameof(updateValueFactory));
            }
            var hashCode = GetHashCode(key);
            var attempts = 0;
            var insertPair = new KeyValuePair<TKey, TValue>(key, addValue);
            var updatePair = default(KeyValuePair<TKey, TValue>);
            while (true)
            {
                ExtendProbingIfNeeded(attempts);
                KeyValuePair<TKey, TValue> updateFactory(KeyValuePair<TKey, TValue> found) => updatePair = new KeyValuePair<TKey, TValue>(key, updateValueFactory(found.Key, found.Value));
                bool check(KeyValuePair<TKey, TValue> found) => _keyComparer.Equals(key, found.Key);
                var result = _bucket.InsertOrUpdate
                    (
                        hashCode + attempts,
                        insertPair,
                        updateFactory,
                        check,
                        out isNew
                    );
                if (result)
                {
                    return isNew ? insertPair.Value : updatePair.Value;
                }
                attempts++;
            }
        }

        /// <summary>
        /// Tries to retrieve the value by hash code and key predicate.
        /// </summary>
        /// <param name="hashCode">The hash code to look for.</param>
        /// <param name="keyCheck">The key predicate.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the value was retrieved; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetValue(int hashCode, Predicate<TKey> keyCheck, out TValue value)
        {
            if (keyCheck == null)
            {
                throw new ArgumentNullException(nameof(keyCheck));
            }
            value = default;
            for (var attempts = 0; attempts < _probing; attempts++)
            {
                if (_bucket.TryGet(hashCode + attempts, out KeyValuePair<TKey, TValue> found))
                {
                    if (GetHashCode(found.Key) == hashCode && keyCheck(found.Key))
                    {
                        value = found.Value;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Attempts to add the specified key and associated value. The value is added if the key is not found.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="keyOverwriteCheck">The key predicate to approve overwriting.</param>
        /// <param name="value">The value.</param>
        /// <param name="stored">The stored pair independently of success.</param>
        /// <returns>
        ///   <c>true</c> if the specified key and associated value were added; otherwise, <c>false</c>.
        /// </returns>
        internal bool TryAdd(TKey key, Predicate<TKey> keyOverwriteCheck, TValue value, out KeyValuePair<TKey, TValue> stored)
        {
#if FAT
            // NOTICE this method has no null check in the public build as an optimization, this is just to appease the dragons
            if (keyOverwriteCheck == null)
            {
                throw new ArgumentNullException(nameof(keyOverwriteCheck));
            }
#endif
            var hashCode = GetHashCode(key);
            var created = new KeyValuePair<TKey, TValue>(key, value);
            var attempts = 0;
            while (true)
            {
                var foundPair = created;
                ExtendProbingIfNeeded(attempts);
                bool check(KeyValuePair<TKey, TValue> found)
                {
                    foundPair = found;
                    if (_keyComparer.Equals(foundPair.Key, key))
                    {
                        // This is the item that has been stored with the key
                        // Throw to abort overwrite
                        throw CreateKeyArgumentException(null); // This exception will buble up to the context where "key" is an argument.
                    }
                    // This is not the key, overwrite?
                    return keyOverwriteCheck(foundPair.Key);
                }
                try
                {
                    // InsertOrUpdate will add if no item is found, otherwise it calls check
                    _bucket.InsertOrUpdate(hashCode + attempts, created, check, out bool isNew);
                    if (isNew)
                    {
                        // It added a new item
                        stored = created;
                        return true;
                    }
                }
                catch (ArgumentException)
                {
                    // An item with the same key has already been added
                    stored = foundPair;
                    return false;
                }
                attempts++;
            }
        }

        internal bool TryGetOrAdd(TKey key, Func<TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory, out TValue stored)
        {
#if FAT
            // NOTICE this method has no null check in the public build as an optimization, this is just to appease the dragons
            if (addValueFactory == null)
            {
                throw new ArgumentNullException(nameof(addValueFactory));
            }
            if (updateValueFactory == null)
            {
                throw new ArgumentNullException(nameof(updateValueFactory));
            }
#endif
            var hashCode = GetHashCode(key);
            var attempts = 0;
            while (true)
            {
                var value = default(TValue);
                ExtendProbingIfNeeded(attempts);
                KeyValuePair<TKey, TValue> itemFactory() => new KeyValuePair<TKey, TValue>(key, value = addValueFactory());
                KeyValuePair<TKey, TValue> itemUpdateFactory(KeyValuePair<TKey, TValue> found)
                {
                    if (_keyComparer.Equals(found.Key, key))
                    {
                        // This is the item that has been stored with the key
                        value = found.Value;
                        // Throw to abort overwrite
                        throw CreateKeyArgumentException(null); // This exception will buble up to the context where "key" is an argument.
                    }
                    value = updateValueFactory(found.Key, found.Value);
                    return new KeyValuePair<TKey, TValue>(key, value);
                }
                try
                {
                    _bucket.InsertOrUpdate(hashCode + attempts, itemFactory, itemUpdateFactory, out bool isNew);
                    if (isNew)
                    {
                        // It added a new item
                        stored = value;
                        return true;
                    }
                }
                catch (ArgumentException)
                {
                    // An item with the same key has already been added
                    // Return it
                    stored = value;
                    return false;
                }
                attempts++;
            }
        }

        private static ArgumentException CreateKeyArgumentException(object key)
        {
            GC.KeepAlive(key);
            return new ArgumentException("An item with the same key has already been added", nameof(key));
        }
    }
}