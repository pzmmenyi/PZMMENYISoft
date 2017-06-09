using System;
using System.Collections.Generic;
using PZMMENYI.Utilities;

namespace PZMMENYI.Specifications {
    /// <summary>
    /// 非规约。
    /// </summary>
    /// <typeparam name="TTarget"></typeparam>
    public  class NotSpecification<TTarget> :Specification<TTarget> {

        private readonly ISpecification<TTarget> _specification;
        public NotSpecification(ISpecification<TTarget> specification) {

            _specification = specification ?? throw new ArgumentNullException(nameof(specification));
        }

        protected override IEnumerable<string> IsNotSatisfiedBecause(TTarget target) {

            if (_specification.IsSatisfiedBy(target)) {

                yield return $" '{_specification.GetType().PrettyPrint()}'类型不满足规范。";
            }
        }
    }
}
