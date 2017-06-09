using System;
using System.Collections.Generic;
using System.Linq;

namespace PZMMENYI.Specifications {
    /// <summary>
    /// 且规约。
    /// </summary>
    /// <typeparam name="TTarget"></typeparam>
    public  class AndSpeficication<TTarget> :Specification<TTarget>{

        private readonly ISpecification<TTarget> _specificationLeft;
        private readonly ISpecification<TTarget> _specificationRight;

        public AndSpeficication(ISpecification<TTarget> specificationLeft,ISpecification<TTarget> specificationRight) {

            _specificationLeft = specificationLeft ?? throw new ArgumentNullException(nameof(specificationLeft));
            _specificationRight = specificationRight ?? throw new ArgumentNullException(nameof(specificationRight));

        }

        protected override IEnumerable<string> IsNotSatisfiedBecause(TTarget target) {

            return _specificationLeft.WhyIsNotSatisfiedBy(target).Concat(_specificationRight.WhyIsNotSatisfiedBy(target));
        }
    }
}
