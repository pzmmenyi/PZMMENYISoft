using PZMMENYI.Primitives;
using System;
using System.Collections.Generic;

namespace PZMMENYI.Options {
    /// <summary>
    /// 用于通知，当选项<see cref="TOptions"/>实例改变。
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public class OptionsMonitor<TOptions> : IOptionsMonitor<TOptions> where TOptions : class, new() {

        private OptionsCache<TOptions> _optionsCache;
        private readonly IEnumerable<IConfigureOptions<TOptions>> _setups;
        private readonly IEnumerable<IOptionsChangeTokenSource<TOptions>> _sources;
        private List<Action<TOptions>> _listeners = new List<Action<TOptions>>();
        /// <summary>
        /// 初始化。
        /// </summary>
        /// <param name="setups">描述配置其选项类型集合。</param>
        /// <param name="sources">跟踪选项更改类集合。</param>
        public OptionsMonitor(IEnumerable<IConfigureOptions<TOptions>> setups, IEnumerable<IOptionsChangeTokenSource<TOptions>> sources) {
            _sources = sources;
            _setups = setups;
            _optionsCache = new OptionsCache<TOptions>(setups);

            foreach (var source in _sources) {
                ChangeToken.OnChange(
                    () => source.GetChangeToken(),
                    () => InvokeChanged());
            }
        }
        private void InvokeChanged() {
            _optionsCache = new OptionsCache<TOptions>(_setups);
            foreach (var listener in _listeners) {
                listener?.Invoke(_optionsCache.Value);
            }
        }
        /// <summary>
        /// 变化前的选项实例。
        /// </summary>
        public TOptions CurrentValue {
            get {
                return _optionsCache.Value;
            }
        }
        /// <summary>
        /// 执行改变。
        /// </summary>
        /// <param name="listener"></param>
        /// <returns></returns>
        public IDisposable OnChange(Action<TOptions> listener) {
            var disposable = new ChangeTrackerDisposable(_listeners, listener);
            _listeners.Add(listener);
            return disposable;
        }
        internal class ChangeTrackerDisposable : IDisposable { 
            private readonly Action<TOptions> _originalListener;
            private readonly IList<Action<TOptions>> _listeners;

            public ChangeTrackerDisposable(IList<Action<TOptions>> listeners, Action<TOptions> listener) {
                _originalListener = listener;
                _listeners = listeners;
            }

            public void Dispose() {
                _listeners.Remove(_originalListener);
            }
        }
    }
}
