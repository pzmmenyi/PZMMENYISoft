using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace PZMMENYI.Collections.Concurrent {
    /// <summary>
    /// 代表一个线程安全的键和值的集合。
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class ConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary, IReadOnlyDictionary<TKey, TValue> {
            
        private sealed class Tables {
            internal readonly Node[] _buckets; 
            internal readonly object[] _locks; 
            internal volatile int[] _countPerLock; 

            internal Tables(Node[] buckets, object[] locks, int[] countPerLock) {
                _buckets = buckets;
                _locks = locks;
                _countPerLock = countPerLock;
            }
        }

        private volatile Tables _tables; 
        private readonly IEqualityComparer<TKey> _comparer; 
        private readonly bool _growLockArray; 
        private int _budget; 
            
        private const int DefaultCapacity = 31;
        private const int MaxLockNumber = 1024;
        private static readonly bool s_isValueWriteAtomic = IsValueWriteAtomic();

        private static bool IsValueWriteAtomic() {
                
            Type valueType = typeof(TValue);
            bool isAtomic =
                !valueType.GetTypeInfo().IsValueType ||
                valueType == typeof(bool) ||
                valueType == typeof(char) ||
                valueType == typeof(byte) ||
                valueType == typeof(sbyte) ||
                valueType == typeof(short) ||
                valueType == typeof(ushort) ||
                valueType == typeof(int) ||
                valueType == typeof(uint) ||
                valueType == typeof(float);

            if (!isAtomic && IntPtr.Size == 8) {
                isAtomic =
                    valueType == typeof(double) ||
                    valueType == typeof(long) ||
                    valueType == typeof(ulong);
            }

            return isAtomic;
        }

        public static readonly ConcurrentDictionary<TKey, TValue> Empty = new ConcurrentDictionary<TKey, TValue>();

        /// <summary>
        /// 初始化，具有默认的并发级别和默认的初始容量。
        /// </summary>
        public ConcurrentDictionary() : this(DefaultConcurrencyLevel, DefaultCapacity, true, EqualityComparer<TKey>.Default) { }

        /// <summary>
        /// 初始化，指定的并发级别和初始容量。
        /// </summary>
        /// <param name="concurrencyLevel"></param>
        /// <param name="capacity"></param>
        public ConcurrentDictionary(int concurrencyLevel, int capacity) : this(concurrencyLevel, capacity, false, EqualityComparer<TKey>.Default) { }

        /// <summary>
        /// 初始化，制定复制的数据。
        /// </summary>
        /// <param name="collection"></param>
        public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) : this(collection, EqualityComparer<TKey>.Default) { }

        /// <summary>
        /// 初始化，指定键的比较器。
        /// </summary>
        /// <param name="comparer"></param>
        public ConcurrentDictionary(IEqualityComparer<TKey> comparer) : this(DefaultConcurrencyLevel, DefaultCapacity, true, comparer) { }

        /// <summary>
        /// 初始化，制定复制的数据和键的比较器。
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="comparer"></param>
        public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
            : this(comparer) {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            InitializeFromCollection(collection);
        }

        /// <summary>
        /// 初始化，制定并发级别、复制的集合和键的比较器。
        /// </summary>
        /// <param name="concurrencyLevel"></param>
        /// <param name="collection"></param>
        /// <param name="comparer"></param>
        public ConcurrentDictionary(
            int concurrencyLevel, IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
            : this(concurrencyLevel, DefaultCapacity, false, comparer) {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            InitializeFromCollection(collection);
        }

        private void InitializeFromCollection(IEnumerable<KeyValuePair<TKey, TValue>> collection) {
            TValue dummy;
            foreach (KeyValuePair<TKey, TValue> pair in collection) {
                if (pair.Key == null) ThrowKeyNullException();

                if (!TryAddInternal(pair.Key, _comparer.GetHashCode(pair.Key), pair.Value, false, false, out dummy)) {
                    throw new ArgumentException("源包含复制钥匙。");
                }
            }

            if (_budget == 0) {
                _budget = _tables._buckets.Length / _tables._locks.Length;
            }
        }
        /// <summary>
        /// 初始化，指定并发级别、出事容量和比较器。
        /// </summary>
        /// <param name="concurrencyLevel"></param>
        /// <param name="capacity"></param>
        /// <param name="comparer"></param>
        public ConcurrentDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer)
            : this(concurrencyLevel, capacity, false, comparer) {
        }

        internal ConcurrentDictionary(int concurrencyLevel, int capacity, bool growLockArray, IEqualityComparer<TKey> comparer) {
            if (concurrencyLevel < 1) {
                throw new ArgumentOutOfRangeException(nameof(concurrencyLevel), "并发级别必须是积极的(>=1)。");
            }
            if (capacity < 0) {
                throw new ArgumentOutOfRangeException(nameof(capacity), "容量大小必须不能小于0。");
            }
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));

            if (capacity < concurrencyLevel) {
                capacity = concurrencyLevel;
            }

            object[] locks = new object[concurrencyLevel];
            for (int i = 0; i < locks.Length; i++) {
                locks[i] = new object();
            }

            int[] countPerLock = new int[locks.Length];
            Node[] buckets = new Node[capacity];
            _tables = new Tables(buckets, locks, countPerLock);

            _comparer = comparer;
            _growLockArray = growLockArray;
            _budget = buckets.Length / locks.Length;
        }
        /// <summary>
        /// 试图向<see cref="ConcurrentDictionary{TKey, TValue}"/>添加指定的键和值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryAdd(TKey key, TValue value) {
            if (key == null) ThrowKeyNullException();
            TValue dummy;
            return TryAddInternal(key, _comparer.GetHashCode(key), value, false, true, out dummy);
        }
        /// <summary>
        /// 试图向<see cref="ConcurrentDictionary{TKey, TValue}"/>查询是否包含指定键。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(TKey key) {
            if (key == null) ThrowKeyNullException();

            TValue throwAwayValue;
            return TryGetValue(key, out throwAwayValue);
        }

        /// <summary>
        /// 试图删除指定键、值对。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryRemove(TKey key, out TValue value) {
            if (key == null) ThrowKeyNullException();

            return TryRemoveInternal(key, out value, false, default(TValue));
        }
        private bool TryRemoveInternal(TKey key, out TValue value, bool matchValue, TValue oldValue) {
            int hashcode = _comparer.GetHashCode(key);
            while (true) {
                Tables tables = _tables;

                int bucketNo, lockNo;
                GetBucketAndLockNo(hashcode, out bucketNo, out lockNo, tables._buckets.Length, tables._locks.Length);

                lock (tables._locks[lockNo]) {
                    if (tables != _tables) {
                        continue;
                    }

                    Node prev = null;
                    for (Node curr = tables._buckets[bucketNo]; curr != null; curr = curr._next) {
                        Debug.Assert((prev == null && curr == tables._buckets[bucketNo]) || prev._next == curr);

                        if (hashcode == curr._hashcode && _comparer.Equals(curr._key, key)) {
                            if (matchValue) {
                                bool valuesMatch = EqualityComparer<TValue>.Default.Equals(oldValue, curr._value);
                                if (!valuesMatch) {
                                    value = default(TValue);
                                    return false;
                                }
                            }

                            if (prev == null) {
                                Volatile.Write<Node>(ref tables._buckets[bucketNo], curr._next);
                            }
                            else {
                                prev._next = curr._next;
                            }

                            value = curr._value;
                            tables._countPerLock[lockNo]--;
                            return true;
                        }
                        prev = curr;
                    }
                }

                value = default(TValue);
                return false;
            }
        }
        /// <summary>
        /// 试图通过指定键获得其值。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TValue value) {
            if (key == null) ThrowKeyNullException();
            return TryGetValueInternal(key, _comparer.GetHashCode(key), out value);
        }

        private bool TryGetValueInternal(TKey key, int hashcode, out TValue value) {
            Debug.Assert(_comparer.GetHashCode(key) == hashcode);
                
            Tables tables = _tables;

            int bucketNo = GetBucket(hashcode, tables._buckets.Length);
                
            Node n = Volatile.Read<Node>(ref tables._buckets[bucketNo]);

            while (n != null) {
                if (hashcode == n._hashcode && _comparer.Equals(n._key, key)) {
                    value = n._value;
                    return true;
                }
                n = n._next;
            }

            value = default(TValue);
            return false;
        }

        /// <summary>
        /// 试图通过指定键更新其值。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newValue"></param>
        /// <param name="comparisonValue"></param>
        /// <returns></returns>
        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue) {
            if (key == null) ThrowKeyNullException();
            return TryUpdateInternal(key, _comparer.GetHashCode(key), newValue, comparisonValue);
        }
            
        private bool TryUpdateInternal(TKey key, int hashcode, TValue newValue, TValue comparisonValue) {
            Debug.Assert(_comparer.GetHashCode(key) == hashcode);

            IEqualityComparer<TValue> valueComparer = EqualityComparer<TValue>.Default;

            while (true) {
                int bucketNo;
                int lockNo;

                Tables tables = _tables;
                GetBucketAndLockNo(hashcode, out bucketNo, out lockNo, tables._buckets.Length, tables._locks.Length);

                lock (tables._locks[lockNo]) {
                    if (tables != _tables) {
                        continue;
                    }

                    Node prev = null;
                    for (Node node = tables._buckets[bucketNo]; node != null; node = node._next) {
                        Debug.Assert((prev == null && node == tables._buckets[bucketNo]) || prev._next == node);
                        if (hashcode == node._hashcode && _comparer.Equals(node._key, key)) {
                            if (valueComparer.Equals(node._value, comparisonValue)) {
                                if (s_isValueWriteAtomic) {
                                    node._value = newValue;
                                }
                                else {
                                    Node newNode = new Node(node._key, newValue, hashcode, node._next);

                                    if (prev == null) {
                                        tables._buckets[bucketNo] = newNode;
                                    }
                                    else {
                                        prev._next = newNode;
                                    }
                                }

                                return true;
                            }

                            return false;
                        }

                        prev = node;
                    }

                    //didn't find the key
                    return false;
                }
            }
        }

        /// <summary>
        /// 清除<see cref="ConcurrentDictionary{TKey,TValue}"/>。
        /// </summary>
        public void Clear() {
            int locksAcquired = 0;
            try {
                AcquireAllLocks(ref locksAcquired);

                Tables newTables = new Tables(new Node[DefaultCapacity], _tables._locks, new int[_tables._countPerLock.Length]);
                _tables = newTables;
                _budget = Math.Max(1, newTables._buckets.Length / newTables._locks.Length);
            }
            finally {
                ReleaseLocks(0, locksAcquired);
            }
        }
            
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index) {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), "指定的复制位置必须不小于0。");

            int locksAcquired = 0;
            try {
                AcquireAllLocks(ref locksAcquired);

                int count = 0;

                for (int i = 0; i < _tables._locks.Length && count >= 0; i++) {
                    count += _tables._countPerLock[i];
                }

                if (array.Length - count < index || count < 0) 
                {
                    throw new ArgumentException("指定的复制位置不能越界。");
                }

                CopyToPairs(array, index);
            }
            finally {
                ReleaseLocks(0, locksAcquired);
            }
        }

        /// <summary>
        /// 将<see cref="ConcurrentDictionary{TKey, TValue}"/>转换为数组。
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<TKey, TValue>[] ToArray() {
            int locksAcquired = 0;
            try {
                AcquireAllLocks(ref locksAcquired);
                int count = 0;
                checked {
                    for (int i = 0; i < _tables._locks.Length; i++) {
                        count += _tables._countPerLock[i];
                    }
                }

                if (count == 0) {
                    return new KeyValuePair<TKey, TValue>[count];
                }

                KeyValuePair<TKey, TValue>[] array = new KeyValuePair<TKey, TValue>[count];
                CopyToPairs(array, 0);
                return array;
            }
            finally {
                ReleaseLocks(0, locksAcquired);
            }
        }
            
        private void CopyToPairs(KeyValuePair<TKey, TValue>[] array, int index) {
            Node[] buckets = _tables._buckets;
            for (int i = 0; i < buckets.Length; i++) {
                for (Node current = buckets[i]; current != null; current = current._next) {
                    array[index] = new KeyValuePair<TKey, TValue>(current._key, current._value);
                    index++; 
                }
            }
        }
        private void CopyToEntries(DictionaryEntry[] array, int index) {
            Node[] buckets = _tables._buckets;
            for (int i = 0; i < buckets.Length; i++) {
                for (Node current = buckets[i]; current != null; current = current._next) {
                    array[index] = new DictionaryEntry(current._key, current._value);
                    index++;  
                }
            }
        }
            
        private void CopyToObjects(object[] array, int index) {
            Node[] buckets = _tables._buckets;
            for (int i = 0; i < buckets.Length; i++) {
                for (Node current = buckets[i]; current != null; current = current._next) {
                    array[index] = new KeyValuePair<TKey, TValue>(current._key, current._value);
                    index++;
                }
            }
        }
        /// <summary>
        /// 返回循环访问的枚举数。
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            Node[] buckets = _tables._buckets;

            for (int i = 0; i < buckets.Length; i++) {
                   
                Node current = Volatile.Read<Node>(ref buckets[i]);

                while (current != null) {
                    yield return new KeyValuePair<TKey, TValue>(current._key, current._value);
                    current = current._next;
                }
            }
        }
        private bool TryAddInternal(TKey key, int hashcode, TValue value, bool updateIfExists, bool acquireLock, out TValue resultingValue) {
            Debug.Assert(_comparer.GetHashCode(key) == hashcode);

            while (true) {
                int bucketNo, lockNo;

                Tables tables = _tables;
                GetBucketAndLockNo(hashcode, out bucketNo, out lockNo, tables._buckets.Length, tables._locks.Length);

                bool resizeDesired = false;
                bool lockTaken = false;
                try {
                    if (acquireLock)
                        Monitor.Enter(tables._locks[lockNo], ref lockTaken);
                        
                    if (tables != _tables) {
                        continue;
                    }
                        
                    Node prev = null;
                    for (Node node = tables._buckets[bucketNo]; node != null; node = node._next) {
                        Debug.Assert((prev == null && node == tables._buckets[bucketNo]) || prev._next == node);
                        if (hashcode == node._hashcode && _comparer.Equals(node._key, key)) {
                            if (updateIfExists) {
                                if (s_isValueWriteAtomic) {
                                    node._value = value;
                                }
                                else {
                                    Node newNode = new Node(node._key, value, hashcode, node._next);
                                    if (prev == null) {
                                        tables._buckets[bucketNo] = newNode;
                                    }
                                    else {
                                        prev._next = newNode;
                                    }
                                }
                                resultingValue = value;
                            }
                            else {
                                resultingValue = node._value;
                            }
                            return false;
                        }
                        prev = node;
                    }
                        
                    Volatile.Write<Node>(ref tables._buckets[bucketNo], new Node(key, value, hashcode, tables._buckets[bucketNo]));
                    checked {
                        tables._countPerLock[lockNo]++;
                    }
                        
                    if (tables._countPerLock[lockNo] > _budget) {
                        resizeDesired = true;
                    }
                }
                finally {
                    if (lockTaken)
                        Monitor.Exit(tables._locks[lockNo]);
                }
                    
                if (resizeDesired) {
                    GrowTable(tables);
                }

                resultingValue = value;
                return true;
            }
        }

        /// <summary>
        /// 获取或设置与指定的键相关联的值。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue this[TKey key] {
            get {
                TValue value;
                if (!TryGetValue(key, out value)) {
                    ThrowKeyNotFoundException();
                }
                return value;
            }
            set {
                if (key == null) ThrowKeyNullException();
                TValue dummy;
                TryAddInternal(key, _comparer.GetHashCode(key), value, true, true, out dummy);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowKeyNotFoundException() {
            throw new KeyNotFoundException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowKeyNullException() {
            throw new ArgumentNullException("key");
        }

        /// <summary>
        /// 获得或设置<see cref="ConcurrentDictionary{TKey, TValue}"/>容量大小。
        /// </summary>
        public int Count {
            get {
                int count = 0;

                int acquiredLocks = 0;
                try {
                    AcquireAllLocks(ref acquiredLocks);
                        
                    for (int i = 0; i < _tables._countPerLock.Length; i++) {
                        count += _tables._countPerLock[i];
                    }
                }
                finally {
                    ReleaseLocks(0, acquiredLocks);
                }

                return count;
            }
        }

        /// <summary>
        /// 添加一个键值对，如果存在则返回其值。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valueFactory"></param>
        /// <returns></returns>
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory) {
            if (key == null) ThrowKeyNullException();
            if (valueFactory == null) throw new ArgumentNullException(nameof(valueFactory));

            int hashcode = _comparer.GetHashCode(key);

            TValue resultingValue;
            if (!TryGetValueInternal(key, hashcode, out resultingValue)) {
                TryAddInternal(key, hashcode, valueFactory(key), false, true, out resultingValue);
            }
            return resultingValue;
        }

        /// <summary>
        /// 添加一个键值对，如果存在则返回其值。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valueFactory"></param>
        /// <returns></returns>
        public TValue GetOrAdd(TKey key, TValue value) {
            if (key == null) ThrowKeyNullException();

            int hashcode = _comparer.GetHashCode(key);

            TValue resultingValue;
            if (!TryGetValueInternal(key, hashcode, out resultingValue)) {
                TryAddInternal(key, hashcode, value, false, true, out resultingValue);
            }
            return resultingValue;
        }

        /// <summary>
        /// 添加一个键值对，如果存在则返回其修改值。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valueFactory"></param>
        /// <returns></returns>
        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory) {
            if (key == null) ThrowKeyNullException();
            if (addValueFactory == null) throw new ArgumentNullException(nameof(addValueFactory));
            if (updateValueFactory == null) throw new ArgumentNullException(nameof(updateValueFactory));

            int hashcode = _comparer.GetHashCode(key);

            while (true) {
                TValue oldValue;
                if (TryGetValueInternal(key, hashcode, out oldValue))
                    
                {
                    TValue newValue = updateValueFactory(key, oldValue);
                    if (TryUpdateInternal(key, hashcode, newValue, oldValue)) {
                        return newValue;
                    }
                }
                else 
                {
                    TValue resultingValue;
                    if (TryAddInternal(key, hashcode, addValueFactory(key), false, true, out resultingValue)) {
                        return resultingValue;
                    }
                }
            }
        }
        /// <summary>
        /// 添加一个键值对，如果存在则返回其修改值。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valueFactory"></param>
        /// <returns></returns>
        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory) {
            if (key == null) ThrowKeyNullException();
            if (updateValueFactory == null) throw new ArgumentNullException(nameof(updateValueFactory));

            int hashcode = _comparer.GetHashCode(key);

            while (true) {
                TValue oldValue;
                if (TryGetValueInternal(key, hashcode, out oldValue))
                //key exists, try to update
                {
                    TValue newValue = updateValueFactory(key, oldValue);
                    if (TryUpdateInternal(key, hashcode, newValue, oldValue)) {
                        return newValue;
                    }
                }
                else //try add
                {
                    TValue resultingValue;
                    if (TryAddInternal(key, hashcode, addValue, false, true, out resultingValue)) {
                        return resultingValue;
                    }
                }
            }
        }

        /// <summary>
        /// 得到一个值，判断其是否为空。
        /// </summary>
        public bool IsEmpty {
            get {
                int acquiredLocks = 0;
                try {
                    AcquireAllLocks(ref acquiredLocks);

                    for (int i = 0; i < _tables._countPerLock.Length; i++) {
                        if (_tables._countPerLock[i] != 0) {
                            return false;
                        }
                    }
                }
                finally {

                    ReleaseLocks(0, acquiredLocks);
                }

                return true;
            }
        }

        #region IDictionary<TKey,TValue> members
        void IDictionary<TKey, TValue>.Add(TKey key, TValue value) {
            if (!TryAdd(key, value)) {
                throw new ArgumentException("键已经存在。");
            }
        }
            
        bool IDictionary<TKey, TValue>.Remove(TKey key) {
            TValue throwAwayValue;
            return TryRemove(key, out throwAwayValue);
        }

        /// <summary>
        /// 获得键的集合。
        /// </summary>
        public ICollection<TKey> Keys {
            get { return GetKeys(); }
        }
            
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys {
            get { return GetKeys(); }
        }

        /// <summary>
        /// 获得值得集合。
        /// </summary>
        public ICollection<TValue> Values {
            get { return GetValues(); }
        }
            
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values {
            get { return GetValues(); }
        }
        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

            
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair) {
            ((IDictionary<TKey, TValue>)this).Add(keyValuePair.Key, keyValuePair.Value);
        }

            
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair) {
            TValue value;
            if (!TryGetValue(keyValuePair.Key, out value)) {
                return false;
            }
            return EqualityComparer<TValue>.Default.Equals(value, keyValuePair.Value);
        }
            
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly {
            get { return false; }
        }

            
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair) {
            if (keyValuePair.Key == null) throw new ArgumentNullException(nameof(keyValuePair),"键不能为空。");

            TValue throwAwayValue;
            return TryRemoveInternal(keyValuePair.Key, out throwAwayValue, true, keyValuePair.Value);
        }

        #endregion

        #region IEnumerable Members

            
        IEnumerator IEnumerable.GetEnumerator() {
            return ((ConcurrentDictionary<TKey, TValue>)this).GetEnumerator();
        }

        #endregion

        #region IDictionary Members

            
        void IDictionary.Add(object key, object value) {
            if (key == null) ThrowKeyNullException();
            if (!(key is TKey)) throw new ArgumentException("键是错误的类型。");

            TValue typedValue;
            try {
                typedValue = (TValue)value;
            }
            catch (InvalidCastException) {
                throw new ArgumentException("值是错误的类型。");
            }

            ((IDictionary<TKey, TValue>)this).Add((TKey)key, typedValue);
        }
        bool IDictionary.Contains(object key) {
            if (key == null) ThrowKeyNullException();

            return (key is TKey) && this.ContainsKey((TKey)key);
        }
        IDictionaryEnumerator IDictionary.GetEnumerator() {
            return new DictionaryEnumerator(this);
        }
            
        bool IDictionary.IsFixedSize {
            get { return false; }
        }
            
        bool IDictionary.IsReadOnly {
            get { return false; }
        }
            
        ICollection IDictionary.Keys {
            get { return GetKeys(); }
        }
            
        void IDictionary.Remove(object key) {
            if (key == null) ThrowKeyNullException();

            TValue throwAwayValue;
            if (key is TKey) {
                TryRemove((TKey)key, out throwAwayValue);
            }
        }
            
        ICollection IDictionary.Values {
            get { return GetValues(); }
        }
            
        object IDictionary.this[object key] {
            get {
                if (key == null) ThrowKeyNullException();

                TValue value;
                if (key is TKey && TryGetValue((TKey)key, out value)) {
                    return value;
                }

                return null;
            }
            set {
                if (key == null) ThrowKeyNullException();

                if (!(key is TKey)) throw new ArgumentException("键是错误的类型。");
                if (!(value is TValue)) throw new ArgumentException("值是错误的类型。");

                ((ConcurrentDictionary<TKey, TValue>)this)[(TKey)key] = (TValue)value;
            }
        }

        #endregion

        #region ICollection Members

            
        void ICollection.CopyTo(Array array, int index) {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), "指定的位置不能小于0。");

            int locksAcquired = 0;
            try {
                AcquireAllLocks(ref locksAcquired);
                Tables tables = _tables;

                int count = 0;

                for (int i = 0; i < tables._locks.Length && count >= 0; i++) {
                    count += tables._countPerLock[i];
                }

                if (array.Length - count < index || count < 0) 
                {
                    throw new ArgumentException("指定的位置不能越界。");
                }
                    

                KeyValuePair<TKey, TValue>[] pairs = array as KeyValuePair<TKey, TValue>[];
                if (pairs != null) {
                    CopyToPairs(pairs, index);
                    return;
                }

                DictionaryEntry[] entries = array as DictionaryEntry[];
                if (entries != null) {
                    CopyToEntries(entries, index);
                    return;
                }

                object[] objects = array as object[];
                if (objects != null) {
                    CopyToObjects(objects, index);
                    return;
                }

                throw new ArgumentException("错误的数组类型。", nameof(array));
            }
            finally {
                ReleaseLocks(0, locksAcquired);
            }
        }
            
        bool ICollection.IsSynchronized {
            get { return false; }
        }
            
        object ICollection.SyncRoot {
            get {
                throw new NotSupportedException("不支持同步根。");
            }
        }

        #endregion
            
        private void GrowTable(Tables tables) {
            const int MaxArrayLength = 0X7FEFFFFF;
            int locksAcquired = 0;
            try {

                AcquireLocks(0, 1, ref locksAcquired);
                    
                if (tables != _tables) {
                    return;
                }
                    
                long approxCount = 0;
                for (int i = 0; i < tables._countPerLock.Length; i++) {
                    approxCount += tables._countPerLock[i];
                }
                    
                if (approxCount < tables._buckets.Length / 4) {
                    _budget = 2 * _budget;
                    if (_budget < 0) {
                        _budget = int.MaxValue;
                    }
                    return;
                }
                int newLength = 0;
                bool maximizeTableSize = false;
                try {
                    checked {
                        newLength = tables._buckets.Length * 2 + 1;
                            
                        while (newLength % 3 == 0 || newLength % 5 == 0 || newLength % 7 == 0) {
                            newLength += 2;
                        }

                        Debug.Assert(newLength % 2 != 0);

                        if (newLength > MaxArrayLength) {
                            maximizeTableSize = true;
                        }
                    }
                }
                catch (OverflowException) {
                    maximizeTableSize = true;
                }

                if (maximizeTableSize) {
                    newLength = MaxArrayLength;
                        
                    _budget = int.MaxValue;
                }
                    
                AcquireLocks(1, tables._locks.Length, ref locksAcquired);

                object[] newLocks = tables._locks;
                    
                if (_growLockArray && tables._locks.Length < MaxLockNumber) {
                    newLocks = new object[tables._locks.Length * 2];
                    Array.Copy(tables._locks, 0, newLocks, 0, tables._locks.Length);
                    for (int i = tables._locks.Length; i < newLocks.Length; i++) {
                        newLocks[i] = new object();
                    }
                }

                Node[] newBuckets = new Node[newLength];
                int[] newCountPerLock = new int[newLocks.Length];
                    
                for (int i = 0; i < tables._buckets.Length; i++) {
                    Node current = tables._buckets[i];
                    while (current != null) {
                        Node next = current._next;
                        int newBucketNo, newLockNo;
                        GetBucketAndLockNo(current._hashcode, out newBucketNo, out newLockNo, newBuckets.Length, newLocks.Length);

                        newBuckets[newBucketNo] = new Node(current._key, current._value, current._hashcode, newBuckets[newBucketNo]);

                        checked {
                            newCountPerLock[newLockNo]++;
                        }

                        current = next;
                    }
                }
                    
                _budget = Math.Max(1, newBuckets.Length / newLocks.Length);
                    
                _tables = new Tables(newBuckets, newLocks, newCountPerLock);
            }
            finally {
                ReleaseLocks(0, locksAcquired);
            }
        }
            
        private static int GetBucket(int hashcode, int bucketCount) {
            int bucketNo = (hashcode & 0x7fffffff) % bucketCount;
            Debug.Assert(bucketNo >= 0 && bucketNo < bucketCount);
            return bucketNo;
        }
            
        private static void GetBucketAndLockNo(int hashcode, out int bucketNo, out int lockNo, int bucketCount, int lockCount) {
            bucketNo = (hashcode & 0x7fffffff) % bucketCount;
            lockNo = bucketNo % lockCount;

            Debug.Assert(bucketNo >= 0 && bucketNo < bucketCount);
            Debug.Assert(lockNo >= 0 && lockNo < lockCount);
        }

            
        private static int DefaultConcurrencyLevel {
            get { return Environment.ProcessorCount; }
        }
            
        private void AcquireAllLocks(ref int locksAcquired) {
                
            AcquireLocks(0, 1, ref locksAcquired);
                
            AcquireLocks(1, _tables._locks.Length, ref locksAcquired);
            Debug.Assert(locksAcquired == _tables._locks.Length);
        }
            
        private void AcquireLocks(int fromInclusive, int toExclusive, ref int locksAcquired) {
            Debug.Assert(fromInclusive <= toExclusive);
            object[] locks = _tables._locks;

            for (int i = fromInclusive; i < toExclusive; i++) {
                bool lockTaken = false;
                try {
                    Monitor.Enter(locks[i], ref lockTaken);
                }
                finally {
                    if (lockTaken) {
                        locksAcquired++;
                    }
                }
            }
        }
            
        private void ReleaseLocks(int fromInclusive, int toExclusive) {
            Debug.Assert(fromInclusive <= toExclusive);

            for (int i = fromInclusive; i < toExclusive; i++) {
                Monitor.Exit(_tables._locks[i]);
            }
        }
            
        private ReadOnlyCollection<TKey> GetKeys() {
            int locksAcquired = 0;
            try {
                AcquireAllLocks(ref locksAcquired);
                List<TKey> keys = new List<TKey>();

                for (int i = 0; i < _tables._buckets.Length; i++) {
                    Node current = _tables._buckets[i];
                    while (current != null) {
                        keys.Add(current._key);
                        current = current._next;
                    }
                }

                return new ReadOnlyCollection<TKey>(keys);
            }
            finally {
                ReleaseLocks(0, locksAcquired);
            }
        }

            
        private ReadOnlyCollection<TValue> GetValues() {
            int locksAcquired = 0;
            try {
                AcquireAllLocks(ref locksAcquired);
                List<TValue> values = new List<TValue>();

                for (int i = 0; i < _tables._buckets.Length; i++) {
                    Node current = _tables._buckets[i];
                    while (current != null) {
                        values.Add(current._value);
                        current = current._next;
                    }
                }

                return new ReadOnlyCollection<TValue>(values);
            }
            finally {
                ReleaseLocks(0, locksAcquired);
            }
        }

        private sealed class Node {
            internal readonly TKey _key;
            internal TValue _value;
            internal volatile Node _next;
            internal readonly int _hashcode;

            internal Node(TKey key, TValue value, int hashcode, Node next) {
                _key = key;
                _value = value;
                _next = next;
                _hashcode = hashcode;
            }
        }
            
        private sealed class DictionaryEnumerator : IDictionaryEnumerator {
            IEnumerator<KeyValuePair<TKey, TValue>> _enumerator; 

            internal DictionaryEnumerator(ConcurrentDictionary<TKey, TValue> dictionary) {
                _enumerator = dictionary.GetEnumerator();
            }

            public DictionaryEntry Entry {
                get { return new DictionaryEntry(_enumerator.Current.Key, _enumerator.Current.Value); }
            }

            public object Key {
                get { return _enumerator.Current.Key; }
            }

            public object Value {
                get { return _enumerator.Current.Value; }
            }

            public object Current {
                get { return Entry; }
            }

            public bool MoveNext() {
                return _enumerator.MoveNext();
            }

            public void Reset() {
                _enumerator.Reset();
            }
        }
    }

    internal sealed class IDictionaryDebugView<K, V> {
        private readonly IDictionary<K, V> _dictionary;

        public IDictionaryDebugView(IDictionary<K, V> dictionary) {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            _dictionary = dictionary;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<K, V>[] Items {
            get {
                KeyValuePair<K, V>[] items = new KeyValuePair<K, V>[_dictionary.Count];
                _dictionary.CopyTo(items, 0);
                return items;
            }
        }
    }
}
