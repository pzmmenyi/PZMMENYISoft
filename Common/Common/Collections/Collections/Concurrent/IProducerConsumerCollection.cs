using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace PZMMENYI.Collections.Concurrent {
    /// <summary>
    /// 定义方法来操纵线程安全的集合用于生产者/消费者使用。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IProducerConsumerCollection<T> : IEnumerable<T>, ICollection {
        /// <summary>
        ///  将<see cref="T:System.Array"/>拷贝到<see cref="IProducerConsumerCollection{T}"/> 中并指定的索引。
        /// </summary>
        /// <param name="array"></param>
        /// <param name="index"></param>
        void CopyTo(T[] array, int index);
        /// <summary>
        /// 试图添加一个对象到<see cref="IProducerConsumerCollection{T}"/> 中。
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool TryAdd(T item);
        /// <summary>
        /// 从<see cref="IProducerConsumerCollection{T}"/> 中尝试删除并返回一个对象。
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool TryTake(out T item);
        /// <summary>
        /// 将<see cref="IProducerConsumerCollection{T}"/> 拷贝到一个新数组当中。
        /// </summary>
        /// <returns></returns>
        T[] ToArray();
    }
}
