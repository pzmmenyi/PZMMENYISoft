using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PZMMENYI.Specifications {
    public static class SpecificationExtensions {
        public static ISpecification<TTarget> All<TTarget>(this IEnumerable<ISpecification<TTarget>> specifications) {

            return new AllSpecifications<TTarget>(specifications);
        }
        public static ISpecification<TTarget> Dictionary<TKey,TTarget>(this IReadOnlyDictionary<TKey, ISpecification<TTarget>> specifications) {

            return new DictionarySpecifications<TKey,TTarget>(specifications);
        }

        public static ISpecification<TTarget> AtLeast<TTarget>(this IEnumerable<ISpecification<TTarget>> specifications,int requiredSpecifications) {

            return new AtLeastSpecification<TTarget>(requiredSpecifications, specifications);
        }

        public static ISpecification<TTarget> And<TTarget>(this ISpecification<TTarget> specificationLeft,ISpecification<TTarget> specificationRight) {

            return new AndSpeficication<TTarget>(specificationLeft, specificationRight);
        }

        public static ISpecification<TTarget> And<TTarget>(this ISpecification<TTarget> specification,Expression<Func<TTarget, bool>> expression) {

            return specification.And(new ExpressionSpecification<TTarget>(expression));
        }

        public static ISpecification<TTarget> Or<TTarget>(this ISpecification<TTarget> specificationLeft, ISpecification<TTarget> specificationRight) {

            return new OrSpecification<TTarget>(specificationLeft, specificationRight);
        }
        public static ISpecification<TTarget> Or<TTarget>(this ISpecification<TTarget> specification,Expression<Func<TTarget, bool>> expression) {

            return specification.Or(new ExpressionSpecification<TTarget>(expression));
        }

        public static ISpecification<TTarget> Not<TTarget>(this ISpecification<TTarget> specification) {

            return new NotSpecification<TTarget>(specification);
        }
    }
}
