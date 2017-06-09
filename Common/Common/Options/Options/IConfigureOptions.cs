namespace PZMMENYI.Options {
    /// <summary>
    /// 描述配置其选项类型。
    /// </summary>
    /// <typeparam name="TOptions">选项的类型。</typeparam>
    public interface IConfigureOptions<in TOptions> where TOptions : class {
        /// <summary>
        /// 配置选项。
        /// </summary>
        /// <param name="options"></param>
        void Configure(TOptions options);
    }
}
