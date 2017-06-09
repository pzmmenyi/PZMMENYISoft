namespace PZMMENYI.Options {
    /// <summary>
    /// 选项帮助类。
    /// </summary>
    public static class Options {
        /// <summary>
        /// 创建一个选项包装器实例。
        /// </summary>
        /// <typeparam name="TOptions">选项的类型。</typeparam>
        /// <param name="options"></param>
        /// <returns>包装的选项实例。</returns>
        public static IOptions<TOptions> Create<TOptions>(TOptions options) where TOptions : class, new() {

            return new OptionsWrapper<TOptions>(options);
        }
    }
}
