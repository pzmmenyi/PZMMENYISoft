namespace PZMMENYI.Options {
    /// <summary>
    /// 用于检索选项<see cref="TOptions"/>实例。
    /// </summary>
    ///  <typeparam name="TOptions">选项的类型。</typeparam>
    public interface IOptions<out TOptions> where TOptions : class, new() {
        /// <summary>
        /// 选项的类型实例。
        /// </summary>
        TOptions Value { get; }
    }
}
