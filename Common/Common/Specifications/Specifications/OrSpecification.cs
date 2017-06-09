using System;
using System.Collections.Generic;
using System.Linq;

namespace PZMMENYI.Specifications {
    /// <summary>
    /// 或规约。
    /// </summary>
    /// <typeparam name="TTarget"></typeparam>
    public class OrSpecification<TTarget> : Specification<TTarget> {

        private readonly ISpecification<TTarget> _specificationLeft;
        private readonly ISpecification<TTarget> _specificationRight;

        public OrSpecification(ISpecification<TTarget> specificationLeft, ISpecification<TTarget> specificationRight) {

            _specificationLeft = specificationLeft ?? throw new ArgumentNullException(nameof(specificationLeft));
            _specificationRight = specificationRight ?? throw new ArgumentNullException(nameof(specificationRight));

        }
        protected override IEnumerable<string> IsNotSatisfiedBecause(TTarget target) {

            var reasonsLeft = _specificationLeft.WhyIsNotSatisfiedBy(target).ToList();
            var reasonsRight = _specificationRight.WhyIsNotSatisfiedBy(target).ToList();

            if (!reasonsLeft.Any() || !reasonsRight.Any()) {

                return Enumerable.Empty<string>();
            }

            return reasonsLeft.Concat(reasonsRight);
        }
    }
}
