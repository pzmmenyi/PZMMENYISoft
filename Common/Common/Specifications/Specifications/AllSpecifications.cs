using System;
using System.Collections.Generic;
using System.Linq;

namespace PZMMENYI.Specifications {
    public class AllSpecifications<TTarget> : Specification<TTarget> {

        private readonly IReadOnlyList<ISpecification<TTarget>> _specifications;

        public AllSpecifications(
            IEnumerable<ISpecification<TTarget>> specifications) {

            var specificationList = (specifications ?? Enumerable.Empty<ISpecification<TTarget>>()).ToList();

            if (!specificationList.Any()) throw new ArgumentException("请提供一些规约。", nameof(specifications));

            _specifications = specificationList;
        }

        protected override IEnumerable<string> IsNotSatisfiedBecause(TTarget target) {

            return _specifications.SelectMany(s => s.WhyIsNotSatisfiedBy(target));
        }
    }
}
