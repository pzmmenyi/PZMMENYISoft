﻿using System;
using System.Collections.Generic;
using PZMMENYI.Utilities;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZMMENYI.ValueObjects {
    /// <summary>
    /// 单一值对象的封装。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SingleValueObject<T> : ValueObject, IComparable, ISingleValueObject
        where T : IComparable, IComparable<T> {
        /// <summary>
        /// 对象的值。
        /// </summary>
        public T Value { get; }

        protected SingleValueObject(T value) {

            Value = value;
        }
        /// <summary>
        /// 与其他比较。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj) {

            if (ReferenceEquals(null, obj)) {

                throw new ArgumentNullException(nameof(obj));
            }

            var other = obj as SingleValueObject<T>;
            if (other == null) {
                throw new ArgumentException($"Cannot compare '{GetType().PrettyPrint()}' and '{obj.GetType().PrettyPrint()}'");
            }

            return Value.CompareTo(other.Value);
        }

        protected override IEnumerable<object> GetEqualityComponents() {

            yield return Value;
        }

        public override string ToString() {

            return ReferenceEquals(Value, null) ? string.Empty : Value.ToString();
        }

        public object GetValue() {

            return Value;
        }
    }
}
