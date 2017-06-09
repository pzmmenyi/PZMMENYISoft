using System;
using System.Collections.Generic;
using System.Linq;

namespace PZMMENYI.Specifications {
    /// <summary>
    /// 所有规约。
    /// </summary>
    /// <typeparam name="TTarget"></typeparam>
    public class DictionarySpecifications<TKey,TTarget> : Specification<TTarget> {

        private readonly IReadOnlyDictionary<TKey, ISpecification<TTarget>> _specifications;

        public DictionarySpecifications(IEnumerable<KeyValuePair<TKey, ISpecification<TTarget>>> specifications) {

            var specificationDictionary = (specifications ?? Enumerable.Empty<KeyValuePair<TKey, ISpecification<TTarget>>>()).ToDictionary(k=>k.Key,e=>e.Value);

            if (!specificationDictionary.Any()) throw new ArgumentException("请提供一些规约。", nameof(specifications));

            _specifications = specificationDictionary;
        }

        protected override IEnumerable<string> IsNotSatisfiedBecause(TTarget target) {

            return _specifications.Values.SelectMany(s => s.WhyIsNotSatisfiedBy(target));
        }
    }
}
