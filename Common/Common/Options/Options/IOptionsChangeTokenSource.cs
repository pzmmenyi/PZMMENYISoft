using PZMMENYI.Primitives;

namespace PZMMENYI.Options {
    /// <summary>
    /// 用于获取<see cref="IChangeToken"/>，用于跟踪选项更改。
    /// </summary>
    /// <typeparam name="TOptions">选项的类型。</typeparam>
    public interface IOptionsChangeTokenSource<out TOptions> {
        /// <summary>
        /// 返回一个<see cref="IChangeToken"/>，用于跟踪选项更改。
        /// </summary>
        /// <returns></returns>
        IChangeToken GetChangeToken();
    }
}
