using System;
using System.Collections.Generic;
using System.Threading;

namespace PZMMENYI.Options {
    /// <summary>
    /// 配置选项缓存，通过配置选项类集合。
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    internal class OptionsCache<TOptions> where TOptions : class, new() {

        private readonly Func<TOptions> _createCache;
        private object _cacheLock = new object();
        private bool _cacheInitialized;
        private TOptions _options;
        private IEnumerable<IConfigureOptions<TOptions>> _setups;
        /// <summary>
        /// 初始化。
        /// </summary>
        /// <param name="setups">描述配置其选项类型集合。</param>
        public OptionsCache(IEnumerable<IConfigureOptions<TOptions>> setups) {
            _setups = setups;
            _createCache = CreateOptions;
        }
        private TOptions CreateOptions() {
            var result = new TOptions();
            if (_setups != null) {
                foreach (var setup in _setups) {
                    setup.Configure(result);
                }
            }
            return result;
        }
        /// <summary>
        /// 选项实例。
        /// </summary>
        public virtual TOptions Value {
            get {
                return LazyInitializer.EnsureInitialized(
                    ref _options,
                    ref _cacheInitialized,
                    ref _cacheLock,
                    _createCache);
            }
        }
    }
}
