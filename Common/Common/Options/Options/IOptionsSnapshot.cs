namespace PZMMENYI.Options {
    /// <summary>
    /// 用于访问选项<see cref="TOptions"/>的值。
    /// </summary>
    /// <typeparam name="TOptions">选项的类型。</typeparam>
    public interface IOptionsSnapshot<out TOptions> {
        /// <summary>
        /// 返回的选项<see cref="TOptions"/>值，将计算一次。
        /// </summary>
        TOptions Value { get; }
    }
}
