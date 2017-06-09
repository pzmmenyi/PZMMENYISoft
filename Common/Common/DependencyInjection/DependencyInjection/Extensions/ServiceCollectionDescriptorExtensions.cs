using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZMMENYI.DependencyInjection.Extensions {
    /// <summary>
    /// <see cref="IServiceCollection"/>扩展，通过把<see cref="ServiceDescriptor"/>加入其自身。
    /// 使用参数构造<see cref="ServiceDescriptor"/>并加入到<see cref="IServiceCollection"/>。
    /// 方法内未指定<see cref="ServiceLifetime"/>枚举值。
    /// </summary>
    public static class ServiceCollectionDescriptorExtension {
        /// <summary>
        /// 添加<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        public static IServiceCollection Add(this IServiceCollection collection, ServiceDescriptor descriptor) {

            if (collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }

            if (descriptor == null) {
                throw new ArgumentNullException(nameof(descriptor));
            }

            collection.Add(descriptor);
            return collection;
        }
        /// <summary>
        /// 添加<see cref="ServiceDescriptor"/>集合到<see cref="IServiceCollection"/>中。
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="descriptors"></param>
        /// <returns></returns>
        public static IServiceCollection Adds(this IServiceCollection collection, IEnumerable<ServiceDescriptor> descriptors) {

            if (collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }

            if (descriptors == null) {
                throw new ArgumentNullException(nameof(descriptors));
            }

            foreach (var descriptor in descriptors) {

                collection.Add(descriptor);
            }

            return collection;
        }
        /// <summary>
        /// 试图添加<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        /// 添加前检测是否存在。
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="descriptor"></param>
        public static void TryAdd(this IServiceCollection collection, ServiceDescriptor descriptor) {

            if (collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }

            if (descriptor == null) {
                throw new ArgumentNullException(nameof(descriptor));
            };

            if (!collection.Any(d => d.ServiceType == descriptor.ServiceType)) {

                collection.Add(descriptor);
            }
        }
        /// <summary>
        ///  试图添加<see cref="ServiceDescriptor"/>集合到<see cref="IServiceCollection"/>中。
        ///  添加前检测是否存在。
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="descriptors"></param>
        public static void TryAdds(this IServiceCollection collection, IEnumerable<ServiceDescriptor> descriptors) {

            if (collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }

            if (descriptors == null) {
                throw new ArgumentNullException(nameof(descriptors));
            }

            foreach (var d in descriptors) {

                collection.TryAdd(d);
            }
        }
        /// <summary>
        /// 试图添加<see cref="ServiceLifetime.Transient"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        ///添加前检测是否存在。
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="service"></param>
        public static void TryAddTransient(this IServiceCollection collection, Type service) {

            if (collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }

            if (service == null) {
                throw new ArgumentNullException(nameof(service));
            }

            var descriptor = ServiceDescriptor.Transient(service, service);
            TryAdd(collection, descriptor);
        }
        /// <summary>
        /// 试图添加<see cref="ServiceLifetime.Transient"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        ///添加前检测是否存在。
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="service"></param>
        /// <param name="implementationType"></param>
        public static void TryAddTransient(this IServiceCollection collection, Type service, Type implementationType) {

            if (collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }

            if (service == null) {
                throw new ArgumentNullException(nameof(service));
            }

            if (implementationType == null) {
                throw new ArgumentNullException(nameof(implementationType));
            }

            var descriptor = ServiceDescriptor.Transient(service, implementationType);
            TryAdd(collection, descriptor);
        }
        /// <summary>
        /// 试图添加<see cref="ServiceLifetime.Transient"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        ///添加前检测是否存在。
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="service"></param>
        /// <param name="implementationFactory"></param>
        public static void TryAddTransient(this IServiceCollection collection,
                                           Type service,
                                           Func<IServiceProvider, object> implementationFactory) {

            if (collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }

            if (service == null) {
                throw new ArgumentNullException(nameof(service));
            }

            if (implementationFactory == null) {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            var descriptor = ServiceDescriptor.Transient(service, implementationFactory);
            TryAdd(collection, descriptor);
        }
        /// <summary>
        /// 试图添加<see cref="ServiceLifetime.Transient"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        ///添加前检测是否存在。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="collection"></param>
        public static void TryAddTransient<TService>(this IServiceCollection collection)
                                           where TService : class {

            if (collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }

            TryAddTransient(collection, typeof(TService), typeof(TService));
        }
        /// <summary>
        /// 试图添加<see cref="ServiceLifetime.Transient"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        ///添加前检测是否存在。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="collection"></param>
        public static void TryAddTransient<TService, TImplementation>(this IServiceCollection collection)
                                           where TService : class
                                           where TImplementation : class, TService {

            if (collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }

            TryAddTransient(collection, typeof(TService), typeof(TImplementation));
        }
        /// <summary>
        /// 试图添加<see cref="ServiceLifetime.Transient"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        ///添加前检测是否存在。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="services"></param>
        /// <param name="implementationFactory"></param>
        public static void TryAddTransient<TService>(this IServiceCollection services,
                                                     Func<IServiceProvider, TService> implementationFactory)
                                                     where TService : class {

            services.TryAdd(ServiceDescriptor.Transient(implementationFactory));
        }
        /// <summary>
        /// 试图添加<see cref="ServiceLifetime.Scoped"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        ///添加前检测是否存在。
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="service"></param>
        public static void TryAddScoped(this IServiceCollection collection, Type service) {

            if (collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }

            if (service == null) {
                throw new ArgumentNullException(nameof(service));
            }

            var descriptor = ServiceDescriptor.Scoped(service, service);
            TryAdd(collection, descriptor);
        }
        /// <summary>
        /// 试图添加<see cref="ServiceLifetime.Scoped"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        ///添加前检测是否存在。
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="service"></param>
        /// <param name="implementationType"></param>
        public static void TryAddScoped(this IServiceCollection collection, Type service, Type implementationType) {

            if (collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }

            if (service == null) {
                throw new ArgumentNullException(nameof(service));
            }

            if (implementationType == null) {
                throw new ArgumentNullException(nameof(implementationType));
            }

            var descriptor = ServiceDescriptor.Scoped(service, implementationType);
            TryAdd(collection, descriptor);
        }
        /// <summary>
        /// 试图添加<see cref="ServiceLifetime.Scoped"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        ///添加前检测是否存在。
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="service"></param>
        /// <param name="implementationFactory"></param>
        public static void TryAddScoped(this IServiceCollection collection,
                                        Type service,
                                        Func<IServiceProvider, object> implementationFactory) {

            if (collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }

            if (service == null) {
                throw new ArgumentNullException(nameof(service));
            }

            if (implementationFactory == null) {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            var descriptor = ServiceDescriptor.Scoped(service, implementationFactory);
            TryAdd(collection, descriptor);
        }
        /// <summary>
        /// 试图添加<see cref="ServiceLifetime.Scoped"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        ///添加前检测是否存在。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="collection"></param>
        public static void TryAddScoped<TService>(this IServiceCollection collection)
                                        where TService : class {

            if (collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }

            TryAddScoped(collection, typeof(TService), typeof(TService));
        }
        /// <summary>
        /// 试图添加<see cref="ServiceLifetime.Scoped"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        ///添加前检测是否存在。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="collection"></param>
        public static void TryAddScoped<TService, TImplementation>(this IServiceCollection collection)
                                        where TService : class
                                        where TImplementation : class, TService {

            if (collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }

            TryAddScoped(collection, typeof(TService), typeof(TImplementation));
        }
        /// <summary>
        /// 试图添加<see cref="ServiceLifetime.Scoped"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        ///添加前检测是否存在。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="services"></param>
        /// <param name="implementationFactory"></param>
        public static void TryAddScoped<TService>(this IServiceCollection services,
                                                  Func<IServiceProvider, TService> implementationFactory)
                                        where TService : class {

            services.TryAdd(ServiceDescriptor.Scoped(implementationFactory));
        }
        /// <summary>
        /// 试图添加<see cref="ServiceLifetime.Singleton"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        ///添加前检测是否存在。
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="service"></param>
        public static void TryAddSingleton(this IServiceCollection collection, Type service) {

            if (collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }

            if (service == null) {
                throw new ArgumentNullException(nameof(service));
            }

            var descriptor = ServiceDescriptor.Singleton(service, service);
            TryAdd(collection, descriptor);
        }
        /// <summary>
        /// 试图添加<see cref="ServiceLifetime.Singleton"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        ///添加前检测是否存在。
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="service"></param>
        /// <param name="implementationType"></param>
        public static void TryAddSingleton(this IServiceCollection collection, Type service, Type implementationType) {

            if (collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }

            if (service == null) {
                throw new ArgumentNullException(nameof(service));
            }

            if (implementationType == null) {
                throw new ArgumentNullException(nameof(implementationType));
            }

            var descriptor = ServiceDescriptor.Singleton(service, implementationType);
            TryAdd(collection, descriptor);
        }
        /// <summary>
        /// 试图添加<see cref="ServiceLifetime.Singleton"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        ///添加前检测是否存在。
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="service"></param>
        /// <param name="implementationFactory"></param>
        public static void TryAddSingleton(this IServiceCollection collection,
                                           Type service,
                                           Func<IServiceProvider, object> implementationFactory) {

            if (collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }

            if (service == null) {
                throw new ArgumentNullException(nameof(service));
            }

            if (implementationFactory == null) {
                throw new ArgumentNullException(nameof(implementationFactory));
            }

            var descriptor = ServiceDescriptor.Singleton(service, implementationFactory);
            TryAdd(collection, descriptor);
        }
        /// <summary>
        /// 试图添加<see cref="ServiceLifetime.Singleton"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        ///添加前检测是否存在。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="collection"></param>
        public static void TryAddSingleton<TService>(this IServiceCollection collection)
                                           where TService : class {

            if (collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }

            TryAddSingleton(collection, typeof(TService), typeof(TService));
        }
        /// <summary>
        /// 试图添加<see cref="ServiceLifetime.Singleton"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        ///添加前检测是否存在。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="collection"></param>
        public static void TryAddSingleton<TService, TImplementation>(this IServiceCollection collection)
                                           where TService : class
                                           where TImplementation : class, TService {

            if (collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }

            TryAddSingleton(collection, typeof(TService), typeof(TImplementation));
        }
        /// <summary>
        /// 试图添加<see cref="ServiceLifetime.Singleton"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        ///添加前检测是否存在。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="collection"></param>
        /// <param name="instance"></param>
        public static void TryAddSingleton<TService>(this IServiceCollection collection, TService instance)
                                           where TService : class {

            if (collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }

            if (instance == null) {
                throw new ArgumentNullException(nameof(instance));
            }

            var descriptor = ServiceDescriptor.Singleton(typeof(TService), instance);
            TryAdd(collection, descriptor);
        }
        /// <summary>
        /// 试图添加<see cref="ServiceLifetime.Singleton"/>属性的<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        ///添加前检测是否存在。
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="services"></param>
        /// <param name="implementationFactory"></param>
        public static void TryAddSingleton<TService>(this IServiceCollection services,
                                                     Func<IServiceProvider, TService> implementationFactory)
                                           where TService : class {

            services.TryAdd(ServiceDescriptor.Singleton(implementationFactory));
        }
        /// <summary>
        /// 试图添加<see cref="ServiceDescriptor"/>集合到<see cref="IServiceCollection"/>中。
        ///添加前检测是否存在。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="descriptor"></param>
        public static void TryAddEnumerable(this IServiceCollection services, ServiceDescriptor descriptor) {

            if (descriptor == null) {
                throw new ArgumentNullException(nameof(descriptor));
            }

            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }

            var implementationType = descriptor.GetImplementationType();

            if (implementationType == typeof(object) ||
                implementationType == descriptor.ServiceType) {
                throw new ArgumentException(nameof(descriptor),"加入实例类型不可分辨。");
            }

            if (!services.Any(d =>
                              d.ServiceType == descriptor.ServiceType &&
                              d.GetImplementationType() == implementationType)) {

                services.Add(descriptor);
            }
        }
        /// <summary>
        /// 试图添加<see cref="ServiceDescriptor"/>集合到<see cref="IServiceCollection"/>中。
        ///添加前检测是否存在。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="descriptors"></param>
        public static void TryAddEnumerable(this IServiceCollection services, IEnumerable<ServiceDescriptor> descriptors) {

            if (descriptors == null) {
                throw new ArgumentNullException(nameof(descriptors));
            }

            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }

            foreach (var d in descriptors) {

                services.TryAddEnumerable(d);
            }
        }
        /// <summary>
        /// 试图替换<see cref="ServiceDescriptor"/>到<see cref="IServiceCollection"/>中。
        ///添加前检测是否存在。
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        public static IServiceCollection Replace(this IServiceCollection collection, ServiceDescriptor descriptor) {

            if (descriptor == null) {
                throw new ArgumentNullException(nameof(descriptor));
            }

            if (collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }

            var registeredServiceDescriptor = collection.FirstOrDefault(s => s.ServiceType == descriptor.ServiceType);
            if (registeredServiceDescriptor != null) {

                collection.Remove(registeredServiceDescriptor);
            }

            collection.Add(descriptor);
            return collection;
        }
    }
}
