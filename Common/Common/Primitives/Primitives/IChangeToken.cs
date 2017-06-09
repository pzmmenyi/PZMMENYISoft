using System;

namespace PZMMENYI.Primitives
{
    /// <summary>
    /// 改变发生时，传播通知。
    /// </summary>
    public interface IChangeToken{
        /// <summary>
        /// 值为true，表明变化已经发生。
        /// </summary>
        bool HasChanged { get; }
        /// <summary>
        /// 值为true，表明将主动回调。
        /// </summary>
        bool ActiveChangeCallbacks { get; }
        /// <summary>
        /// 值变化时，注册一个回调，<see cref="HasChanged"/>必须设置为ture。
        /// </summary>
        /// <param name="callback">回调操作。</param>
        /// <param name="state">状态。</param>
        /// <returns>释放资源的接口。</returns>
        IDisposable RegisterChangeCallback(Action<object> callback, object state);
    }
}
