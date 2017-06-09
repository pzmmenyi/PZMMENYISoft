using PZMMENYI.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PZMMENYI.ValueObjects {
    /// <summary>
    /// 值对象的封装。
    /// </summary>
    public abstract class ValueObject {

        private static readonly ConcurrentDictionary<Type, IReadOnlyCollection<PropertyInfo>> _typeProperties = 
            new ConcurrentDictionary<Type, IReadOnlyCollection<PropertyInfo>>();

        public override bool Equals(object obj) {

            if (ReferenceEquals(this, obj)) return true;
            if (ReferenceEquals(null, obj)) return false;
            if (GetType() != obj.GetType()) return false;
            var other = obj as ValueObject;
            return other != null && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        public override int GetHashCode() {

            unchecked {

                return GetEqualityComponents().Aggregate(17, (current, obj) => current * 23 + (obj?.GetHashCode() ?? 0));
            }
        }

        public static bool operator ==(ValueObject left, ValueObject right) {

            return Equals(left, right);
        }

        public static bool operator !=(ValueObject left, ValueObject right) {

            return !Equals(left, right);
        }

        public override string ToString() {

            return $"{{{string.Join(", ", GetProperties().Select(f => $"{f.Name}: {f.GetValue(this)}"))}}}";
        }

        protected virtual IEnumerable<object> GetEqualityComponents() {

            Type t = GetType();
            return GetProperties().Select(x => x.GetValue(this));
        }

        protected virtual IEnumerable<PropertyInfo> GetProperties() {

            return _typeProperties.GetOrAdd(GetType(),
                t => t.GetRuntimeProperties()
                    .OrderBy(p => p.Name)
                    .ToList());
        }
    }
}
