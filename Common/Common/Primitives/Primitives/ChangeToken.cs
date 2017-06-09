using System;

namespace PZMMENYI.Primitives{
    /// <summary>
    /// 改变发生时，传播通知。
    /// </summary>
    public static class ChangeToken{
        /// <summary>
        /// 注册一个<paramref name="changeTokenConsumer"/>，当令牌发生改变时。
        /// </summary>
        /// <param name="changeTokenProducer">一个代理返回<see cref="IChangeToken"/>。</param>
        /// <param name="changeTokenConsumer">当令牌发生改变时，执行的操作。</param>
        /// <returns>释放资源的接口。</returns>
        public static IDisposable OnChange(Func<IChangeToken> changeTokenProducer, Action changeTokenConsumer){
            if (changeTokenProducer == null){
                throw new ArgumentNullException(nameof(changeTokenProducer));
            }
            if (changeTokenConsumer == null){
                throw new ArgumentNullException(nameof(changeTokenConsumer));
            }

            Action<object> callback = null;
            callback = (s) =>{
                var t = changeTokenProducer();
                try{
                    changeTokenConsumer();
                }
                finally {
                    t.RegisterChangeCallback(callback, null);
                }
            };

            return changeTokenProducer().RegisterChangeCallback(callback, null);
        }

        /// <summary>
        /// 注册一个<paramref name="changeTokenConsumer"/>，当令牌发生改变时。
        /// </summary>
        /// <param name="changeTokenProducer">一个代理返回<see cref="IChangeToken"/>。</param>
        /// <param name="changeTokenConsumer">当令牌发生改变时，执行的操作。</param>
        /// <param name="state">注册者的状态。</param>
        /// <returns>释放资源的接口。</returns>
        public static IDisposable OnChange<TState>(Func<IChangeToken> changeTokenProducer, Action<TState> changeTokenConsumer, TState state){
            if (changeTokenProducer == null){
                throw new ArgumentNullException(nameof(changeTokenProducer));
            }
            if (changeTokenConsumer == null){
                throw new ArgumentNullException(nameof(changeTokenConsumer));
            }

            Action<object> callback = null;
            callback = (s) =>{
                var t = changeTokenProducer();
                try{
                    changeTokenConsumer((TState)s);
                }
                finally {
                    t.RegisterChangeCallback(callback, s);
                }
            };

            return changeTokenProducer().RegisterChangeCallback(callback, state);
        }
    }
}
