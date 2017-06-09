namespace PZMMENYI.Options {
    /// <summary>
    /// 选项快照。
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public class OptionsSnapshot<TOptions> : IOptionsSnapshot<TOptions> where TOptions : class, new() {

        private TOptions _options;
        /// <summary>
        /// 初始化。
        /// </summary>
        /// <param name="monitor">选项追踪类。</param>
        public OptionsSnapshot(IOptionsMonitor<TOptions> monitor) {
            _options = monitor.CurrentValue;
        }
        /// <summary>
        /// 选项实例。
        /// </summary>
        public virtual TOptions Value {
            get {
                return _options;
            }
        }
    }
}
