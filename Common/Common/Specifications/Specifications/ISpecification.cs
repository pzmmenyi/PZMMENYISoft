using System.Collections.Generic;

namespace PZMMENYI.Specifications {
    /// <summary>
    /// 规约。
    /// </summary>
    /// <typeparam name="TTarget"></typeparam>
    public interface ISpecification<in TTarget> {
        /// <summary>
        /// 确定目标对象是否满足规范。
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        bool IsSatisfiedBy(TTarget target);
        /// <summary>
        /// 不满足的原因。
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        IEnumerable<string> WhyIsNotSatisfiedBy(TTarget target);
    }
}
