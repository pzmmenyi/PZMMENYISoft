using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System;

namespace PZMMENYI.Collections.Concurrent {
    public class ConcurrentBag<T> : IProducerConsumerCollection<T>, IReadOnlyCollection<T> {
        
        private ThreadLocal<ThreadLocalList> _locals;
        private volatile ThreadLocalList _headList, _tailList;
        private bool _needSync;

        /// <summary>
        ///初始化。
        /// </summary>
        public ConcurrentBag() {

            Initialize(null);
        }

        /// <summary>
        /// 初始化，并制定复制的数据。
        /// </summary>
        /// <param name="collection"></param>
        public ConcurrentBag(IEnumerable<T> collection) {

            if (collection == null) {

                throw new ArgumentNullException(nameof(collection), "要复制的不能为空。");
            }
            Initialize(collection);
        }
        
        private void Initialize(IEnumerable<T> collection) {

            _locals = new ThreadLocal<ThreadLocalList>();
            
            if (collection != null) {

                ThreadLocalList list = GetThreadList(true);
                foreach (T item in collection) {
                    list.Add(item, false);
                }
            }
        }

        /// <summary>
        /// 添加一个数据到 <see cref="ConcurrentBag{T}"/>中。
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item) {
            
            ThreadLocalList list = GetThreadList(true);
            AddInternal(list, item);
        }
        
        private void AddInternal(ThreadLocalList list, T item) {

            bool lockTaken = false;
            try {
                Interlocked.Exchange(ref list._currentOp, (int)ListOperation.Add);

                if (list.Count < 2 || _needSync) {

                    list._currentOp = (int)ListOperation.None;
                    Monitor.Enter(list, ref lockTaken);
                }
                list.Add(item, lockTaken);
            }
            finally {
                list._currentOp = (int)ListOperation.None;
                if (lockTaken) {
                    Monitor.Exit(list);
                }
            }
        }
        
        bool IProducerConsumerCollection<T>.TryAdd(T item) {

            Add(item);
            return true;
        }

        /// <summary>
        /// 从<see cref="IProducerConsumerCollection{T}"/> 中尝试删除并返回一个对象。
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryTake(out T result) {

            return TryTakeOrPeek(out result, true);
        }

        /// <summary>
        /// 从<see cref="IProducerConsumerCollection{T}"/> 中查找指定数据。
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryPeek(out T result) {

            return TryTakeOrPeek(out result, false);
        }
        
        private bool TryTakeOrPeek(out T result, bool take) {

            ThreadLocalList list = GetThreadList(false);
            if (list == null || list.Count == 0) {

                return Steal(out result, take);
            }

            bool lockTaken = false;
            try {
                if (take) {

                    Interlocked.Exchange(ref list._currentOp, (int)ListOperation.Take);
                    
                    if (list.Count <= 2 || _needSync) {

                        list._currentOp = (int)ListOperation.None;
                        Monitor.Enter(list, ref lockTaken);


                        if (list.Count == 0) {

                            if (lockTaken) {

                                try { }
                                finally {

                                    lockTaken = false; 
                                    Monitor.Exit(list);
                                }
                            }
                            return Steal(out result, true);
                        }
                    }
                    list.Remove(out result);
                }
                else {

                    if (!list.Peek(out result)) {

                        return Steal(out result, false);
                    }
                }
            }
            finally {

                list._currentOp = (int)ListOperation.None;
                if (lockTaken) {

                    Monitor.Exit(list);
                }
            }
            return true;
        }
        
        private ThreadLocalList GetThreadList(bool forceCreate) {

            ThreadLocalList list = _locals.Value;

            if (list != null) {

                return list;
            }
            else if (forceCreate) {

                lock (GlobalListsLock) {

                    if (_headList == null) {

                        list = new ThreadLocalList(Environment.CurrentManagedThreadId);
                        _headList = list;
                        _tailList = list;
                    }
                    else {

                        list = GetUnownedList();
                        if (list == null) {

                            list = new ThreadLocalList(Environment.CurrentManagedThreadId);
                            _tailList._nextList = list;
                            _tailList = list;
                        }
                    }
                    _locals.Value = list;
                }
            }
            else {
                return null;
            }
            Debug.Assert(list != null);
            return list;
        }

        private ThreadLocalList GetUnownedList() {

            Debug.Assert(Monitor.IsEntered(GlobalListsLock));

            int currentThreadId = Environment.CurrentManagedThreadId;
            ThreadLocalList currentList = _headList;
            while (currentList != null) {

                if (currentList._ownerThreadId == currentThreadId) {

                    return currentList;
                }
                currentList = currentList._nextList;
            }
            return null;
        }



        private bool Steal(out T result, bool take) {

            bool loop;
            List<int> versionsList = new List<int>(); 
            do {

                versionsList.Clear(); 
                loop = false;


                ThreadLocalList currentList = _headList;
                while (currentList != null) {

                    versionsList.Add(currentList._version);
                    if (currentList._head != null && TrySteal(currentList, out result, take)) {

                        return true;
                    }
                    currentList = currentList._nextList;
                }

                currentList = _headList;
                foreach (int version in versionsList) {

                    if (version != currentList._version) {

                        loop = true;
                        if (currentList._head != null && TrySteal(currentList, out result, take))
                            return true;
                    }
                    currentList = currentList._nextList;
                }
            } while (loop);


            result = default(T);
            return false;
        }
        
        private bool TrySteal(ThreadLocalList list, out T result, bool take) {

            lock (list) {

                if (CanSteal(list)) {

                    list.Steal(out result, take);
                    return true;
                }
                result = default(T);
                return false;
            }
        }
        private static bool CanSteal(ThreadLocalList list) {

            if (list.Count <= 2 && list._currentOp != (int)ListOperation.None) {


                SpinWait spinner = new SpinWait();
                while (list._currentOp != (int)ListOperation.None) {

                    spinner.SpinOnce();
                }
            }
            return list.Count > 0;
        }
        /// <summary>
        /// 将<see cref="T:System.Array"/>拷贝到<see cref="IProducerConsumerCollection{T}"/> 中并指定的索引。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="index"></param>
        public void CopyTo(T[] array, int index) {
            if (array == null) {
                throw new ArgumentNullException(nameof(array), "要拷贝的数据不能为空。");
            }
            if (index < 0) {
                throw new ArgumentOutOfRangeException
                    (nameof(index), "索引不能小于0。");
            }
            
            if (_headList == null)
                return;

            bool lockTaken = false;
            try {

                FreezeBag(ref lockTaken);
                ToList().CopyTo(array, index);
            }
            finally {

                UnfreezeBag(lockTaken);
            }
        }
        
        void ICollection.CopyTo(Array array, int index) {

            if (array == null) {

                throw new ArgumentNullException(nameof(array), "要拷贝的数据不能为空。");
            }

            bool lockTaken = false;
            try {

                FreezeBag(ref lockTaken);
                ((ICollection)ToList()).CopyTo(array, index);
            }
            finally {

                UnfreezeBag(lockTaken);
            }
        }

        /// <summary>
        /// 将<see cref="IProducerConsumerCollection{T}"/> 拷贝到一个新数组当中。
        /// </summary>
        /// <returns></returns>
        public T[] ToArray() {

            if (_headList == null)
                return new T[0];

            bool lockTaken = false;
            try {

                FreezeBag(ref lockTaken);
                return ToList().ToArray();
            }
            finally {

                UnfreezeBag(lockTaken);
            }
        }

        /// <summary>
        /// 从<see cref="ConcurrentBag{T}"/>中返回一个迭代的枚举器。
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator() {

            if (_headList == null)
                return ((IEnumerable<T>)new T[0]).GetEnumerator();

            bool lockTaken = false;
            try {

                FreezeBag(ref lockTaken);
                return ToList().GetEnumerator();
            }
            finally {

                UnfreezeBag(lockTaken);
            }
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return ((ConcurrentBag<T>)this).GetEnumerator();
        }

        /// <summary>
        /// 获得集合中数据的个数。
        /// </summary>
        public int Count {

            get {

                if (_headList == null)
                    return 0;

                bool lockTaken = false;
                try {

                    FreezeBag(ref lockTaken);
                    return GetCountInternal();
                }
                finally {

                    UnfreezeBag(lockTaken);
                }
            }
        }

        /// <summary>
        /// 获得聚合是否为空。
        /// </summary>
        public bool IsEmpty {

            get {

                if (_headList == null)
                    return true;

                bool lockTaken = false;
                try {

                    FreezeBag(ref lockTaken);
                    ThreadLocalList currentList = _headList;
                    while (currentList != null) {

                        if (currentList._head != null) {

                            return false;
                        }
                        currentList = currentList._nextList;
                    }
                    return true;
                }
                finally {
                    UnfreezeBag(lockTaken);
                }
            }
        }
        
        bool ICollection.IsSynchronized {

            get { return false; }
        }
        
        object ICollection.SyncRoot {

            get {

                throw new NotSupportedException("集合不支持根异步。");
            }
        }
        
        private object GlobalListsLock {

            get {

                Debug.Assert(_locals != null);
                return _locals;
            }
        }

        private void FreezeBag(ref bool lockTaken) {

            Debug.Assert(!Monitor.IsEntered(GlobalListsLock));
            
            Monitor.Enter(GlobalListsLock, ref lockTaken);

            _needSync = true;

            AcquireAllLocks();

            WaitAllOperations();
        }

        private void UnfreezeBag(bool lockTaken) {

            ReleaseAllLocks();
            _needSync = false;
            if (lockTaken) {

                Monitor.Exit(GlobalListsLock);
            }
        }


        private void AcquireAllLocks() {

            Debug.Assert(Monitor.IsEntered(GlobalListsLock));

            bool lockTaken = false;
            ThreadLocalList currentList = _headList;
            while (currentList != null) {
                
                try {

                    Monitor.Enter(currentList, ref lockTaken);
                }
                finally {

                    if (lockTaken) {

                        currentList._lockTaken = true;
                        lockTaken = false;
                    }
                }
                currentList = currentList._nextList;
            }
        }
        
        private void ReleaseAllLocks() {

            ThreadLocalList currentList = _headList;
            while (currentList != null) {

                if (currentList._lockTaken) {

                    currentList._lockTaken = false;
                    Monitor.Exit(currentList);
                }
                currentList = currentList._nextList;
            }
        }
        
        private void WaitAllOperations() {

            Debug.Assert(Monitor.IsEntered(GlobalListsLock));

            ThreadLocalList currentList = _headList;
            while (currentList != null) {

                if (currentList._currentOp != (int)ListOperation.None) {

                    SpinWait spinner = new SpinWait();
                    while (currentList._currentOp != (int)ListOperation.None) {

                        spinner.SpinOnce();
                    }
                }
                currentList = currentList._nextList;
            }
        }
        private int GetCountInternal() {

            Debug.Assert(Monitor.IsEntered(GlobalListsLock));

            int count = 0;
            ThreadLocalList currentList = _headList;
            while (currentList != null) {

                checked {

                    count += currentList.Count;
                }
                currentList = currentList._nextList;
            }
            return count;
        }
        
        private List<T> ToList() {

            Debug.Assert(Monitor.IsEntered(GlobalListsLock));

            List<T> list = new List<T>();
            ThreadLocalList currentList = _headList;
            while (currentList != null) {

                Node currentNode = currentList._head;
                while (currentNode != null) {

                    list.Add(currentNode._value);
                    currentNode = currentNode._next;
                }
                currentList = currentList._nextList;
            }

            return list;
        }
        
        internal class Node {
            public Node(T value) {

                _value = value;
            }
            public readonly T _value;
            public Node _next;
            public Node _prev;
        }
        internal class ThreadLocalList {

            internal volatile Node _head;
            private volatile Node _tail;
            internal volatile int _currentOp;
            private int _count;
            internal int _stealCount;
            
            internal volatile ThreadLocalList _nextList;
            internal bool _lockTaken;
            internal int _ownerThreadId;
            internal volatile int _version;

            internal ThreadLocalList(int ownerThreadId) {

                _ownerThreadId = ownerThreadId;
            }

            internal void Add(T item, bool updateCount) {

                checked {

                    _count++;
                }
                Node node = new Node(item);
                if (_head == null) {

                    Debug.Assert(_tail == null);
                    _head = node;
                    _tail = node;
                    _version++; 
                }
                else {

                    node._next = _head;
                    _head._prev = node;
                    _head = node;
                }
                if (updateCount) {

                    _count = _count - _stealCount;
                    _stealCount = 0;
                }
            }

            internal void Remove(out T result) {

                Debug.Assert(_head != null);
                Node head = _head;
                _head = _head._next;
                if (_head != null) {

                    _head._prev = null;
                }
                else {
                    _tail = null;
                }
                _count--;
                result = head._value;
            }
            
            internal bool Peek(out T result) {

                Node head = _head;
                if (head != null) {

                    result = head._value;
                    return true;
                }
                result = default(T);
                return false;
            }

            internal void Steal(out T result, bool remove) {

                Node tail = _tail;
                Debug.Assert(tail != null);
                if (remove) {

                    _tail = _tail._prev;
                    if (_tail != null) {

                        _tail._next = null;
                    }
                    else {
                        _head = null;
                    }

                    _stealCount++;
                }
                result = tail._value;
            }


            internal int Count {

                get {

                    return _count - _stealCount;
                }
            }
        }

    }

    internal enum ListOperation {

        None,
        Add,
        Take
    };
}
