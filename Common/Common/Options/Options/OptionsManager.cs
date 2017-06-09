using System.Collections.Generic;

namespace PZMMENYI.Options {
    /// <summary>
    /// 实例化选项类，通过配置选项类集合(里面用选项缓存类包装)。
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public class OptionsManager<TOptions> : IOptions<TOptions> where TOptions : class, new() {

        private OptionsCache<TOptions> _optionsCache;
        /// <summary>
        /// 初始化。
        /// </summary>
        /// <param name="setups">描述配置其选项类型集合。</param>
        public OptionsManager(IEnumerable<IConfigureOptions<TOptions>> setups) {

            _optionsCache = new OptionsCache<TOptions>(setups);
        }
        /// <summary>
        /// 选项实例。
        /// </summary>
        public virtual TOptions Value {
            get {
                return _optionsCache.Value;
            }
        }
    }
}
