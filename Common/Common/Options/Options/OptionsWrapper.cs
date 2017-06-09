namespace PZMMENYI.Options {
    /// <summary>
    /// 包装选项，返回一个选项实例。
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public class OptionsWrapper<TOptions> : IOptions<TOptions> where TOptions : class, new() {
        /// <summary>
        /// 初始化。
        /// </summary>
        /// <param name="options">选项实例。</param>
        public OptionsWrapper(TOptions options) {
            Value = options;
        }
        /// <summary>
        /// 选项实例。
        /// </summary>
        public TOptions Value { get; }
    }
}
