using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZMMENYI.DependencyInjection {
    /// <summary>
    /// <see cref="IServiceProvider"/>扩展，主要是GetService。
    /// </summary>
    public static class ServiceProviderServiceExtension {

        /// <summary>
        /// 通过<see cref="IServiceProvider"/>创建<paramref name="T"/>类型的实例。
        /// </summary>
        /// <typeparam name="T">实例类型。</typeparam>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static T GetService<T>(this IServiceProvider provider) {

            if (provider == null) {
                throw new ArgumentNullException(nameof(provider));
            }
            return (T)provider.GetService(typeof(T));
        }

        /// <summary>
        /// 通过<see cref="IServiceProvider"/>创建<paramref name="T"/>类型的实例列表。
        /// </summary>
        /// <typeparam name="T">实例类型。</typeparam>
        /// <param name="provider"></param>
        /// <returns><paramref name="serviceType"/>类型的实例<see cref="IEnumerable{T}"/>列表。</returns>
        public static IEnumerable<T> GetServices<T>(this IServiceProvider provider) {

            if (provider == null) {
                throw new ArgumentNullException(nameof(provider));
            }

            return provider.GetRequiredService<IEnumerable<T>>();
        }

        /// <summary>
        /// 通过<see cref="IServiceProvider"/>创建<paramref name="serviceType"/>类型的实例列表。
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="serviceType"></param>
        /// <returns><paramref name="serviceType"/>类型的实例<see cref="IEnumerable{T}"/>列表。</returns>
        public static IEnumerable<object> GetServices(this IServiceProvider provider, Type serviceType) {

            if (provider == null) {
                throw new ArgumentNullException(nameof(provider));
            }

            if (serviceType == null) {
                throw new ArgumentNullException(nameof(serviceType));
            }

            var genericEnumerable = typeof(IEnumerable<>).MakeGenericType(serviceType);
            return (IEnumerable<object>)provider.GetRequiredService(genericEnumerable);
        }

        /// <summary>
        /// 创建<see cref="IServiceScope"/>去用于解析一个范围服务。
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static IServiceScope CreateScope(this IServiceProvider provider) {

            return provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        }


        /// <summary>
        /// 通过<see cref="IServiceProvider"/>获得<see cref="{T}"/>的类型实例。
        /// </summary>
        /// <typeparam name="T">实例的类型。</typeparam>
        /// <param name="provider"></param>
        /// <returns><see cref="{T}"/>类型实例。</returns>
        public static T GetRequiredService<T>(this IServiceProvider provider) {

            if (provider == null) {
                throw new ArgumentNullException(nameof(provider));
            }

            return (T)provider.GetRequiredService(typeof(T));
        }

        /// <summary>
        /// 通过<see cref="IServiceProvider"/>获得<paramref name="serviceType"/>的类型实例。
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="serviceType"></param>
        /// <returns><paramref name="serviceType"/>类型实例。</returns>
        public static object GetRequiredService(this IServiceProvider provider, Type serviceType) {

            if (provider == null) {
                throw new ArgumentNullException(nameof(provider));
            }

            if (serviceType == null) {
                throw new ArgumentNullException(nameof(serviceType));
            }

            var requiredServiceSupportingProvider = provider as ISupportRequiredService;
            if (requiredServiceSupportingProvider != null) {

                return requiredServiceSupportingProvider.GetRequiredService(serviceType);
            }

            var service = provider.GetService(serviceType);
            if (service == null) {
                throw new InvalidOperationException("serviceType类型没有被注册。");
            }

            return service;
        }
    }
}
