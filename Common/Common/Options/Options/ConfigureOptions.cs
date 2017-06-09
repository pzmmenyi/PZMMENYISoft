using System;

namespace PZMMENYI.Options {
    /// <summary>
    /// 描述配置其选项类型。
    /// </summary>
    /// <typeparam name="TOptions">选项的类型。</typeparam>
    public class ConfigureOptions<TOptions> : IConfigureOptions<TOptions> where TOptions : class {
        /// <summary>
        /// 配置选项的操作，以<see cref="TOptions"/>为参数。
        /// </summary>
        public Action<TOptions> Action { get; }
        /// <summary>
        /// 初始化。
        /// </summary>
        /// <param name="action">选项的操作。</param>
        public ConfigureOptions(Action<TOptions> action) {

            if (action == null) {
                throw new ArgumentNullException(nameof(action));
            }

            Action = action;
        }
        /// <summary>
        /// 配置选项。
        /// </summary>
        /// <param name="options"></param>
        public virtual void Configure(TOptions options) {

            if (options == null) {
                throw new ArgumentNullException(nameof(options));
            }

            Action.Invoke(options);
        }
    }
}
