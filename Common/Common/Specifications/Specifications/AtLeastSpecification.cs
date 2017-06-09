using System;
using System.Collections.Generic;
using System.Linq;
using PZMMENYI.Utilities;

namespace PZMMENYI.Specifications {
    /// <summary>
    /// 至少规约。
    /// </summary>
    /// <typeparam name="TTarget"></typeparam>
    public class AtLeastSpecification<TTarget> : Specification<TTarget> {

        private readonly int _requiredSpecifications;
        private readonly IReadOnlyList<ISpecification<TTarget>> _specifications;

        public AtLeastSpecification(int requiredSpecifications,IEnumerable<ISpecification<TTarget>> specifications) {

            var specificationList = (specifications ?? Enumerable.Empty<ISpecification<TTarget>>()).ToList();

            if (requiredSpecifications <= 0)
                throw new ArgumentOutOfRangeException(nameof(requiredSpecifications));
            if (!specificationList.Any())
                throw new ArgumentException("请提供一些规约。", nameof(specifications));
            if (requiredSpecifications > specificationList.Count)
                throw new ArgumentOutOfRangeException($"你请求规约数('{requiredSpecifications}')，但是其大于规约总数('{specificationList.Count}')。");

            _requiredSpecifications = requiredSpecifications;
            _specifications = specificationList;
        }
        protected override IEnumerable<string> IsNotSatisfiedBecause(TTarget target) {

            var notStatisfiedReasons = _specifications
                .Select(s => new
                {
                    Specification = s,
                    WhyIsNotStatisfied = s.WhyIsNotSatisfiedBy(target).ToList()
                })
                .Where(a => a.WhyIsNotStatisfied.Any())
                .Select(a => $"{a.Specification.GetType().PrettyPrint()}: {string.Join(", ", a.WhyIsNotStatisfied)}")
                .ToList();

            return (_specifications.Count - notStatisfiedReasons.Count) >= _requiredSpecifications
                ? Enumerable.Empty<string>()
                : notStatisfiedReasons;
        }
    }
}
