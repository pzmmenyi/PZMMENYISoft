using System.Collections.Generic;
using System.Linq;

namespace PZMMENYI.Specifications {
    /// <summary>
    /// 规约。
    /// </summary>
    /// <typeparam name="TTarget"></typeparam>
    public abstract class Specification<TTarget> : ISpecification<TTarget> {
        /// <summary>
        /// 确定目标对象是否满足规范。
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool IsSatisfiedBy(TTarget target) {

            return !IsNotSatisfiedBecause(target).Any();
        }
        /// <summary>
        /// 不满足的原因。
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public IEnumerable<string> WhyIsNotSatisfiedBy(TTarget target) {

            return IsNotSatisfiedBecause(target);
        }
        protected abstract IEnumerable<string> IsNotSatisfiedBecause(TTarget target);

        public static ISpecification<TTarget> operator &(Specification<TTarget> left, Specification<TTarget> right) {

            return left.And(right);
        }
        public static ISpecification<TTarget> operator |(Specification<TTarget> left, Specification<TTarget> right) {

            return left.Or(right);
        }
        public static ISpecification<TTarget> operator !(Specification<TTarget> specification) {

            return specification.Not();
        }
    }
}
