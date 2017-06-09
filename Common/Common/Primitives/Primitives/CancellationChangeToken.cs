using System;
using System.Threading;

namespace PZMMENYI.Primitives {
    /// <summary>
    /// 操作变化通知，继承<see cref="IChangeToken"。/>
    /// </summary>
    public class CancellationChangeToken : IChangeToken {
        /// <summary>
        /// 初始化。
        /// </summary>
        /// <param name="cancellationToken">传播有关应取消操作的通知。</param>
        public CancellationChangeToken(CancellationToken cancellationToken) {
            Token = cancellationToken;
        }
        private CancellationToken Token { get; }
        /// <summary>
        /// 值为true，表明将主动回调。
        /// </summary>
        public bool ActiveChangeCallbacks => true;
        /// <summary>
        /// 值为true，表明变化已经发生。
        /// </summary>
        public bool HasChanged => Token.IsCancellationRequested;
        /// <summary>
        /// 值变化时，注册一个回调，<see cref="HasChanged"/>必须设置为ture。
        /// </summary>
        /// <param name="callback">回调操作。</param>
        /// <param name="state">状态。</param>
        /// <returns>释放资源的接口。</returns>
        public IDisposable RegisterChangeCallback(Action<object> callback, object state) => Token.Register(callback, state);
    }
}
