using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using PZMMENYI.Utilities;

namespace PZMMENYI.Specifications {
    /// <summary>
    /// 表达式规约。
    /// </summary>
    /// <typeparam name="TTarget"></typeparam>
    public class ExpressionSpecification<TTarget> : Specification<TTarget> {

        private readonly Func<TTarget, bool> _predicate;
        private readonly Lazy<string> _string;

        public ExpressionSpecification(Expression<Func<TTarget, bool>> expression) {

            _predicate = expression.Compile();
            _string = new Lazy<string>(() => MakeString(expression));
        }
        public override string ToString() {

            return _string.Value;
        }

        protected override IEnumerable<string> IsNotSatisfiedBecause(TTarget target) {

            if (!_predicate(target)) {

                yield return $"'{_string.Value}' 不是规约。";
            }
        }

        private static string MakeString(Expression<Func<TTarget, bool>> expression) {
            try {
                var paramName = expression.Parameters[0].Name;
                var expBody = expression.Body.ToString();

                expBody = expBody
                    .Replace("AndAlso", "&&")
                    .Replace("OrElse", "||");

                return $"{paramName} => {expBody}";
            }
            catch {
                return typeof(ExpressionSpecification<TTarget>).PrettyPrint();
            }
        }
    }
}
