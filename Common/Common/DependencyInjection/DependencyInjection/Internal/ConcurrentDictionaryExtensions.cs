using System;
using PZMMENYI.Collections.Concurrent;

namespace PZMMENYI.DependencyInjection.Internal {

    internal static class ConcurrentDictionaryExtensions {
        
        /// <summary>
        /// 获得或添加。
        /// </summary>
        /// <typeparam name="TKey">主键类型。</typeparam>
        /// <typeparam name="TValue">值类型。</typeparam>
        /// <typeparam name="TArg">参数类型。</typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key">主键值。</param>
        /// <param name="valurFactoty">值工厂（<see cref="Func{TKey,TArg,TValue }"/>）。</param>
        /// <param name="arg">参数值。</param>
        /// <returns></returns>
        internal static TValue GetOrAdd<TKey, TValue, TArg>(this ConcurrentDictionary<TKey, TValue> dictionary,
                                                          TKey key,
                                                          Func<TKey, TArg, TValue> valurFactoty,
                                                          TArg arg) {
            if(dictionary == null) {
                throw new ArgumentNullException(nameof(dictionary));
            }
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }
            if (valurFactoty == null) {
                throw new ArgumentNullException(nameof(valurFactoty));
            }

            while (true) {

                TValue value;
                if (dictionary.TryGetValue(key, out value)) {

                    return value;
                }
                value = valurFactoty(key, arg);
                if (dictionary.TryAdd(key, value)) {

                    return value;
                }

            }
        }
    }
}
