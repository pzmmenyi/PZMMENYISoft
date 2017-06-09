using System;

namespace PZMMENYI.Options {
    /// <summary>
    /// 用于通知，当选项<see cref="TOptions"/>实例改变。
    /// </summary>
    /// <typeparam name="TOptions">选项的类型。</typeparam>
    public interface IOptionsMonitor<out TOptions> {
        /// <summary>
        /// 当前选项实例。
        /// </summary>
        TOptions CurrentValue { get; }
        /// <summary>
        /// 注册一个侦听器，每当选项<see cref="TOptions"/>实例改变。
        /// </summary>
        /// <param name="listener">监听者的操作。</param>
        /// <returns></returns>
        IDisposable OnChange(Action<TOptions> listener);
    }
}
